"use client";

import { Card, Table, Typography } from "antd";
import type { TablePaginationConfig } from "antd";
import type {
  ColumnsType,
  SortOrder,
  SorterResult,
} from "antd/es/table/interface";
import { DATASET_EXPLORATION_ROW_PAGE_SIZE_OPTIONS } from "@/constants/datasetExploration";
import {
  DatasetRowSortDirection,
  type DatasetExplorationColumn,
  type DatasetRow,
  type PagedDatasetRowsRequest,
} from "@/types/datasets";
import { useStyles } from "./style";

const { Title } = Typography;

interface DatasetExplorationRowsTableProps {
  columns: DatasetExplorationColumn[];
  rows: DatasetRow[];
  totalCount: number;
  query: PagedDatasetRowsRequest;
  isLoading?: boolean;
  onQueryChange: (query: PagedDatasetRowsRequest) => void;
}

interface DatasetRowTableRecord extends DatasetRow {
  key: number;
}

export const DatasetExplorationRowsTable = ({
  columns,
  rows,
  totalCount,
  query,
  isLoading = false,
  onQueryChange,
}: DatasetExplorationRowsTableProps) => {
  const { styles } = useStyles();
  const getSortOrder = (datasetColumnId: number): SortOrder | undefined => {
    if (query.sortDatasetColumnId !== datasetColumnId) {
      return undefined;
    }

    return query.sortDirection === DatasetRowSortDirection.Descending
      ? "descend"
      : "ascend";
  };

  const tableColumns: ColumnsType<DatasetRowTableRecord> = [
    {
      title: "Row",
      dataIndex: "rowNumber",
      key: "rowNumber",
      width: 84,
      fixed: "left",
    },
    ...columns.map((column) => ({
      title: column.name,
      key: String(column.datasetColumnId),
      width: 180,
      sorter: true,
      sortOrder: getSortOrder(column.datasetColumnId),
      render: (_value: unknown, record: DatasetRowTableRecord) =>
        record.values[column.ordinal - 1] ?? "—",
    })),
  ];

  const dataSource: DatasetRowTableRecord[] = rows.map((row) => ({
    ...row,
    key: row.rowNumber,
  }));

  const handleTableChange = (
    pagination: TablePaginationConfig,
    _filters: Record<string, unknown>,
    sorter: SorterResult<DatasetRowTableRecord> | SorterResult<DatasetRowTableRecord>[],
  ) => {
    const nextSorter = Array.isArray(sorter) ? sorter[0] : sorter;
    const sortDatasetColumnId =
      nextSorter?.field || nextSorter?.columnKey
        ? Number(nextSorter.columnKey)
        : undefined;

    onQueryChange({
      ...query,
      page: pagination.current || 1,
      pageSize: pagination.pageSize || query.pageSize,
      sortDatasetColumnId:
        sortDatasetColumnId && Number.isFinite(sortDatasetColumnId)
          ? sortDatasetColumnId
          : undefined,
      sortDirection:
        nextSorter?.order === "descend"
          ? DatasetRowSortDirection.Descending
          : DatasetRowSortDirection.Ascending,
    });
  };

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Rows
      </Title>

      <Table<DatasetRowTableRecord>
        rowKey="key"
        loading={isLoading}
        columns={tableColumns}
        dataSource={dataSource}
        className={styles.table}
        scroll={{ x: 960 }}
        onChange={handleTableChange}
        pagination={{
          current: query.page,
          pageSize: query.pageSize,
          total: totalCount,
          showSizeChanger: true,
          pageSizeOptions: DATASET_EXPLORATION_ROW_PAGE_SIZE_OPTIONS,
        }}
      />
    </Card>
  );
};
