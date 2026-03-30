"use client";

import { Card, Descriptions, Empty, Typography } from "antd";
import type { DatasetFileSummary } from "@/types/datasets";
import { formatBytes, formatDateTime } from "@/utils/datasets";
import { useStyles } from "./style";

const { Title } = Typography;

interface RawFileSummaryCardProps {
  rawFile?: DatasetFileSummary | null;
}

export const RawFileSummaryCard = ({ rawFile }: RawFileSummaryCardProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Raw File
      </Title>

      {!rawFile ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Raw file metadata is not available for this version."
        />
      ) : (
        <Descriptions
          size="small"
          column={1}
          items={[
            {
              key: "name",
              label: "File name",
              children: rawFile.originalFileName,
            },
            {
              key: "type",
              label: "Content type",
              children: rawFile.contentType || "Unknown",
            },
            {
              key: "size",
              label: "Size",
              children: formatBytes(rawFile.sizeBytes),
            },
            {
              key: "created",
              label: "Stored",
              children: formatDateTime(rawFile.creationTime),
            },
          ]}
        />
      )}
    </Card>
  );
};
