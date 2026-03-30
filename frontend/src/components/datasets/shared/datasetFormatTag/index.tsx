"use client";

import { Tag } from "antd";
import type { DatasetFormat } from "@/types/datasets";
import { getDatasetFormatLabel } from "@/utils/datasets";
import { useStyles } from "./style";

interface DatasetFormatTagProps {
  format: DatasetFormat;
}

export const DatasetFormatTag = ({ format }: DatasetFormatTagProps) => {
  const { styles } = useStyles();

  return <Tag className={styles.tag}>{getDatasetFormatLabel(format)}</Tag>;
};
