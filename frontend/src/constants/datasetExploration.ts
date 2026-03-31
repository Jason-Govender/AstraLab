import { DatasetRowSortDirection, type DatasetExplorationWorkspaceTab } from "@/types/datasets";

export const DEFAULT_DATASET_EXPLORATION_ROW_PAGE_SIZE = 10;
export const DATASET_EXPLORATION_ROW_PAGE_SIZE_OPTIONS = ["10", "25", "50", "100"];
export const DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT = 10;
export const DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT = 10;
export const DEFAULT_DATASET_EXPLORATION_SCATTER_POINT_COUNT = 250;
export const DEFAULT_DATASET_EXPLORATION_CORRELATION_COLUMN_COUNT = 3;
export const DEFAULT_DATASET_EXPLORATION_TAB: DatasetExplorationWorkspaceTab = "rows";
export const DEFAULT_DATASET_EXPLORATION_SORT_DIRECTION =
  DatasetRowSortDirection.Ascending;

export const DATASET_EXPLORATION_TAB_OPTIONS: Array<{
  key: DatasetExplorationWorkspaceTab;
  label: string;
}> = [
  { key: "rows", label: "Rows" },
  { key: "distribution", label: "Distribution" },
  { key: "histogram", label: "Histogram" },
  { key: "barChart", label: "Bar Chart" },
  { key: "scatterPlot", label: "Scatter Plot" },
  { key: "correlation", label: "Correlation" },
];
