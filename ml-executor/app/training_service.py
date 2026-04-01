from __future__ import annotations

import json
from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Any

import numpy as np
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.compose import ColumnTransformer
from sklearn.ensemble import IsolationForest, RandomForestClassifier, RandomForestRegressor
from sklearn.impute import SimpleImputer
from sklearn.linear_model import LinearRegression, LogisticRegression
from sklearn.metrics import (
    accuracy_score,
    f1_score,
    mean_absolute_error,
    mean_squared_error,
    precision_score,
    r2_score,
    recall_score,
    silhouette_score,
)
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder

from .dataset_loader import load_dataset
from .schemas import JobColumn, JobRequest
from .settings import ExecutorSettings


SUPPORTED_ALGORITHMS = {
    "classification": {"logistic_regression", "random_forest_classifier"},
    "regression": {"linear_regression", "random_forest_regressor"},
    "clustering": {"kmeans"},
    "anomaly_detection": {"isolation_forest"},
}

NUMERIC_DATA_TYPES = {
    "integer",
    "int",
    "long",
    "decimal",
    "double",
    "float",
    "number",
    "boolean",
    "bool",
}


@dataclass
class TrainingResult:
    started_at_utc: datetime
    completed_at_utc: datetime
    model_type: str
    performance_summary_json: str
    warnings_json: str | None
    metrics: list[dict[str, float]]
    feature_importances: list[dict[str, float | int]]
    artifact_bundle: dict[str, Any]


def train_job(job: JobRequest, settings: ExecutorSettings) -> TrainingResult:
    validate_algorithm(job)
    started_at_utc = datetime.now(timezone.utc)
    configuration = job.parse_training_configuration()
    warnings: list[str] = []

    dataframe = load_dataset(job.dataset_download_url, job.dataset_format, settings.http_timeout_seconds)
    dataframe = dataframe.copy()

    feature_names = [column.name for column in sorted(job.feature_columns, key=lambda item: item.ordinal)]
    missing_columns = [column_name for column_name in feature_names if column_name not in dataframe.columns]
    if missing_columns:
        raise ValueError(f"The dataset is missing requested feature columns: {', '.join(missing_columns)}.")

    feature_frame = dataframe[feature_names]
    feature_frame = coerce_feature_frame(feature_frame, job.feature_columns)
    preprocessor = build_preprocessor(feature_frame)
    estimator = build_estimator(job.task_type, job.algorithm_key, configuration)
    pipeline = Pipeline(
        steps=[
            ("preprocessor", preprocessor),
            ("model", estimator),
        ]
    )

    if job.task_type in ("classification", "regression"):
        if job.target_column is None:
            raise ValueError("A target column is required for supervised training.")

        if job.target_column.name not in dataframe.columns:
            raise ValueError(f"The dataset is missing the requested target column '{job.target_column.name}'.")

        target_series = dataframe[job.target_column.name]
        training_frame = feature_frame.copy()
        training_frame["_target"] = target_series
        training_frame = training_frame.dropna(subset=["_target"])

        if training_frame.empty:
            raise ValueError("No training rows remain after removing rows with null target values.")

        target_series = training_frame.pop("_target")
        feature_frame = training_frame

        if job.task_type == "regression":
            target_series = pd.to_numeric(target_series, errors="coerce")
            non_null_mask = target_series.notna()
            target_series = target_series.loc[non_null_mask]
            feature_frame = feature_frame.loc[non_null_mask]

        if len(feature_frame.index) < 3:
            raise ValueError("At least three rows are required for supervised ML training.")

        test_size = clamp_float(configuration.get("testSize", 0.2), 0.1, 0.4)
        random_state = int(configuration.get("randomState", 42))
        stratify = target_series if job.task_type == "classification" and can_stratify(target_series) else None
        if job.task_type == "classification" and stratify is None:
            warnings.append("classification_not_stratified")

        x_train, x_test, y_train, y_test = train_test_split(
            feature_frame,
            target_series,
            test_size=test_size,
            random_state=random_state,
            stratify=stratify,
        )

        pipeline.fit(x_train, y_train)
        predictions = pipeline.predict(x_test)

        if job.task_type == "classification":
            metrics = [
                {"metricName": "accuracy", "metricValue": float(accuracy_score(y_test, predictions))},
                {"metricName": "precision_weighted", "metricValue": float(precision_score(y_test, predictions, average="weighted", zero_division=0))},
                {"metricName": "recall_weighted", "metricValue": float(recall_score(y_test, predictions, average="weighted", zero_division=0))},
                {"metricName": "f1_weighted", "metricValue": float(f1_score(y_test, predictions, average="weighted", zero_division=0))},
            ]
        else:
            metrics = [
                {"metricName": "rmse", "metricValue": float(np.sqrt(mean_squared_error(y_test, predictions)))},
                {"metricName": "mae", "metricValue": float(mean_absolute_error(y_test, predictions))},
                {"metricName": "r2", "metricValue": float(r2_score(y_test, predictions))},
            ]

        feature_importances = extract_feature_importances(pipeline, job.feature_columns)
        summary = {
            "rowCount": int(len(feature_frame.index)),
            "featureCount": len(feature_names),
            "taskType": job.task_type,
            "algorithmKey": job.algorithm_key,
            "testSize": test_size,
        }
    elif job.task_type == "clustering":
        cluster_count = int(configuration.get("clusterCount", 3))
        if cluster_count < 2:
            raise ValueError("Clustering requires at least two clusters.")

        pipeline.fit(feature_frame)
        transformed = pipeline.named_steps["preprocessor"].transform(feature_frame)
        labels = pipeline.named_steps["model"].labels_
        metrics = [
            {"metricName": "cluster_count", "metricValue": float(cluster_count)},
            {"metricName": "inertia", "metricValue": float(pipeline.named_steps["model"].inertia_)},
        ]

        if len(set(labels)) > 1 and len(feature_frame.index) > cluster_count:
            metrics.append({"metricName": "silhouette_score", "metricValue": float(silhouette_score(transformed, labels))})
        else:
            warnings.append("silhouette_not_available")

        feature_importances = []
        summary = {
            "rowCount": int(len(feature_frame.index)),
            "featureCount": len(feature_names),
            "taskType": job.task_type,
            "algorithmKey": job.algorithm_key,
        }
    else:
        pipeline.fit(feature_frame)
        predictions = pipeline.predict(feature_frame)
        anomaly_count = int(np.count_nonzero(predictions == -1))
        metrics = [
            {"metricName": "anomaly_count", "metricValue": float(anomaly_count)},
            {"metricName": "anomaly_ratio", "metricValue": float(anomaly_count / max(len(feature_frame.index), 1))},
        ]
        feature_importances = []
        summary = {
            "rowCount": int(len(feature_frame.index)),
            "featureCount": len(feature_names),
            "taskType": job.task_type,
            "algorithmKey": job.algorithm_key,
        }

    completed_at_utc = datetime.now(timezone.utc)
    warnings_json = json.dumps(warnings) if warnings else None
    artifact_bundle = {
        "model": pipeline,
        "summary": summary,
        "metrics": metrics,
        "featureImportances": feature_importances,
    }

    return TrainingResult(
        started_at_utc=started_at_utc,
        completed_at_utc=completed_at_utc,
        model_type=job.algorithm_key,
        performance_summary_json=json.dumps(summary),
        warnings_json=warnings_json,
        metrics=metrics,
        feature_importances=feature_importances,
        artifact_bundle=artifact_bundle,
    )


def validate_algorithm(job: JobRequest) -> None:
    if job.algorithm_key not in SUPPORTED_ALGORITHMS[job.task_type]:
        raise ValueError(f"The algorithm '{job.algorithm_key}' is not supported for task type '{job.task_type}'.")


def build_estimator(task_type: str, algorithm_key: str, configuration: dict[str, Any]) -> Any:
    random_state = int(configuration.get("randomState", 42))

    if task_type == "classification" and algorithm_key == "logistic_regression":
        return LogisticRegression(
            max_iter=int(configuration.get("maxIterations", 1000)),
            random_state=random_state,
        )

    if task_type == "classification" and algorithm_key == "random_forest_classifier":
        return RandomForestClassifier(
            n_estimators=int(configuration.get("numberOfEstimators", 200)),
            random_state=random_state,
        )

    if task_type == "regression" and algorithm_key == "linear_regression":
        return LinearRegression()

    if task_type == "regression" and algorithm_key == "random_forest_regressor":
        return RandomForestRegressor(
            n_estimators=int(configuration.get("numberOfEstimators", 200)),
            random_state=random_state,
        )

    if task_type == "clustering" and algorithm_key == "kmeans":
        return KMeans(
            n_clusters=int(configuration.get("clusterCount", 3)),
            n_init=10,
            random_state=random_state,
            max_iter=int(configuration.get("maxIterations", 300)),
        )

    if task_type == "anomaly_detection" and algorithm_key == "isolation_forest":
        return IsolationForest(
            n_estimators=int(configuration.get("numberOfEstimators", 200)),
            contamination=clamp_float(configuration.get("contamination", "auto"), 0.001, 0.5, allow_auto=True),
            random_state=random_state,
        )

    raise ValueError(f"Unsupported estimator '{algorithm_key}'.")


def build_preprocessor(feature_frame: pd.DataFrame) -> ColumnTransformer:
    numeric_columns = [column_name for column_name in feature_frame.columns if pd.api.types.is_numeric_dtype(feature_frame[column_name])]
    categorical_columns = [column_name for column_name in feature_frame.columns if column_name not in numeric_columns]

    transformers: list[tuple[str, Pipeline, list[str]]] = []

    if numeric_columns:
        transformers.append(
            (
                "numeric",
                Pipeline(steps=[("imputer", SimpleImputer(strategy="median"))]),
                numeric_columns,
            )
        )

    if categorical_columns:
        transformers.append(
            (
                "categorical",
                Pipeline(
                    steps=[
                        ("imputer", SimpleImputer(strategy="most_frequent")),
                        ("encoder", OneHotEncoder(handle_unknown="ignore")),
                    ]
                ),
                categorical_columns,
            )
        )

    return ColumnTransformer(transformers=transformers)


def coerce_feature_frame(feature_frame: pd.DataFrame, feature_columns: list[JobColumn]) -> pd.DataFrame:
    coerced = feature_frame.copy()
    for column in feature_columns:
        if (column.data_type or "").strip().lower() in NUMERIC_DATA_TYPES:
            coerced[column.name] = pd.to_numeric(coerced[column.name], errors="coerce")

    return coerced


def can_stratify(target_series: pd.Series) -> bool:
    value_counts = target_series.value_counts(dropna=True)
    return len(value_counts.index) > 1 and int(value_counts.min()) > 1


def extract_feature_importances(
    pipeline: Pipeline,
    feature_columns: list[JobColumn],
) -> list[dict[str, float | int]]:
    model = pipeline.named_steps["model"]
    preprocessor = pipeline.named_steps["preprocessor"]
    try:
        transformed_feature_names = preprocessor.get_feature_names_out()
    except AttributeError:
        transformed_feature_names = np.array([column.name for column in feature_columns])

    raw_importances: np.ndarray | None = None

    if hasattr(model, "feature_importances_"):
        raw_importances = np.asarray(model.feature_importances_, dtype=float)
    elif hasattr(model, "coef_"):
        coefficients = np.asarray(model.coef_, dtype=float)
        raw_importances = np.mean(np.abs(np.atleast_2d(coefficients)), axis=0)

    if raw_importances is None or len(raw_importances) != len(transformed_feature_names):
        return []

    aggregated_importances: dict[str, float] = {column.name: 0.0 for column in feature_columns}
    for transformed_name, importance_score in zip(transformed_feature_names, raw_importances):
        source_column = next(
            (
                column.name
                for column in feature_columns
                if transformed_name == column.name
                or transformed_name.endswith(f"__{column.name}")
                or transformed_name.startswith(f"categorical__{column.name}_")
            ),
            None,
        )
        if source_column is not None:
            aggregated_importances[source_column] += float(abs(importance_score))

    ranked_columns = sorted(
        (
            {
                "datasetColumnId": column.dataset_column_id,
                "importanceScore": aggregated_importances.get(column.name, 0.0),
            }
            for column in feature_columns
        ),
        key=lambda item: item["importanceScore"],
        reverse=True,
    )

    return [
        {
            "datasetColumnId": int(item["datasetColumnId"]),
            "importanceScore": float(item["importanceScore"]),
            "rank": index + 1,
        }
        for index, item in enumerate(ranked_columns)
        if item["importanceScore"] > 0
    ]


def clamp_float(value: Any, minimum: float, maximum: float, allow_auto: bool = False) -> float | str:
    if allow_auto and isinstance(value, str) and value == "auto":
        return value

    parsed = float(value)
    return max(minimum, min(maximum, parsed))
