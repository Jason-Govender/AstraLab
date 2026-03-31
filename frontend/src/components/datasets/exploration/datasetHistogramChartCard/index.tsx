"use client";

import { Column } from "@ant-design/charts";
import { Card, Empty, Typography, theme } from "antd";
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
  const { token } = theme.useToken();

  const histogramChartConfig = histogram
    ? {
        data: histogram.buckets,
        xField: "label",
        yField: "count",
        height: 320,
        color: token.colorPrimary,
        paddingLeft: 56,
        paddingRight: 16,
        paddingTop: 24,
        paddingBottom: 72,
        style: {
          fillOpacity: 0.9,
          lineWidth: 1,
          stroke: token.colorPrimaryBorder,
        },
        axis: {
          x: {
            title: "Value range",
            labelAutoHide: true,
            labelAutoRotate: false,
            tick: true,
            line: true,
            labelFill: token.colorTextSecondary,
            titleFill: token.colorTextSecondary,
            lineStroke: token.colorBorderSecondary,
            tickStroke: token.colorBorderSecondary,
          },
          y: {
            title: "Count",
            tick: true,
            line: true,
            grid: true,
            labelFill: token.colorTextSecondary,
            titleFill: token.colorTextSecondary,
            lineStroke: token.colorBorderSecondary,
            tickStroke: token.colorBorderSecondary,
            gridStroke: "rgba(148, 163, 184, 0.16)",
            gridLineWidth: 1,
          },
        },
      }
    : undefined;

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
          <Column {...histogramChartConfig} />
        </>
      )}
    </Card>
  );
};
