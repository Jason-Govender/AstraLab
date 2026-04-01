from __future__ import annotations

import json
from typing import Any, Literal

from pydantic import BaseModel, Field, field_validator, model_validator


TaskType = Literal["classification", "regression", "clustering", "anomaly_detection"]


class JobColumn(BaseModel):
    dataset_column_id: int = Field(alias="datasetColumnId")
    name: str
    data_type: str | None = Field(default=None, alias="dataType")
    ordinal: int


class JobRequest(BaseModel):
    experiment_id: int = Field(alias="experimentId")
    tenant_id: int = Field(alias="tenantId")
    dataset_version_id: int = Field(alias="datasetVersionId")
    dataset_format: Literal["csv", "json"] = Field(alias="datasetFormat")
    dataset_download_url: str = Field(alias="datasetDownloadUrl")
    task_type: TaskType = Field(alias="taskType")
    algorithm_key: str = Field(alias="algorithmKey")
    training_configuration_json: str = Field(alias="trainingConfigurationJson")
    artifact_upload_url: str = Field(alias="artifactUploadUrl")
    artifact_storage_provider: str = Field(alias="artifactStorageProvider")
    artifact_storage_key: str = Field(alias="artifactStorageKey")
    feature_columns: list[JobColumn] = Field(alias="featureColumns", min_length=1)
    target_column: JobColumn | None = Field(default=None, alias="targetColumn")
    completed_callback_url: str = Field(alias="completedCallbackUrl")
    failed_callback_url: str = Field(alias="failedCallbackUrl")

    @field_validator("algorithm_key")
    @classmethod
    def validate_algorithm_key(cls, value: str) -> str:
        normalized = value.strip()
        if not normalized:
            raise ValueError("An algorithm key is required.")

        return normalized

    @model_validator(mode="after")
    def validate_target_column(self) -> "JobRequest":
        requires_target = self.task_type in ("classification", "regression")
        if requires_target and self.target_column is None:
            raise ValueError("A target column is required for supervised jobs.")

        if not requires_target and self.target_column is not None:
            raise ValueError("A target column is not supported for this job type.")

        feature_column_ids = {column.dataset_column_id for column in self.feature_columns}
        if self.target_column and self.target_column.dataset_column_id in feature_column_ids:
            raise ValueError("The target column cannot also be used as a feature column.")

        return self

    def parse_training_configuration(self) -> dict[str, Any]:
        if not self.training_configuration_json.strip():
            return {}

        parsed = json.loads(self.training_configuration_json)
        if not isinstance(parsed, dict):
            raise ValueError("The training configuration payload must deserialize to an object.")

        return parsed


class JobAcceptedResponse(BaseModel):
    experiment_id: int = Field(alias="experimentId")
    status: str


class CompletionMetric(BaseModel):
    metric_name: str = Field(alias="metricName")
    metric_value: float = Field(alias="metricValue")


class CompletionFeatureImportance(BaseModel):
    dataset_column_id: int = Field(alias="datasetColumnId")
    importance_score: float = Field(alias="importanceScore")
    rank: int


class CompletionCallbackPayload(BaseModel):
    experiment_id: int = Field(alias="experimentId")
    started_at_utc: str | None = Field(default=None, alias="startedAtUtc")
    completed_at_utc: str | None = Field(default=None, alias="completedAtUtc")
    model_type: str = Field(alias="modelType")
    artifact_storage_provider: str = Field(alias="artifactStorageProvider")
    artifact_storage_key: str = Field(alias="artifactStorageKey")
    performance_summary_json: str = Field(alias="performanceSummaryJson")
    warnings_json: str | None = Field(default=None, alias="warningsJson")
    metrics: list[CompletionMetric]
    feature_importances: list[CompletionFeatureImportance] = Field(alias="featureImportances")


class FailureCallbackPayload(BaseModel):
    experiment_id: int = Field(alias="experimentId")
    started_at_utc: str | None = Field(default=None, alias="startedAtUtc")
    completed_at_utc: str | None = Field(default=None, alias="completedAtUtc")
    failure_message: str = Field(alias="failureMessage")
    warnings_json: str | None = Field(default=None, alias="warningsJson")
