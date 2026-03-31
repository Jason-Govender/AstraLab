"use client";

import { Bar } from "@ant-design/charts";
import { Card, Empty, Typography, theme } from "antd";
import type { BarChart } from "@/types/datasets";
import { formatNumber } from "@/utils/datasets";
import { getCategoryChartData } from "@/utils/datasetExploration";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetBarChartCardProps {
  barChart?: BarChart;
  isLoading?: boolean;
}

export const DatasetBarChartCard = ({
  barChart,
  isLoading = false,
}: DatasetBarChartCardProps) => {
  const { styles } = useStyles();
  const { token } = theme.useToken();

  const barChartConfig = barChart
    ? {
        data: getCategoryChartData(barChart.categories),
        xField: "count",
        yField: "label",
        height: 320,
        theme: "classicDark",
        paddingLeft: 120,
        paddingRight: 24,
        paddingTop: 24,
        paddingBottom: 32,
        style: {
          fill: token.colorPrimary,
          fillOpacity: 0.9,
          radiusTopRight: 6,
          radiusBottomRight: 6,
          lineWidth: 1,
          stroke: token.colorPrimaryBorder,
          maxWidth: 32,
        },
        axis: {
          x: {
            title: "Count",
            tick: true,
            line: true,
            grid: true,
            labelAutoHide: false,
            labelFill: token.colorTextSecondary,
            titleFill: token.colorTextSecondary,
            lineStroke: token.colorBorderSecondary,
            tickStroke: token.colorBorderSecondary,
            gridStroke: "rgba(148, 163, 184, 0.16)",
            gridLineWidth: 1,
          },
          y: {
            title: "Category",
            tick: false,
            line: true,
            labelAutoHide: false,
            labelAutoEllipsis: true,
            labelSpacing: 8,
            labelFill: token.colorTextSecondary,
            titleFill: token.colorTextSecondary,
            lineStroke: token.colorBorderSecondary,
          },
        },
      }
    : undefined;

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Bar Chart
      </Title>

      {!barChart || barChart.categories.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Run a bar chart analysis on a categorical column to visualize category frequency."
        />
      ) : (
        <>
          <Paragraph className={styles.helperText}>
            {barChart.columnName} · {formatNumber(barChart.distinctCategoryCount, 0)} distinct categories ·{" "}
            {formatNumber(barChart.nullCount, 0)} nulls
          </Paragraph>
          <Bar {...barChartConfig} />
        </>
      )}
    </Card>
  );
};
