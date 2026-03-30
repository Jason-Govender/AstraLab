import {
  DatasetFormat,
  DatasetSchema,
  DatasetStatus,
  DatasetVersionStatus,
  DatasetVersionType,
} from "@/types/datasets";

const relativeTimeFormatter = new Intl.RelativeTimeFormat("en", {
  numeric: "auto",
});

export const getDatasetFormatLabel = (format: DatasetFormat): string => {
  switch (format) {
    case DatasetFormat.Csv:
      return "CSV";
    case DatasetFormat.Json:
      return "JSON";
    default:
      return "Unknown";
  }
};

export const getDatasetStatusLabel = (status: DatasetStatus): string => {
  switch (status) {
    case DatasetStatus.Uploaded:
      return "Uploaded";
    case DatasetStatus.Profiling:
      return "Profiling";
    case DatasetStatus.Ready:
      return "Ready";
    case DatasetStatus.Failed:
      return "Failed";
    case DatasetStatus.Archived:
      return "Archived";
    default:
      return "Unknown";
  }
};

export const getDatasetStatusColor = (status: DatasetStatus) => {
  switch (status) {
    case DatasetStatus.Ready:
      return "success";
    case DatasetStatus.Profiling:
      return "processing";
    case DatasetStatus.Failed:
      return "error";
    case DatasetStatus.Archived:
      return "default";
    case DatasetStatus.Uploaded:
    default:
      return "blue";
  }
};

export const getDatasetVersionStatusLabel = (
  status: DatasetVersionStatus,
): string => {
  switch (status) {
    case DatasetVersionStatus.Draft:
      return "Draft";
    case DatasetVersionStatus.Active:
      return "Active";
    case DatasetVersionStatus.Superseded:
      return "Superseded";
    case DatasetVersionStatus.Failed:
      return "Failed";
    default:
      return "Unknown";
  }
};

export const getDatasetVersionStatusColor = (
  status: DatasetVersionStatus,
) => {
  switch (status) {
    case DatasetVersionStatus.Active:
      return "success";
    case DatasetVersionStatus.Superseded:
      return "default";
    case DatasetVersionStatus.Failed:
      return "error";
    case DatasetVersionStatus.Draft:
    default:
      return "gold";
  }
};

export const getDatasetVersionTypeLabel = (
  versionType: DatasetVersionType,
): string => {
  switch (versionType) {
    case DatasetVersionType.Raw:
      return "Raw";
    case DatasetVersionType.Processed:
      return "Processed";
    default:
      return "Unknown";
  }
};

export const formatBytes = (value?: number | null): string => {
  if (!value || value <= 0) {
    return "0 B";
  }

  const units = ["B", "KB", "MB", "GB", "TB"];
  const unitIndex = Math.min(
    Math.floor(Math.log(value) / Math.log(1024)),
    units.length - 1,
  );
  const size = value / 1024 ** unitIndex;

  return `${size.toFixed(size >= 10 || unitIndex === 0 ? 0 : 1)} ${units[unitIndex]}`;
};

export const formatDateTime = (value?: string | null): string => {
  if (!value) {
    return "Unknown";
  }

  return new Intl.DateTimeFormat("en", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
};

export const formatRelativeTime = (value?: string | null): string => {
  if (!value) {
    return "Unknown";
  }

  const now = Date.now();
  const target = new Date(value).getTime();
  const diffInMinutes = Math.round((target - now) / (1000 * 60));

  if (Math.abs(diffInMinutes) < 60) {
    return relativeTimeFormatter.format(diffInMinutes, "minute");
  }

  const diffInHours = Math.round(diffInMinutes / 60);
  if (Math.abs(diffInHours) < 24) {
    return relativeTimeFormatter.format(diffInHours, "hour");
  }

  const diffInDays = Math.round(diffInHours / 24);
  return relativeTimeFormatter.format(diffInDays, "day");
};

export const parseDatasetSchema = (
  schemaJson?: string | null,
): DatasetSchema | null => {
  if (!schemaJson) {
    return null;
  }

  try {
    return JSON.parse(schemaJson) as DatasetSchema;
  } catch {
    return null;
  }
};
