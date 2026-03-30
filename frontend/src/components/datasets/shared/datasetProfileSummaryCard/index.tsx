"use client";

import { Card, Empty, Progress, Tag, Typography } from "antd";
import type { DatasetProfileOverview } from "@/utils/datasets";
import {
  formatDateTime,
  formatNumber,
  formatPercentage,
  getDataHealthScoreTone,
} from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetProfileSummaryCardProps {
  profile?: DatasetProfileOverview | null;
  isLoading?: boolean;
  title?: string;
  emptyDescription?: string;
  helperText?: string;
}

interface MetricItem {
  key: string;
  label: string;
  value: string;
}

export const DatasetProfileSummaryCard = ({
  profile,
  isLoading = false,
  title = "Profiling Summary",
  emptyDescription = "Profiling insights are not available for this dataset version yet.",
  helperText,
}: DatasetProfileSummaryCardProps) => {
  const { styles } = useStyles();
  const healthScore = profile?.dataHealthScore ?? 0;
  const metrics: MetricItem[] = [
    profile?.rowCount === null || profile?.rowCount === undefined
      ? null
      : {
          key: "rows",
          label: "Rows",
          value: formatNumber(profile.rowCount, 0),
        },
    profile?.duplicateRowCount === null || profile?.duplicateRowCount === undefined
      ? null
      : {
          key: "duplicates",
          label: "Duplicate Rows",
          value: formatNumber(profile.duplicateRowCount, 0),
        },
    profile?.profiledColumnCount === null || profile?.profiledColumnCount === undefined
      ? null
      : {
          key: "profiled-columns",
          label: "Profiled Columns",
          value: formatNumber(profile.profiledColumnCount, 0),
        },
    profile?.totalNullCount === null || profile?.totalNullCount === undefined
      ? null
      : {
          key: "null-cells",
          label: "Null Cells",
          value: formatNumber(profile.totalNullCount, 0),
        },
    profile?.overallNullPercentage === null ||
    profile?.overallNullPercentage === undefined
      ? null
      : {
          key: "null-rate",
          label: "Overall Null Rate",
          value: formatPercentage(profile.overallNullPercentage),
        },
    profile?.totalAnomalyCount === null || profile?.totalAnomalyCount === undefined
      ? null
      : {
          key: "anomalies",
          label: "Detected Anomalies",
          value: formatNumber(profile.totalAnomalyCount, 0),
        },
    profile?.overallAnomalyPercentage === null ||
    profile?.overallAnomalyPercentage === undefined
      ? null
      : {
          key: "anomaly-rate",
          label: "Overall Anomaly Rate",
          value: formatPercentage(profile.overallAnomalyPercentage),
        },
  ].filter((metric): metric is MetricItem => metric !== null);

  return (
    <Card loading={isLoading} className={styles.card}>
      <div className={styles.header}>
        <div>
          <Title level={4} className={styles.title}>
            {title}
          </Title>
          {helperText ? (
            <Paragraph className={styles.helperText}>{helperText}</Paragraph>
          ) : null}
        </div>
        {profile ? (
          <Tag color={getDataHealthScoreTone(profile.dataHealthScore)}>
            Health Score {formatNumber(profile.dataHealthScore)}
          </Tag>
        ) : null}
      </div>

      {!profile ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={emptyDescription}
        />
      ) : (
        <>
          <div className={styles.scoreSection}>
            <div className={styles.progressWrap}>
              <Progress
                type="dashboard"
                percent={Math.max(0, Math.min(100, Number(healthScore)))}
                status={
                  healthScore >= 85
                    ? "success"
                    : healthScore >= 60
                      ? "active"
                      : "exception"
                }
              />
            </div>

            <div className={styles.scoreMeta}>
              <div>
                <Paragraph className={styles.helperText}>
                  Backend-calculated data health score
                </Paragraph>
                <Title level={2} className={styles.scoreValue}>
                  {formatNumber(profile.dataHealthScore)}
                </Title>
              </div>

              {profile.creationTime ? (
                <Paragraph className={styles.timestamp}>
                  Profiled {formatDateTime(profile.creationTime)}
                </Paragraph>
              ) : null}
            </div>
          </div>

          <div className={styles.metricsGrid}>
            {metrics.map((metric) => (
              <div key={metric.key} className={styles.metricCard}>
                <span className={styles.metricLabel}>{metric.label}</span>
                <span className={styles.metricValue}>{metric.value}</span>
              </div>
            ))}
          </div>
        </>
      )}
    </Card>
  );
};
