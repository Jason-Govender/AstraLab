"use client";

import { Tag } from "antd";
import type { DatasetStatus, DatasetVersionStatus } from "@/types/datasets";
import {
  getDatasetStatusColor,
  getDatasetStatusLabel,
  getDatasetVersionStatusColor,
  getDatasetVersionStatusLabel,
} from "@/utils/datasets";
import { useStyles } from "./style";

interface DatasetStatusTagProps {
  status: DatasetStatus | DatasetVersionStatus;
  kind?: "dataset" | "version";
}

export const DatasetStatusTag = ({
  status,
  kind = "dataset",
}: DatasetStatusTagProps) => {
  const { styles } = useStyles();
  const color =
    kind === "version"
      ? getDatasetVersionStatusColor(status as DatasetVersionStatus)
      : getDatasetStatusColor(status as DatasetStatus);
  const label =
    kind === "version"
      ? getDatasetVersionStatusLabel(status as DatasetVersionStatus)
      : getDatasetStatusLabel(status as DatasetStatus);

  return (
    <Tag color={color} className={styles.tag}>
      {label}
    </Tag>
  );
};
