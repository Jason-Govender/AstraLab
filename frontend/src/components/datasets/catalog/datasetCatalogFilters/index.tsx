"use client";

import { Button, Card, Form, Input, Select, Typography } from "antd";
import {
  DATASET_PAGE_SIZE_OPTIONS,
  DATASET_STATUS_FILTER_OPTIONS,
  DEFAULT_DATASET_PAGE_SIZE,
} from "@/constants/datasets";
import type {
  DatasetCatalogFilters as DatasetCatalogFiltersShape,
  DatasetStatus,
} from "@/types/datasets";
import { useStyles } from "./style";

const { Paragraph } = Typography;

interface DatasetCatalogFiltersProps {
  filters: DatasetCatalogFiltersShape;
  isPending?: boolean;
  onApplyFilters: (filters: DatasetCatalogFiltersShape) => void;
}

interface DatasetCatalogFilterFormValues {
  keyword?: string;
  status?: DatasetStatus | 0;
  pageSize?: number;
}

export const DatasetCatalogFilters = ({
  filters,
  isPending = false,
  onApplyFilters,
}: DatasetCatalogFiltersProps) => {
  const { styles } = useStyles();

  const handleSubmit = (values: DatasetCatalogFilterFormValues) => {
    onApplyFilters({
      keyword: values.keyword?.trim() || "",
      status: values.status && values.status > 0 ? values.status : undefined,
      page: 1,
      pageSize: values.pageSize || DEFAULT_DATASET_PAGE_SIZE,
    });
  };

  const handleReset = () => {
    onApplyFilters({
      keyword: "",
      status: undefined,
      page: 1,
      pageSize: DEFAULT_DATASET_PAGE_SIZE,
    });
  };

  return (
    <Card className={styles.card}>
      <Form<DatasetCatalogFilterFormValues>
        key={JSON.stringify(filters)}
        initialValues={{
          keyword: filters.keyword,
          status: filters.status ?? 0,
          pageSize: filters.pageSize,
        }}
        onFinish={handleSubmit}
        className={styles.form}
      >
        <Form.Item className={styles.field} name="keyword">
          <Input
            size="large"
            placeholder="Search by dataset name or description"
            allowClear
          />
        </Form.Item>

        <Form.Item className={styles.field} name="status">
          <Select
            size="large"
            placeholder="Filter by status"
            options={DATASET_STATUS_FILTER_OPTIONS.map((option) => ({
              label: option.label,
              value: option.value,
            }))}
            allowClear
          />
        </Form.Item>

        <Form.Item className={styles.field} name="pageSize">
          <Select
            size="large"
            options={DATASET_PAGE_SIZE_OPTIONS.map((value) => ({
              label: `${value} per page`,
              value: Number(value),
            }))}
          />
        </Form.Item>

        <div className={styles.actions}>
          <Button
            htmlType="submit"
            type="primary"
            size="large"
            loading={isPending}
            className={styles.button}
          >
            Apply filters
          </Button>
          <Button
            size="large"
            disabled={isPending}
            onClick={handleReset}
            className={styles.button}
          >
            Clear
          </Button>
        </div>
      </Form>

      <Paragraph className={styles.helperText}>
        Filter datasets by name, description, lifecycle status, and page size.
      </Paragraph>
    </Card>
  );
};
