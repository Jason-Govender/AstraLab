"use client";

import { Button, Card, Input, Select, Typography } from "antd";
import { AGGREGATION_FUNCTION_OPTIONS } from "@/constants/datasetTransformations";
import type {
  AggregateTransformationConfig,
  AggregateTransformationDefinition,
  DatasetColumn,
} from "@/types/datasets";
import {
  canAggregationUseColumn,
  createDefaultAggregationDefinition,
  getNumericDatasetColumns,
} from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface AggregateTransformationStepFormProps {
  columns: DatasetColumn[];
  configuration: AggregateTransformationConfig;
  onChange: (configuration: AggregateTransformationConfig) => void;
}

export const AggregateTransformationStepForm = ({
  columns,
  configuration,
  onChange,
}: AggregateTransformationStepFormProps) => {
  const { styles } = useStyles();
  const numericColumns = getNumericDatasetColumns(columns);

  const updateAggregation = (
    index: number,
    aggregation: AggregateTransformationDefinition,
  ) => {
    onChange({
      ...configuration,
      aggregations: configuration.aggregations.map(
        (currentAggregation, aggregationIndex) =>
          aggregationIndex === index ? aggregation : currentAggregation,
      ),
    });
  };

  const removeAggregation = (index: number) => {
    onChange({
      ...configuration,
      aggregations: configuration.aggregations.filter(
        (_aggregation, aggregationIndex) => aggregationIndex !== index,
      ),
    });
  };

  const addAggregation = () => {
    onChange({
      ...configuration,
      aggregations: [
        ...configuration.aggregations,
        createDefaultAggregationDefinition(),
      ],
    });
  };

  return (
    <Card size="small" className={styles.card}>
      <div className={styles.stack}>
        <div className={styles.field}>
          <Paragraph className={styles.label}>Group By Columns</Paragraph>
          <Select
            mode="multiple"
            size="large"
            value={configuration.groupByColumns}
            className={styles.fullWidth}
            placeholder="Optional grouping columns"
            options={columns.map((column) => ({
              label: column.name,
              value: column.name,
            }))}
            onChange={(groupByColumns) =>
              onChange({
                ...configuration,
                groupByColumns: groupByColumns.map(String),
              })
            }
          />
        </div>

        <div className={styles.aggregationList}>
          {configuration.aggregations.map((aggregation, index) => {
            const requiresColumn = canAggregationUseColumn(aggregation.function);

            return (
              <div
                key={`${aggregation.outputColumn || "aggregation"}-${index}`}
                className={styles.aggregationCard}
              >
                <div className={styles.aggregationHeader}>
                  <Paragraph className={styles.aggregationTitle}>
                    Aggregation {index + 1}
                  </Paragraph>
                  <Button
                    size="small"
                    danger
                    disabled={configuration.aggregations.length === 1}
                    onClick={() => removeAggregation(index)}
                  >
                    Remove
                  </Button>
                </div>

                <div className={styles.aggregationGrid}>
                  <div className={styles.field}>
                    <Paragraph className={styles.label}>Function</Paragraph>
                    <Select
                      size="large"
                      value={aggregation.function}
                      className={styles.fullWidth}
                      options={AGGREGATION_FUNCTION_OPTIONS.map((option) => ({
                        label: option.label,
                        value: option.value,
                      }))}
                      onChange={(aggregationFunction) =>
                        updateAggregation(index, {
                          ...aggregation,
                          function: aggregationFunction,
                          column: canAggregationUseColumn(aggregationFunction)
                            ? aggregation.column
                            : undefined,
                        })
                      }
                    />
                  </div>

                  {requiresColumn ? (
                    <div className={styles.field}>
                      <Paragraph className={styles.label}>Column</Paragraph>
                      <Select
                        size="large"
                        value={aggregation.column}
                        className={styles.fullWidth}
                        placeholder="Numeric column"
                        options={numericColumns.map((column) => ({
                          label: column.name,
                          value: column.name,
                        }))}
                        onChange={(column) =>
                          updateAggregation(index, {
                            ...aggregation,
                            column,
                          })
                        }
                      />
                    </div>
                  ) : null}

                  <div className={styles.field}>
                    <Paragraph className={styles.label}>Output Column</Paragraph>
                    <Input
                      size="large"
                      value={aggregation.outputColumn}
                      placeholder="totalAmount"
                      onChange={(event) =>
                        updateAggregation(index, {
                          ...aggregation,
                          outputColumn: event.target.value,
                        })
                      }
                    />
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        <div className={styles.actionsRow}>
          <Button onClick={addAggregation}>Add aggregation</Button>
        </div>

        <Paragraph className={styles.helperText}>
          Count can run without a source column. Sum, average, minimum, and maximum require a numeric column.
        </Paragraph>
      </div>
    </Card>
  );
};
