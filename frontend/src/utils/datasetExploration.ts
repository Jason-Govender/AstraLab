import {
  DEFAULT_DATASET_EXPLORATION_CORRELATION_COLUMN_COUNT,
  DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
  DEFAULT_DATASET_EXPLORATION_SCATTER_POINT_COUNT,
  DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
} from "@/constants/datasetExploration";
import type {
  BarChartCategory,
  CorrelationAnalysis,
  DatasetBarChartRequest,
  DatasetCorrelationRequest,
  DatasetDistributionRequest,
  DatasetExplorationColumn,
  DatasetExplorationWorkspaceTab,
  DatasetHistogramRequest,
  DatasetScatterPlotRequest,
  DistributionAnalysis,
} from "@/types/datasets";

export const getDatasetExplorationTabLabel = (
  tab: DatasetExplorationWorkspaceTab,
): string => {
  switch (tab) {
    case "rows":
      return "Rows";
    case "distribution":
      return "Distribution";
    case "histogram":
      return "Histogram";
    case "barChart":
      return "Bar Chart";
    case "scatterPlot":
      return "Scatter Plot";
    case "correlation":
      return "Correlation";
    default:
      return "Explore";
  }
};

export const getNumericExplorationColumns = (
  columns: DatasetExplorationColumn[],
): DatasetExplorationColumn[] => columns.filter((column) => column.isNumeric);

export const getCategoricalExplorationColumns = (
  columns: DatasetExplorationColumn[],
): DatasetExplorationColumn[] =>
  columns.filter((column) => column.isCategorical);

export const getDistributionEligibleColumns = (
  columns: DatasetExplorationColumn[],
): DatasetExplorationColumn[] =>
  columns.filter((column) => column.isNumeric || column.isCategorical);

export const buildDefaultHistogramRequest = (
  datasetVersionId: number,
  columns: DatasetExplorationColumn[],
): DatasetHistogramRequest | undefined => {
  const firstNumericColumn = getNumericExplorationColumns(columns)[0];

  if (!firstNumericColumn) {
    return undefined;
  }

  return {
    datasetVersionId,
    datasetColumnId: firstNumericColumn.datasetColumnId,
    bucketCount: DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
  };
};

export const buildDefaultBarChartRequest = (
  datasetVersionId: number,
  columns: DatasetExplorationColumn[],
): DatasetBarChartRequest | undefined => {
  const firstCategoricalColumn = getCategoricalExplorationColumns(columns)[0];

  if (!firstCategoricalColumn) {
    return undefined;
  }

  return {
    datasetVersionId,
    datasetColumnId: firstCategoricalColumn.datasetColumnId,
    topCategoryCount: DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
  };
};

export const buildDefaultScatterPlotRequest = (
  datasetVersionId: number,
  columns: DatasetExplorationColumn[],
): DatasetScatterPlotRequest | undefined => {
  const numericColumns = getNumericExplorationColumns(columns);

  if (numericColumns.length < 2) {
    return undefined;
  }

  return {
    datasetVersionId,
    xDatasetColumnId: numericColumns[0].datasetColumnId,
    yDatasetColumnId: numericColumns[1].datasetColumnId,
    maxPointCount: DEFAULT_DATASET_EXPLORATION_SCATTER_POINT_COUNT,
  };
};

export const buildDefaultDistributionRequest = (
  datasetVersionId: number,
  columns: DatasetExplorationColumn[],
): DatasetDistributionRequest | undefined => {
  const eligibleColumns = getDistributionEligibleColumns(columns);
  const defaultColumn = eligibleColumns[0];

  if (!defaultColumn) {
    return undefined;
  }

  return {
    datasetVersionId,
    datasetColumnId: defaultColumn.datasetColumnId,
    bucketCount: DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
    topCategoryCount: DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
  };
};

export const buildDefaultCorrelationRequest = (
  datasetVersionId: number,
  columns: DatasetExplorationColumn[],
): DatasetCorrelationRequest | undefined => {
  const numericColumns = getNumericExplorationColumns(columns);

  if (numericColumns.length < 2) {
    return undefined;
  }

  return {
    datasetVersionId,
    datasetColumnIds: numericColumns
      .slice(0, DEFAULT_DATASET_EXPLORATION_CORRELATION_COLUMN_COUNT)
      .map((column) => column.datasetColumnId),
  };
};

export const isDistributionNumeric = (
  distribution?: DistributionAnalysis | null,
): boolean => Boolean(distribution && distribution.buckets.length > 0);

export const buildCorrelationHeatmapData = (
  correlation?: CorrelationAnalysis | null,
): Array<{
  xColumnName: string;
  yColumnName: string;
  coefficient: number;
}> => {
  if (!correlation) {
    return [];
  }

  return correlation.pairs.map((pair) => ({
    xColumnName: pair.xColumnName,
    yColumnName: pair.yColumnName,
    coefficient: pair.coefficient ?? 0,
  }));
};

export const getCorrelationCoefficientTone = (
  value?: number | null,
): "success" | "processing" | "warning" | "error" | "default" => {
  if (value === null || value === undefined) {
    return "default";
  }

  const absoluteValue = Math.abs(value);

  if (absoluteValue >= 0.8) {
    return "success";
  }

  if (absoluteValue >= 0.5) {
    return "processing";
  }

  if (absoluteValue >= 0.3) {
    return "warning";
  }

  return "error";
};

export const getCategoryChartData = (
  categories: BarChartCategory[],
): Array<{ label: string; count: number }> =>
  categories.map((category) => ({
    label: category.label,
    count: category.count,
  }));
