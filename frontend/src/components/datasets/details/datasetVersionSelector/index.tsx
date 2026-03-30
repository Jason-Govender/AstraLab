"use client";

import { Card, Select, Space, Typography } from "antd";
import type { DatasetVersionSummary } from "@/types/datasets";
import { formatDateTime, getDatasetVersionTypeLabel } from "@/utils/datasets";
import { DatasetStatusTag } from "@/components/datasets/shared/datasetStatusTag";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetVersionSelectorProps {
  versions: DatasetVersionSummary[];
  selectedVersionId?: number;
  onChange: (versionId?: number) => void;
}

export const DatasetVersionSelector = ({
  versions,
  selectedVersionId,
  onChange,
}: DatasetVersionSelectorProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Dataset Version
      </Title>

      <Select
        value={selectedVersionId}
        onChange={(value) => onChange(value)}
        options={versions.map((version) => ({
          label: `v${version.versionNumber} · ${getDatasetVersionTypeLabel(version.versionType)}`,
          value: version.id,
        }))}
        size="large"
        className={styles.select}
      />

      {versions.length > 0 ? (
        <div className={styles.list}>
          {versions.slice(0, 3).map((version) => (
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
      ) : null}
    </Card>
  );
};
