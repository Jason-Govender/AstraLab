"use client";

import { Bar } from "@ant-design/charts";
import { Card, Empty, Typography } from "antd";
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
          <Bar
            data={getCategoryChartData(barChart.categories)}
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
