import {
  DatasetColumn,
  DatasetColumnInsight,
  DatasetColumnProfile,
  DatasetColumnStatistics,
  DatasetFormat,
  DatasetProfile,
  DatasetProfileSummaryPayload,
  DatasetSchema,
  DatasetStatus,
  DatasetVersionStatus,
  DatasetVersionType,
  UploadedRawDatasetResult,
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

export const formatNumber = (
  value?: number | null,
  maximumFractionDigits = 2,
): string => {
  if (value === null || value === undefined) {
    return "Unavailable";
  }

  return new Intl.NumberFormat("en", {
    maximumFractionDigits,
  }).format(value);
};

export const formatPercentage = (
  value?: number | null,
  maximumFractionDigits = 1,
): string => {
  if (value === null || value === undefined) {
    return "Unavailable";
  }

  return `${formatNumber(value, maximumFractionDigits)}%`;
};

export const getDataHealthScoreTone = (value?: number | null) => {
  if (value === null || value === undefined) {
    return "default";
  }

  if (value >= 85) {
    return "success";
  }

  if (value >= 60) {
    return "processing";
  }

  return "error";
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

export const parseDatasetProfileSummary = (
  summaryJson?: string | null,
): DatasetProfileSummaryPayload | null => {
  if (!summaryJson) {
    return null;
  }

  try {
    return JSON.parse(summaryJson) as DatasetProfileSummaryPayload;
  } catch {
    return null;
  }
};

export const parseDatasetColumnStatistics = (
  statisticsJson?: string | null,
): DatasetColumnStatistics | null => {
  if (!statisticsJson) {
    return null;
  }

  try {
    return JSON.parse(statisticsJson) as DatasetColumnStatistics;
  } catch {
    return null;
  }
};

export interface DatasetProfileOverview {
  rowCount?: number | null;
  duplicateRowCount?: number | null;
  dataHealthScore?: number | null;
  totalNullCount?: number | null;
  overallNullPercentage?: number | null;
  totalAnomalyCount?: number | null;
  overallAnomalyPercentage?: number | null;
  creationTime?: string | null;
  profiledColumnCount?: number | null;
}

export const getDatasetProfileOverview = (
  profile?: DatasetProfile | null,
): DatasetProfileOverview | null => {
  if (!profile) {
    return null;
  }

  const summary = parseDatasetProfileSummary(profile.summaryJson);

  return {
    rowCount: profile.rowCount,
    duplicateRowCount: profile.duplicateRowCount,
    dataHealthScore: profile.dataHealthScore,
    totalNullCount: summary?.totalNullCount,
    overallNullPercentage: summary?.overallNullPercentage,
    totalAnomalyCount: summary?.totalAnomalyCount,
    overallAnomalyPercentage: summary?.overallAnomalyPercentage,
    creationTime: profile.creationTime,
    profiledColumnCount: profile.columnProfiles.length,
  };
};

export const getUploadProfileOverview = (
  uploadResult?: UploadedRawDatasetResult | null,
): DatasetProfileOverview | null => {
  if (!uploadResult) {
    return null;
  }

  return {
    rowCount: uploadResult.rowCount,
    duplicateRowCount: uploadResult.duplicateRowCount,
    dataHealthScore: uploadResult.dataHealthScore,
    profiledColumnCount: uploadResult.columnProfiles.length || uploadResult.columnCount,
  };
};

export const buildDatasetColumnInsights = (
  columns: DatasetColumn[],
  columnProfiles: DatasetColumnProfile[],
): DatasetColumnInsight[] => {
  const profileMap = new Map(
    columnProfiles.map((columnProfile) => [
      columnProfile.datasetColumnId,
      columnProfile,
    ]),
  );

  return columns
    .map((column) => {
      const columnProfile = profileMap.get(column.id);
      const statistics = parseDatasetColumnStatistics(columnProfile?.statisticsJson);

      return {
        datasetColumnId: column.id,
        columnProfileId: columnProfile?.id ?? 0,
        name: column.name,
        ordinal: column.ordinal,
        inferredDataType: columnProfile?.inferredDataType || column.dataType,
        nullCount: columnProfile?.nullCount ?? column.nullCount ?? 0,
        nullPercentage:
          columnProfile?.nullPercentage ?? statistics?.nullPercentage ?? 0,
        distinctCount:
          columnProfile?.distinctCount ?? column.distinctCount ?? undefined,
        mean: statistics?.mean,
        min: statistics?.min,
        max: statistics?.max,
        anomalyCount: statistics?.anomalyCount ?? 0,
        anomalyPercentage: statistics?.anomalyPercentage ?? 0,
        hasAnomalies: statistics?.hasAnomalies ?? false,
        creationTime: columnProfile?.creationTime ?? column.creationTime,
      };
    })
    .sort((leftColumn, rightColumn) => leftColumn.ordinal - rightColumn.ordinal);
};
