"use client";

import { Button, Input, Select, Space, Typography } from "antd";
import { AGGREGATION_FUNCTION_OPTIONS } from "@/constants/datasetTransformations";
import type {
  AggregateTransformationConfig,
  AggregateTransformationDefinition,
  DatasetColumn,
} from "@/types/datasets";
import { AggregationFunction } from "@/types/datasets";
import {
  canAggregationUseColumn,
  getNumericDatasetColumns,
} from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface AggregateTransformationFormProps {
  columns: DatasetColumn[];
  value: AggregateTransformationConfig;
  onChange: (value: AggregateTransformationConfig) => void;
}

export const AggregateTransformationForm = ({
  columns,
  value,
  onChange,
}: AggregateTransformationFormProps) => {
  const { styles } = useStyles();
  const numericColumns = getNumericDatasetColumns(columns);

  const updateAggregation = (
    aggregationIndex: number,
    nextAggregation: AggregateTransformationDefinition,
  ) => {
    onChange({
      ...value,
      aggregations: value.aggregations.map((aggregation, index) =>
        index === aggregationIndex ? nextAggregation : aggregation,
      ),
    });
  };

  const removeAggregation = (aggregationIndex: number) => {
    onChange({
      ...value,
      aggregations: value.aggregations.filter((_, index) => index !== aggregationIndex),
    });
  };

  return (
    <Space direction="vertical" size="middle" style={{ width: "100%" }}>
      <Paragraph type="secondary">
        Group rows by one or more columns and define the aggregation outputs that should be produced.
      </Paragraph>

      <Select
        mode="multiple"
        size="large"
        value={value.groupByColumns}
        placeholder="Group by columns"
        options={columns.map((column) => ({
          label: column.name,
          value: column.name,
        }))}
        onChange={(groupByColumns) =>
          onChange({
            ...value,
            groupByColumns,
          })
        }
      />

      <div className={styles.aggregationList}>
        {value.aggregations.map((aggregation, aggregationIndex) => (
          <div key={`${aggregation.outputColumn}-${aggregationIndex}`} className={styles.aggregationCard}>
            <div className={styles.aggregationGrid}>
              <Select
                size="large"
                value={aggregation.function}
                options={
                  AGGREGATION_FUNCTION_OPTIONS as unknown as {
                    label: string;
                    value: string;
                  }[]
                }
                onChange={(aggregationFunction) =>
                  updateAggregation(aggregationIndex, {
                    ...aggregation,
                    function: aggregationFunction,
                    column: canAggregationUseColumn(aggregationFunction)
                      ? aggregation.column
                      : undefined,
                  })
                }
              />

              {canAggregationUseColumn(aggregation.function) ? (
                <Select
                  size="large"
                  value={aggregation.column}
                  placeholder="Numeric column"
                  options={numericColumns.map((column) => ({
                    label: column.name,
                    value: column.name,
                  }))}
                  onChange={(column) =>
                    updateAggregation(aggregationIndex, {
                      ...aggregation,
                      column,
                    })
                  }
                />
              ) : (
                <Input size="large" disabled value="No source column needed" />
              )}

              <Input
                size="large"
                value={aggregation.outputColumn}
                placeholder="Output column name"
                onChange={(event) =>
                  updateAggregation(aggregationIndex, {
                    ...aggregation,
                    outputColumn: event.target.value,
                  })
                }
              />

              <Button
                danger
                onClick={() => removeAggregation(aggregationIndex)}
                disabled={value.aggregations.length === 1}
              >
                Remove
              </Button>
            </div>
          </div>
        ))}
      </div>

      <Button
        onClick={() =>
          onChange({
            ...value,
            aggregations: [
              ...value.aggregations,
              {
                function: AggregationFunction.Count,
                column: undefined,
                outputColumn: "",
              },
            ],
          })
        }
      >
        Add aggregation
      </Button>
    </Space>
  );
};
