"use client";

import { Input, Select, Space, Typography } from "antd";
import { MISSING_VALUE_STRATEGY_OPTIONS } from "@/constants/datasetTransformations";
import type {
  DatasetColumn,
  HandleMissingValuesTransformationConfig,
} from "@/types/datasets";
import { MissingValueStrategy } from "@/types/datasets";

const { Paragraph } = Typography;

interface HandleMissingValuesTransformationFormProps {
  columns: DatasetColumn[];
  value: HandleMissingValuesTransformationConfig;
  onChange: (value: HandleMissingValuesTransformationConfig) => void;
}

export const HandleMissingValuesTransformationForm = ({
  columns,
  value,
  onChange,
}: HandleMissingValuesTransformationFormProps) => (
  <Space direction="vertical" size="middle" style={{ width: "100%" }}>
    <Paragraph type="secondary">
      Choose the columns to target and the missing-value strategy the backend should apply.
    </Paragraph>

    <Select
      mode="multiple"
      size="large"
      value={value.columns}
      placeholder="Select columns"
      options={columns.map((column) => ({
        label: column.name,
        value: column.name,
      }))}
      onChange={(selectedColumns) =>
        onChange({
          ...value,
          columns: selectedColumns,
        })
      }
    />

    <Select
      size="large"
      value={value.strategy}
      options={MISSING_VALUE_STRATEGY_OPTIONS as unknown as { label: string; value: string }[]}
      onChange={(strategy) =>
        onChange({
          ...value,
          strategy,
          constantValue:
            strategy === MissingValueStrategy.FillConstant
              ? value.constantValue || ""
              : undefined,
        })
      }
    />

    {value.strategy === MissingValueStrategy.FillConstant ? (
      <Input
        size="large"
        value={value.constantValue}
        placeholder="Constant value"
        onChange={(event) =>
          onChange({
            ...value,
            constantValue: event.target.value,
          })
        }
      />
    ) : null}
  </Space>
);
