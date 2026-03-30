"use client";

import { Button, Card, Space, Table, Typography } from "antd";
import type { ColumnsType, TablePaginationConfig } from "antd/es/table";
import type { DatasetListItem } from "@/types/datasets";
import { formatRelativeTime } from "@/utils/datasets";
import { DatasetFormatTag } from "@/components/datasets/shared/datasetFormatTag";
import { DatasetStatusTag } from "@/components/datasets/shared/datasetStatusTag";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DatasetCatalogTableProps {
  datasets: DatasetListItem[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  isLoading?: boolean;
  onOpenDataset: (datasetId: number, versionId?: number | null) => void;
  onChangePage: (page: number, pageSize: number) => void;
}

export const DatasetCatalogTable = ({
  datasets,
  totalCount,
  currentPage,
  pageSize,
  isLoading = false,
  onOpenDataset,
  onChangePage,
}: DatasetCatalogTableProps) => {
  const { styles } = useStyles();

  const columns: ColumnsType<DatasetListItem> = [
    {
      title: "Dataset",
      dataIndex: "name",
      key: "name",
      render: (_, record) => (
        <div className={styles.nameBlock}>
          <Text strong>{record.name}</Text>
          <Text className={styles.description}>
            {record.description || record.originalFileName}
          </Text>
        </div>
      ),
    },
    {
      title: "Format",
      dataIndex: "sourceFormat",
      key: "sourceFormat",
      width: 110,
      render: (_, record) => <DatasetFormatTag format={record.sourceFormat} />,
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      width: 130,
      render: (_, record) => <DatasetStatusTag status={record.status} />,
    },
    {
      title: "Current Version",
      key: "currentVersion",
      width: 150,
      render: (_, record) =>
        record.currentVersionNumber ? `v${record.currentVersionNumber}` : "Not set",
    },
    {
      title: "Columns",
      dataIndex: "columnCount",
      key: "columnCount",
      width: 120,
      render: (_, record) => record.columnCount ?? "Unknown",
    },
    {
      title: "Updated",
      dataIndex: "creationTime",
      key: "creationTime",
      width: 160,
      render: (_, record) => formatRelativeTime(record.creationTime),
    },
    {
      title: "",
      key: "actions",
      align: "right",
      width: 140,
      render: (_, record) => (
        <Button
          type="default"
          onClick={() => onOpenDataset(record.id, record.currentVersionId)}
        >
          Open dataset
        </Button>
      ),
    },
  ];

  const handlePaginationChange = (pagination: TablePaginationConfig) => {
    onChangePage(pagination.current || 1, pagination.pageSize || pageSize);
  };

  return (
    <Card className={styles.card}>
      <div className={styles.titleRow}>
        <div>
          <Title level={4} className={styles.title}>
            Datasets
          </Title>
          <Paragraph className={styles.helperText}>
            Browse uploaded datasets and open a specific version for inspection.
          </Paragraph>
        </div>

        <Space>
          <Text className={styles.helperText}>{totalCount} total</Text>
        </Space>
      </div>

      <Table<DatasetListItem>
        rowKey="id"
        loading={isLoading}
        columns={columns}
        dataSource={datasets}
        className={styles.table}
        pagination={{
          current: currentPage,
          pageSize,
          total: totalCount,
          showSizeChanger: true,
        }}
        onChange={handlePaginationChange}
        scroll={{ x: 960 }}
      />
    </Card>
  );
};
