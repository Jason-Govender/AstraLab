"use client";

import { Card, Select, Typography } from "antd";
import { DATASET_TRANSFORMATION_DATA_TYPE_OPTIONS } from "@/constants/datasetTransformations";
import type {
  ConvertDataTypeTransformationConfig,
  DatasetColumn,
} from "@/types/datasets";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface ConvertDataTypeTransformationStepFormProps {
  columns: DatasetColumn[];
  configuration: ConvertDataTypeTransformationConfig;
  onChange: (configuration: ConvertDataTypeTransformationConfig) => void;
}

export const ConvertDataTypeTransformationStepForm = ({
  columns,
  configuration,
  onChange,
}: ConvertDataTypeTransformationStepFormProps) => {
  const { styles } = useStyles();

  return (
    <Card size="small" className={styles.card}>
      <div className={styles.stack}>
        <div className={styles.field}>
          <Paragraph className={styles.label}>Column</Paragraph>
          <Select
            size="large"
            value={configuration.column}
            className={styles.fullWidth}
            placeholder="Choose a column to convert"
            options={columns.map((column) => ({
              label: column.name,
              value: column.name,
            }))}
            onChange={(column) =>
              onChange({
                ...configuration,
                column,
              })
            }
          />
        </div>

        <div className={styles.field}>
          <Paragraph className={styles.label}>Target Type</Paragraph>
          <Select
            size="large"
            value={configuration.targetType}
            className={styles.fullWidth}
            options={DATASET_TRANSFORMATION_DATA_TYPE_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            onChange={(targetType) =>
              onChange({
                ...configuration,
                targetType,
              })
            }
          />
        </div>

        <Paragraph className={styles.helperText}>
          Invalid conversions are rejected by the backend with a clear validation message.
        </Paragraph>
      </div>
    </Card>
  );
};
