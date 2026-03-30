import { DatasetStatus } from "@/types/datasets";

export const DEFAULT_DATASET_PAGE_SIZE = 10;

export const DATASET_PAGE_SIZE_OPTIONS = ["10", "20", "50"];

export const DATASET_STATUS_FILTER_OPTIONS = [
  { label: "All statuses", value: 0 },
  { label: "Uploaded", value: DatasetStatus.Uploaded },
  { label: "Profiling", value: DatasetStatus.Profiling },
  { label: "Ready", value: DatasetStatus.Ready },
  { label: "Failed", value: DatasetStatus.Failed },
  { label: "Archived", value: DatasetStatus.Archived },
] as const;
