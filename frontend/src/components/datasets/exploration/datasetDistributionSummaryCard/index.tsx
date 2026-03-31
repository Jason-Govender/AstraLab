"use client";

import { Column } from "@ant-design/charts";
import { Card, Descriptions, Empty, Table, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { BarChartCategory, DistributionAnalysis } from "@/types/datasets";
import { formatNumber } from "@/utils/datasets";
import { isDistributionNumeric } from "@/utils/datasetExploration";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetDistributionSummaryCardProps {
  distribution?: DistributionAnalysis;
  isLoading?: boolean;
}

export const DatasetDistributionSummaryCard = ({
  distribution,
  isLoading = false,
}: DatasetDistributionSummaryCardProps) => {
  const { styles } = useStyles();

  const categoryColumns: ColumnsType<BarChartCategory> = [
    {
      title: "Category",
      dataIndex: "label",
      key: "label",
    },
    {
      title: "Count",
      dataIndex: "count",
      key: "count",
      width: 120,
      render: (value: number) => formatNumber(value, 0),
    },
  ];

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Distribution Analysis
      </Title>

      {!distribution ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Run a distribution analysis to inspect numeric spread or categorical frequency for the selected column."
        />
      ) : (
        <div className={styles.stack}>
          <Descriptions
            size="small"
            column={2}
            items={[
              {
                key: "column",
                label: "Column",
                children: distribution.columnName,
              },
              {
                key: "data-type",
                label: "Data Type",
                children: distribution.dataType,
              },
              {
                key: "values",
                label: "Values",
                children: formatNumber(distribution.valueCount, 0),
              },
              {
                key: "nulls",
                label: "Nulls",
                children: formatNumber(distribution.nullCount, 0),
              },
              {
                key: "mean",
                label: "Mean",
                children:
                  distribution.mean === null || distribution.mean === undefined
                    ? "—"
                    : formatNumber(distribution.mean),
              },
              {
                key: "median",
                label: "Median",
                children:
                  distribution.median === null || distribution.median === undefined
                    ? "—"
                    : formatNumber(distribution.median),
              },
              {
                key: "min",
                label: "Min",
                children:
                  distribution.min === null || distribution.min === undefined
                    ? "—"
                    : formatNumber(distribution.min),
              },
              {
                key: "max",
                label: "Max",
                children:
                  distribution.max === null || distribution.max === undefined
                    ? "—"
                    : formatNumber(distribution.max),
              },
            ]}
          />

          {isDistributionNumeric(distribution) ? (
            <>
              <Paragraph className={styles.helperText}>
                Numeric distribution rendered from backend-generated histogram buckets.
              </Paragraph>
              <Column
                data={distribution.buckets}
                xField="label"
                yField="count"
                height={320}
                axis={{ x: { labelAutoHide: true } }}
              />
            </>
          ) : (
            <Table<BarChartCategory>
              rowKey="label"
              columns={categoryColumns}
              dataSource={distribution.categories}
              pagination={false}
              className={styles.table}
            />
          )}
        </div>
      )}
    </Card>
  );
};
