"use client";

import { Heatmap } from "@ant-design/charts";
import { Card, Empty, Table, Tag, Typography } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { CorrelationAnalysis, CorrelationPair } from "@/types/datasets";
import { formatNumber } from "@/utils/datasets";
import {
  buildCorrelationHeatmapData,
  getCorrelationCoefficientTone,
} from "@/utils/datasetExploration";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetCorrelationMatrixCardProps {
  correlation?: CorrelationAnalysis;
  isLoading?: boolean;
}

export const DatasetCorrelationMatrixCard = ({
  correlation,
  isLoading = false,
}: DatasetCorrelationMatrixCardProps) => {
  const { styles } = useStyles();

  const pairColumns: ColumnsType<CorrelationPair> = [
    {
      title: "X Column",
      dataIndex: "xColumnName",
      key: "xColumnName",
    },
    {
      title: "Y Column",
      dataIndex: "yColumnName",
      key: "yColumnName",
    },
    {
      title: "Coefficient",
      dataIndex: "coefficient",
      key: "coefficient",
      width: 160,
      render: (value?: number | null) => (
        <Tag color={getCorrelationCoefficientTone(value)}>
          {value === null || value === undefined ? "Unavailable" : formatNumber(value, 4)}
        </Tag>
      ),
    },
    {
      title: "Sample Size",
      dataIndex: "sampleSize",
      key: "sampleSize",
      width: 140,
      render: (value: number) => formatNumber(value, 0),
    },
  ];

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Correlation Analysis
      </Title>

      {!correlation || correlation.pairs.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Run a correlation analysis on two or more numeric columns to inspect pairwise Pearson coefficients."
        />
      ) : (
        <div className={styles.stack}>
          <Paragraph className={styles.helperText}>
            Backend-calculated Pearson correlation pairs for the selected numeric columns.
          </Paragraph>
          <Heatmap
            data={buildCorrelationHeatmapData(correlation)}
            xField="xColumnName"
            yField="yColumnName"
            colorField="coefficient"
            height={320}
          />
          <Table<CorrelationPair>
            rowKey={(pair) => `${pair.xDatasetColumnId}-${pair.yDatasetColumnId}`}
            columns={pairColumns}
            dataSource={correlation.pairs}
            pagination={false}
            className={styles.table}
          />
        </div>
      )}
    </Card>
  );
};
