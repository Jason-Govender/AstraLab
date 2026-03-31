"use client";

import { Tag } from "antd";
import type { DatasetTransformationType } from "@/types/datasets";
import {
  getDatasetTransformationTypeColor,
  getDatasetTransformationTypeLabel,
} from "@/utils/datasetTransformations";
import { useStyles } from "./style";

interface DatasetTransformationTypeTagProps {
  transformationType: DatasetTransformationType;
}

export const DatasetTransformationTypeTag = ({
  transformationType,
}: DatasetTransformationTypeTagProps) => {
  const { styles } = useStyles();

  return (
    <Tag
      color={getDatasetTransformationTypeColor(transformationType)}
      className={styles.tag}
    >
      {getDatasetTransformationTypeLabel(transformationType)}
    </Tag>
  );
};
