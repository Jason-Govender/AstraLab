"use client";

import { Select, Space, Typography } from "antd";
import { DATASET_TRANSFORMATION_DATA_TYPE_OPTIONS } from "@/constants/datasetTransformations";
import type {
  ConvertDataTypeTransformationConfig,
  DatasetColumn,
} from "@/types/datasets";

const { Paragraph } = Typography;

interface ConvertDataTypeTransformationFormProps {
  columns: DatasetColumn[];
  value: ConvertDataTypeTransformationConfig;
  onChange: (value: ConvertDataTypeTransformationConfig) => void;
}

export const ConvertDataTypeTransformationForm = ({
  columns,
  value,
  onChange,
}: ConvertDataTypeTransformationFormProps) => (
  <Space direction="vertical" size="middle" style={{ width: "100%" }}>
    <Paragraph type="secondary">
      Select the source column and the target type to enforce on the processed version.
    </Paragraph>

    <Select
      size="large"
      value={value.column}
      placeholder="Select column"
      options={columns.map((column) => ({
        label: column.name,
        value: column.name,
      }))}
      onChange={(column) =>
        onChange({
          ...value,
          column,
        })
      }
    />

    <Select
      size="large"
      value={value.targetType}
      placeholder="Select target type"
      options={
        DATASET_TRANSFORMATION_DATA_TYPE_OPTIONS as unknown as {
          label: string;
          value: string;
        }[]
      }
      onChange={(targetType) =>
        onChange({
          ...value,
          targetType,
        })
      }
    />
  </Space>
);
