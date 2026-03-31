"use client";

import { Card, Input, Select, Typography } from "antd";
import { MISSING_VALUE_STRATEGY_OPTIONS } from "@/constants/datasetTransformations";
import type {
  DatasetColumn,
  HandleMissingValuesTransformationConfig,
} from "@/types/datasets";
import { MissingValueStrategy } from "@/types/datasets";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface HandleMissingValuesTransformationStepFormProps {
  columns: DatasetColumn[];
  configuration: HandleMissingValuesTransformationConfig;
  onChange: (configuration: HandleMissingValuesTransformationConfig) => void;
}

export const HandleMissingValuesTransformationStepForm = ({
  columns,
  configuration,
  onChange,
}: HandleMissingValuesTransformationStepFormProps) => {
  const { styles } = useStyles();
  const isConstantStrategy =
    configuration.strategy === MissingValueStrategy.FillConstant;

  return (
    <Card size="small" className={styles.card}>
      <div className={styles.stack}>
        <div className={styles.field}>
          <Paragraph className={styles.label}>Columns</Paragraph>
          <Select
            mode="multiple"
            size="large"
            value={configuration.columns}
            className={styles.fullWidth}
            placeholder="Select one or more columns"
            options={columns.map((column) => ({
              label: column.name,
              value: column.name,
            }))}
            onChange={(selectedColumns) =>
              onChange({
                ...configuration,
                columns: selectedColumns.map(String),
              })
            }
          />
        </div>

        <div className={styles.field}>
          <Paragraph className={styles.label}>Strategy</Paragraph>
          <Select
            size="large"
            value={configuration.strategy}
            className={styles.fullWidth}
            options={MISSING_VALUE_STRATEGY_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            onChange={(strategy) =>
              onChange({
                ...configuration,
                strategy,
                constantValue:
                  strategy === MissingValueStrategy.FillConstant
                    ? configuration.constantValue || ""
                    : undefined,
              })
            }
          />
        </div>

        {isConstantStrategy ? (
          <div className={styles.field}>
            <Paragraph className={styles.label}>Constant Value</Paragraph>
            <Input
              size="large"
              value={configuration.constantValue}
              placeholder="Value to insert when a cell is null"
              onChange={(event) =>
                onChange({
                  ...configuration,
                  constantValue: event.target.value,
                })
              }
            />
          </div>
        ) : null}

        <Paragraph className={styles.helperText}>
          The backend applies the selected strategy to the chosen columns and returns the transformed processed version.
        </Paragraph>
      </div>
    </Card>
  );
};
