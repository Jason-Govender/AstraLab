"use client";

import { Card, Descriptions, Typography } from "antd";
import type { Dataset, DatasetVersionDetails } from "@/types/datasets";
import { formatBytes, formatDateTime } from "@/utils/datasets";
import { DatasetFormatTag } from "@/components/datasets/shared/datasetFormatTag";
import { DatasetStatusTag } from "@/components/datasets/shared/datasetStatusTag";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetOverviewCardProps {
  dataset: Dataset;
  selectedVersion?: DatasetVersionDetails | null;
}

export const DatasetOverviewCard = ({
  dataset,
  selectedVersion,
}: DatasetOverviewCardProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={3} className={styles.title}>
        {dataset.name}
      </Title>
      <Paragraph className={styles.description}>
        {dataset.description || "No description provided for this dataset."}
      </Paragraph>

      <div className={styles.metaRow}>
        <DatasetFormatTag format={dataset.sourceFormat} />
        <DatasetStatusTag status={dataset.status} />
        {selectedVersion ? (
          <DatasetStatusTag status={selectedVersion.status} kind="version" />
        ) : null}
      </div>

      <Descriptions
        size="small"
        column={2}
        items={[
          {
            key: "uploaded-file",
            label: "Original file",
            children: dataset.originalFileName,
          },
          {
            key: "created",
            label: "Created",
            children: formatDateTime(dataset.creationTime),
          },
          {
            key: "version",
            label: "Selected version",
            children: selectedVersion ? `v${selectedVersion.versionNumber}` : "Not selected",
          },
          {
            key: "size",
            label: "Version size",
            children: selectedVersion ? formatBytes(selectedVersion.sizeBytes) : "Unknown",
          },
          {
            key: "rows",
            label: "Rows",
            children: selectedVersion?.rowCount ?? "Not profiled",
          },
          {
            key: "columns",
            label: "Columns",
            children: selectedVersion?.columnCount ?? "Unknown",
          },
        ]}
      />
    </Card>
  );
};
