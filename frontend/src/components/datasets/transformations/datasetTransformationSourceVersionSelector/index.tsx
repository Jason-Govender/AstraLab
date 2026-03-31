"use client";

import { Card, Select, Space, Typography } from "antd";
import type { DatasetVersionSummary } from "@/types/datasets";
import { DatasetStatusTag } from "@/components/datasets/shared/datasetStatusTag";
import {
  formatDateTime,
  getDatasetVersionTypeLabel,
} from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetTransformationSourceVersionSelectorProps {
  versions: DatasetVersionSummary[];
  selectedVersionId?: number;
  onChange: (versionId?: number) => void;
}

export const DatasetTransformationSourceVersionSelector = ({
  versions,
  selectedVersionId,
  onChange,
}: DatasetTransformationSourceVersionSelectorProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Source Version
      </Title>
      <Paragraph className={styles.helperText}>
        Choose the dataset version that the transformation pipeline should run against.
      </Paragraph>

      <Select
        value={selectedVersionId}
        onChange={(value) => onChange(value)}
        size="large"
        className={styles.select}
        options={versions.map((version) => ({
          label: `v${version.versionNumber} · ${getDatasetVersionTypeLabel(version.versionType)}`,
          value: version.id,
        }))}
      />

      <div className={styles.list}>
        {versions.slice(0, 4).map((version) => (
          <div key={version.id} className={styles.listItem}>
            <Space wrap>
              <strong>{`v${version.versionNumber}`}</strong>
              <DatasetStatusTag status={version.status} kind="version" />
            </Space>
            <Paragraph className={styles.meta}>
              {getDatasetVersionTypeLabel(version.versionType)} ·{" "}
              {formatDateTime(version.creationTime)}
            </Paragraph>
          </div>
        ))}
      </div>
    </Card>
  );
};
