"use client";

import { Column } from "@ant-design/charts";
import { Card, Empty, Typography } from "antd";
import type { HistogramChart } from "@/types/datasets";
import { formatNumber } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetHistogramChartCardProps {
  histogram?: HistogramChart;
  isLoading?: boolean;
}

export const DatasetHistogramChartCard = ({
  histogram,
  isLoading = false,
}: DatasetHistogramChartCardProps) => {
  const { styles } = useStyles();

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Histogram
      </Title>

      {!histogram || histogram.buckets.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Run a histogram analysis on a numeric column to visualize bucketed value counts."
        />
      ) : (
        <>
          <Paragraph className={styles.helperText}>
            {histogram.columnName} · {formatNumber(histogram.valueCount, 0)} values ·{" "}
            {formatNumber(histogram.nullCount, 0)} nulls
          </Paragraph>
          <Column
            data={histogram.buckets}
            xField="label"
            yField="count"
            height={320}
            axis={{ x: { labelAutoHide: true } }}
          />
        </>
      )}
    </Card>
  );
};
