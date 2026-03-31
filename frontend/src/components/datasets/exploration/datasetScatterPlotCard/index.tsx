"use client";

import { Scatter } from "@ant-design/charts";
import { Card, Empty, Typography, theme } from "antd";
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
  const { token } = theme.useToken();

  const scatterPlotConfig = scatterPlot
    ? {
        data: scatterPlot.points,
        xField: "x",
        yField: "y",
        height: 320,
        theme: "classicDark",
        paddingLeft: 64,
        paddingRight: 24,
        paddingTop: 24,
        paddingBottom: 48,
        tooltip: {
          title: false,
          items: [
            { channel: "x", name: scatterPlot.xColumnName },
            { channel: "y", name: scatterPlot.yColumnName },
          ],
        },
        style: {
          fill: token.colorPrimary,
          fillOpacity: 0.3,
          stroke: token.colorPrimary,
          lineWidth: 1.25,
        },
        axis: {
          x: {
            title: scatterPlot.xColumnName,
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
          y: {
            title: scatterPlot.yColumnName,
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
          <Scatter {...scatterPlotConfig} />
        </>
      )}
    </Card>
  );
};
