"use client";

import { Select, Typography } from "antd";
import type {
  DatasetColumn,
  RemoveDuplicatesTransformationConfig,
} from "@/types/datasets";

const { Paragraph } = Typography;

interface RemoveDuplicatesTransformationFormProps {
  columns: DatasetColumn[];
  value: RemoveDuplicatesTransformationConfig;
  onChange: (value: RemoveDuplicatesTransformationConfig) => void;
}

export const RemoveDuplicatesTransformationForm = ({
  columns,
  value,
  onChange,
}: RemoveDuplicatesTransformationFormProps) => (
  <>
    <Paragraph type="secondary">
      Optionally choose the columns that define a duplicate. Leave this empty to deduplicate entire rows.
    </Paragraph>

    <Select
      mode="multiple"
      size="large"
      value={value.columns}
      placeholder="All columns"
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
  </>
);
