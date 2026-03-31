"use client";

import { Button, Input, Select, Space, Typography } from "antd";
import { FILTER_MATCH_MODE_OPTIONS, FILTER_OPERATOR_OPTIONS } from "@/constants/datasetTransformations";
import type {
  DatasetColumn,
  FilterRowsTransformationConfig,
  FilterRowsTransformationCondition,
} from "@/types/datasets";
import { FilterOperator } from "@/types/datasets";
import {
  getAllowedFilterOperators,
  getFilterOperatorLabel,
} from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface FilterRowsTransformationFormProps {
  columns: DatasetColumn[];
  value: FilterRowsTransformationConfig;
  onChange: (value: FilterRowsTransformationConfig) => void;
}

export const FilterRowsTransformationForm = ({
  columns,
  value,
  onChange,
}: FilterRowsTransformationFormProps) => {
  const { styles } = useStyles();

  const updateCondition = (
    conditionIndex: number,
    nextCondition: FilterRowsTransformationCondition,
  ) => {
    onChange({
      ...value,
      conditions: value.conditions.map((condition, index) =>
        index === conditionIndex ? nextCondition : condition,
      ),
    });
  };

  const removeCondition = (conditionIndex: number) => {
    onChange({
      ...value,
      conditions: value.conditions.filter((_, index) => index !== conditionIndex),
    });
  };

  return (
    <Space direction="vertical" size="middle" style={{ width: "100%" }}>
      <Paragraph type="secondary">
        Define one or more conditions that the backend should evaluate to keep matching rows.
      </Paragraph>

      <Select
        size="large"
        value={value.match}
        options={FILTER_MATCH_MODE_OPTIONS as unknown as { label: string; value: string }[]}
        onChange={(match) =>
          onChange({
            ...value,
            match,
          })
        }
      />

      <div className={styles.conditionList}>
        {value.conditions.map((condition, conditionIndex) => {
          const selectedColumn = columns.find(
            (column) => column.name === condition.column,
          );
          const allowedOperators = getAllowedFilterOperators(
            selectedColumn?.dataType,
          );

          return (
            <div key={`${condition.column || "condition"}-${conditionIndex}`} className={styles.conditionCard}>
              <div className={styles.conditionGrid}>
                <Select
                  size="large"
                  value={condition.column}
                  placeholder="Column"
                  options={columns.map((column) => ({
                    label: column.name,
                    value: column.name,
                  }))}
                  onChange={(column) =>
                    updateCondition(conditionIndex, {
                      ...condition,
                      column,
                      operator: allowedOperators.includes(condition.operator)
                        ? condition.operator
                        : allowedOperators[0],
                    })
                  }
                />

                <Select
                  size="large"
                  value={condition.operator}
                  placeholder="Operator"
                  options={FILTER_OPERATOR_OPTIONS.filter((option) =>
                    allowedOperators.includes(option.value),
                  ) as unknown as { label: string; value: string }[]}
                  onChange={(operator) =>
                    updateCondition(conditionIndex, {
                      ...condition,
                      operator,
                      value:
                        operator === FilterOperator.IsNull ||
                        operator === FilterOperator.IsNotNull
                          ? undefined
                          : condition.value,
                    })
                  }
                />

                {condition.operator === FilterOperator.IsNull ||
                condition.operator === FilterOperator.IsNotNull ? (
                  <Input
                    size="large"
                    disabled
                    value={getFilterOperatorLabel(condition.operator)}
                  />
                ) : (
                  <Input
                    size="large"
                    value={condition.value}
                    placeholder="Comparison value"
                    onChange={(event) =>
                      updateCondition(conditionIndex, {
                        ...condition,
                        value: event.target.value,
                      })
                    }
                  />
                )}

                <Button
                  danger
                  onClick={() => removeCondition(conditionIndex)}
                  disabled={value.conditions.length === 1}
                >
                  Remove
                </Button>
              </div>
            </div>
          );
        })}
      </div>

      <Button
        onClick={() =>
          onChange({
            ...value,
            conditions: [
              ...value.conditions,
              {
                column: columns[0]?.name,
                operator: FilterOperator.Equals,
                value: "",
              },
            ],
          })
        }
      >
        Add condition
      </Button>
    </Space>
  );
};
