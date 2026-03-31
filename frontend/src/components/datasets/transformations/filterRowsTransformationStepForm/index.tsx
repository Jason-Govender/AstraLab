"use client";

import { Button, Card, Input, Select, Typography } from "antd";
import {
  FILTER_MATCH_MODE_OPTIONS,
  FILTER_OPERATOR_OPTIONS,
} from "@/constants/datasetTransformations";
import type {
  DatasetColumn,
  FilterRowsTransformationCondition,
  FilterRowsTransformationConfig,
} from "@/types/datasets";
import { FilterOperator } from "@/types/datasets";
import {
  createDefaultFilterCondition,
  getAllowedFilterOperators,
} from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface FilterRowsTransformationStepFormProps {
  columns: DatasetColumn[];
  configuration: FilterRowsTransformationConfig;
  onChange: (configuration: FilterRowsTransformationConfig) => void;
}

const isOperatorValueOptional = (operator: FilterOperator): boolean =>
  operator === FilterOperator.IsNull || operator === FilterOperator.IsNotNull;

export const FilterRowsTransformationStepForm = ({
  columns,
  configuration,
  onChange,
}: FilterRowsTransformationStepFormProps) => {
  const { styles } = useStyles();

  const updateCondition = (
    index: number,
    condition: FilterRowsTransformationCondition,
  ) => {
    onChange({
      ...configuration,
      conditions: configuration.conditions.map((currentCondition, conditionIndex) =>
        conditionIndex === index ? condition : currentCondition,
      ),
    });
  };

  const removeCondition = (index: number) => {
    onChange({
      ...configuration,
      conditions: configuration.conditions.filter(
        (_currentCondition, conditionIndex) => conditionIndex !== index,
      ),
    });
  };

  const addCondition = () => {
    onChange({
      ...configuration,
      conditions: [...configuration.conditions, createDefaultFilterCondition()],
    });
  };

  return (
    <Card size="small" className={styles.card}>
      <div className={styles.stack}>
        <div className={styles.field}>
          <Paragraph className={styles.label}>Match Mode</Paragraph>
          <Select
            size="large"
            value={configuration.match}
            className={styles.fullWidth}
            options={FILTER_MATCH_MODE_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            onChange={(match) =>
              onChange({
                ...configuration,
                match,
              })
            }
          />
        </div>

        <div className={styles.conditions}>
          {configuration.conditions.map((condition, index) => {
            const selectedColumn = columns.find(
              (column) => column.name === condition.column,
            );
            const allowedOperators = getAllowedFilterOperators(
              selectedColumn?.dataType,
            );

            return (
              <div
                key={`${condition.column || "condition"}-${index}`}
                className={styles.conditionCard}
              >
                <div className={styles.conditionHeader}>
                  <Paragraph className={styles.conditionTitle}>
                    Condition {index + 1}
                  </Paragraph>
                  <Button
                    size="small"
                    danger
                    disabled={configuration.conditions.length === 1}
                    onClick={() => removeCondition(index)}
                  >
                    Remove
                  </Button>
                </div>

                <div className={styles.conditionGrid}>
                  <div className={styles.field}>
                    <Paragraph className={styles.label}>Column</Paragraph>
                    <Select
                      size="large"
                      value={condition.column}
                      className={styles.fullWidth}
                      placeholder="Select a column"
                      options={columns.map((column) => ({
                        label: column.name,
                        value: column.name,
                      }))}
                      onChange={(column) =>
                        updateCondition(index, {
                          column,
                          operator: getAllowedFilterOperators(
                            columns.find((item) => item.name === column)?.dataType,
                          )[0],
                          value: "",
                        })
                      }
                    />
                  </div>

                  <div className={styles.field}>
                    <Paragraph className={styles.label}>Operator</Paragraph>
                    <Select
                      size="large"
                      value={condition.operator}
                      className={styles.fullWidth}
                      options={FILTER_OPERATOR_OPTIONS.filter((option) =>
                        allowedOperators.includes(option.value),
                      ).map((option) => ({
                        label: option.label,
                        value: option.value,
                      }))}
                      onChange={(operator) =>
                        updateCondition(index, {
                          ...condition,
                          operator,
                          value: isOperatorValueOptional(operator)
                            ? undefined
                            : condition.value || "",
                        })
                      }
                    />
                  </div>

                  {!isOperatorValueOptional(condition.operator) ? (
                    <div className={styles.field}>
                      <Paragraph className={styles.label}>Value</Paragraph>
                      <Input
                        size="large"
                        value={condition.value}
                        placeholder="Value to compare against"
                        onChange={(event) =>
                          updateCondition(index, {
                            ...condition,
                            value: event.target.value,
                          })
                        }
                      />
                    </div>
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>

        <div className={styles.actionsRow}>
          <Button onClick={addCondition}>Add condition</Button>
        </div>

        <Paragraph className={styles.helperText}>
          Operators adapt to the selected column type so the frontend stays aligned with the backend contract.
        </Paragraph>
      </div>
    </Card>
  );
};
