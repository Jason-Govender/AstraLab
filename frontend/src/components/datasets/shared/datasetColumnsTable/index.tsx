"use client";

import { Card, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { DatasetColumn } from "@/types/datasets";
import { useStyles } from "./style";

const { Title } = Typography;

interface DatasetColumnsTableProps {
  columns: DatasetColumn[];
  isLoading?: boolean;
  title?: string;
}

export const DatasetColumnsTable = ({
  columns,
  isLoading = false,
  title = "Columns",
}: DatasetColumnsTableProps) => {
  const { styles } = useStyles();

  const tableColumns: ColumnsType<DatasetColumn> = [
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
    },
    {
      title: "Data Type",
      dataIndex: "dataType",
      key: "dataType",
      render: (_, record) => record.dataType || "Unknown",
    },
    {
      title: "Inference",
      dataIndex: "isDataTypeInferred",
      key: "isDataTypeInferred",
      render: (_, record) =>
        record.isDataTypeInferred ? <Tag color="processing">Inferred</Tag> : <Tag>Declared</Tag>,
    },
    {
      title: "Nulls",
      dataIndex: "nullCount",
      key: "nullCount",
      render: (_, record) => (
        <span className={styles.subtleValue}>
          {record.nullCount ?? "Not profiled"}
        </span>
      ),
    },
    {
      title: "Distinct",
      dataIndex: "distinctCount",
      key: "distinctCount",
      render: (_, record) => (
        <span className={styles.subtleValue}>
          {record.distinctCount ?? "Not profiled"}
        </span>
      ),
    },
  ];

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        {title}
      </Title>

      <Table<DatasetColumn>
        rowKey="id"
        loading={isLoading}
        columns={tableColumns}
        dataSource={columns}
        pagination={false}
        className={styles.table}
        scroll={{ x: 920 }}
      />
    </Card>
  );
};
