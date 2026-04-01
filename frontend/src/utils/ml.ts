import { ML_ALGORITHM_OPTIONS } from "@/constants/ml";
import { MlExperimentStatus, MlTaskType } from "@/types/ml";

export const getMlTaskTypeLabel = (taskType: MlTaskType): string => {
  switch (taskType) {
    case MlTaskType.Classification:
      return "Classification";
    case MlTaskType.Regression:
      return "Regression";
    case MlTaskType.Clustering:
      return "Clustering";
    case MlTaskType.AnomalyDetection:
      return "Anomaly Detection";
    default:
      return "Unknown";
  }
};

export const getMlExperimentStatusLabel = (
  status: MlExperimentStatus,
): string => {
  switch (status) {
    case MlExperimentStatus.Pending:
      return "Pending";
    case MlExperimentStatus.Running:
      return "Running";
    case MlExperimentStatus.Completed:
      return "Completed";
    case MlExperimentStatus.Failed:
      return "Failed";
    case MlExperimentStatus.Cancelled:
      return "Cancelled";
    default:
      return "Unknown";
  }
};

export const getMlExperimentStatusColor = (status: MlExperimentStatus) => {
  switch (status) {
    case MlExperimentStatus.Completed:
      return "success";
    case MlExperimentStatus.Running:
      return "processing";
    case MlExperimentStatus.Failed:
      return "error";
    case MlExperimentStatus.Cancelled:
      return "default";
    case MlExperimentStatus.Pending:
    default:
      return "gold";
  }
};

export const getMlAlgorithmLabel = (algorithmKey: string): string =>
  ML_ALGORITHM_OPTIONS.find((option) => option.key === algorithmKey)?.label ||
  algorithmKey;

export const parseMlJsonObject = (
  value?: string | null,
): Record<string, unknown> | null => {
  if (!value) {
    return null;
  }

  try {
    const parsed = JSON.parse(value) as unknown;
    if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
      return parsed as Record<string, unknown>;
    }
  } catch {
    return null;
  }

  return null;
};

export const parseMlJsonArray = (value?: string | null): string[] => {
  if (!value) {
    return [];
  }

  try {
    const parsed = JSON.parse(value) as unknown;
    if (Array.isArray(parsed)) {
      return parsed.filter((item): item is string => typeof item === "string");
    }
  } catch {
    return [];
  }

  return [];
};

export const formatMlMetricLabel = (metricName: string): string =>
  metricName
    .split("_")
    .map((segment) => segment.charAt(0).toUpperCase() + segment.slice(1))
    .join(" ");

export const buildMlWorkspaceHref = (
  datasetId?: number,
  versionId?: number,
): string => {
  const searchParams = new URLSearchParams();

  if (datasetId) {
    searchParams.set("datasetId", String(datasetId));
  }

  if (versionId) {
    searchParams.set("versionId", String(versionId));
  }

  const queryString = searchParams.toString();

  return queryString ? `/ml-workspace?${queryString}` : "/ml-workspace";
};
