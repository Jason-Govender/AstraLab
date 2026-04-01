import { MlTaskType, type MlAlgorithmOption } from "@/types/ml";

export const ML_EXPERIMENT_POLL_INTERVAL_MS = 8000;

export const ML_ALGORITHM_OPTIONS: MlAlgorithmOption[] = [
  {
    key: "logistic_regression",
    label: "Logistic Regression",
    taskType: MlTaskType.Classification,
    description: "Fast linear baseline for binary and multiclass classification.",
  },
  {
    key: "random_forest_classifier",
    label: "Random Forest Classifier",
    taskType: MlTaskType.Classification,
    description: "Tree ensemble baseline with feature importance support.",
  },
  {
    key: "linear_regression",
    label: "Linear Regression",
    taskType: MlTaskType.Regression,
    description: "Simple supervised baseline for numeric targets.",
  },
  {
    key: "random_forest_regressor",
    label: "Random Forest Regressor",
    taskType: MlTaskType.Regression,
    description: "Non-linear regression baseline with feature importance support.",
  },
  {
    key: "kmeans",
    label: "K-Means",
    taskType: MlTaskType.Clustering,
    description: "Cluster similar rows into a configurable number of groups.",
  },
  {
    key: "isolation_forest",
    label: "Isolation Forest",
    taskType: MlTaskType.AnomalyDetection,
    description: "Detect outliers based on tree isolation depth.",
  },
];
