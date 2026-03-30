"use client";

import { Card, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import { DATASET_PROFILE_COLUMNS_PAGE_SIZE_OPTIONS } from "@/constants/datasets";
import type { DatasetColumnInsight } from "@/types/datasets";
import { formatNumber, formatPercentage } from "@/utils/datasets";
import { useStyles } from "./style";

const { Title } = Typography;

interface DatasetProfileColumnsTableProps {
  columns: DatasetColumnInsight[];
  isLoading?: boolean;
  title?: string;
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number, pageSize: number) => void;
}

export const DatasetProfileColumnsTable = ({
  columns,
  isLoading = false,
  title = "Profiled Column Insights",
  page = 1,
  pageSize = columns.length || 1,
  totalCount = columns.length,
  onPageChange,
}: DatasetProfileColumnsTableProps) => {
  const { styles } = useStyles();

  const tableColumns: ColumnsType<DatasetColumnInsight> = [
    {
      title: "#",
      dataIndex: "ordinal",
      key: "ordinal",
      width: 72,
    },
    {
      title: "Column",
      dataIndex: "name",
      key: "name",
      width: 220,
    },
    {
      title: "Inferred Type",
      dataIndex: "inferredDataType",
      key: "inferredDataType",
      render: (_, record) => (
        <Tag color="processing" className={styles.typeTag}>
          {record.inferredDataType || "Unknown"}
        </Tag>
      ),
    },
    {
      title: "Nulls",
      dataIndex: "nullCount",
      key: "nullCount",
      render: (_, record) => formatNumber(record.nullCount, 0),
    },
    {
      title: "Null Rate",
      dataIndex: "nullPercentage",
      key: "nullPercentage",
      render: (_, record) => (
        <Tag color={record.nullPercentage > 0 ? "warning" : "success"}>
          {formatPercentage(record.nullPercentage)}
        </Tag>
      ),
    },
    {
      title: "Distinct",
      dataIndex: "distinctCount",
      key: "distinctCount",
      render: (_, record) =>
        record.distinctCount === null || record.distinctCount === undefined ? (
          <span className={styles.subtleValue}>Unknown</span>
        ) : (
          formatNumber(record.distinctCount, 0)
        ),
    },
    {
      title: "Mean",
      dataIndex: "mean",
      key: "mean",
      render: (_, record) => formatNumber(record.mean),
    },
    {
      title: "Min",
      dataIndex: "min",
      key: "min",
      render: (_, record) => formatNumber(record.min),
    },
    {
      title: "Max",
      dataIndex: "max",
      key: "max",
      render: (_, record) => formatNumber(record.max),
    },
    {
      title: "Anomalies",
      dataIndex: "anomalyCount",
      key: "anomalyCount",
      render: (_, record) => (
        <Tag color={record.hasAnomalies ? "error" : "success"}>
          {record.hasAnomalies
            ? `${formatNumber(record.anomalyCount, 0)} flagged`
            : "None"}
        </Tag>
      ),
    },
    {
      title: "Anomaly Rate",
      dataIndex: "anomalyPercentage",
      key: "anomalyPercentage",
      render: (_, record) => formatPercentage(record.anomalyPercentage),
    },
  ];

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        {title}
      </Title>

      <Table<DatasetColumnInsight>
        rowKey="datasetColumnId"
        loading={isLoading}
        columns={tableColumns}
        dataSource={columns}
        className={styles.table}
        scroll={{ x: 1320 }}
        pagination={
          onPageChange
            ? {
                current: page,
                pageSize,
                total: totalCount,
                onChange: onPageChange,
                showSizeChanger: true,
                pageSizeOptions: DATASET_PROFILE_COLUMNS_PAGE_SIZE_OPTIONS,
              }
            : false
        }
      />
    </Card>
  );
};
