"use client";

import { Scatter } from "@ant-design/charts";
import { Card, Empty, Typography } from "antd";
import type { ScatterPlot } from "@/types/datasets";
import { formatNumber } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetScatterPlotCardProps {
  scatterPlot?: ScatterPlot;
  isLoading?: boolean;
}

export const DatasetScatterPlotCard = ({
  scatterPlot,
  isLoading = false,
}: DatasetScatterPlotCardProps) => {
  const { styles } = useStyles();

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Scatter Plot
      </Title>

      {!scatterPlot || scatterPlot.points.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Run a scatter plot analysis on two numeric columns to inspect row-level relationships."
        />
      ) : (
        <>
          <Paragraph className={styles.helperText}>
            {scatterPlot.xColumnName} vs {scatterPlot.yColumnName} ·{" "}
            {formatNumber(scatterPlot.pointCount, 0)} plotted rows
          </Paragraph>
          <Scatter
            data={scatterPlot.points}
            xField="x"
            yField="y"
            colorField="rowNumber"
            height={320}
          />
        </>
      )}
    </Card>
  );
};
