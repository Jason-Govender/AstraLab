"use client";

import { Card, Checkbox, Typography } from "antd";
import type { DatasetColumn, RemoveDuplicatesTransformationConfig } from "@/types/datasets";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface RemoveDuplicatesTransformationStepFormProps {
  columns: DatasetColumn[];
  configuration: RemoveDuplicatesTransformationConfig;
  onChange: (configuration: RemoveDuplicatesTransformationConfig) => void;
}

export const RemoveDuplicatesTransformationStepForm = ({
  columns,
  configuration,
  onChange,
}: RemoveDuplicatesTransformationStepFormProps) => {
  const { styles } = useStyles();

  return (
    <Card size="small" className={styles.card}>
      <Paragraph className={styles.helperText}>
        Select the columns that define a duplicate row. Leave this empty to remove exact duplicate rows across the full dataset.
      </Paragraph>

      <Checkbox.Group
        className={styles.group}
        value={configuration.columns}
        options={columns.map((column) => ({
          label: column.name,
          value: column.name,
        }))}
        onChange={(selectedColumns) =>
          onChange({
            columns: selectedColumns.map(String),
          })
        }
      />
    </Card>
  );
};
