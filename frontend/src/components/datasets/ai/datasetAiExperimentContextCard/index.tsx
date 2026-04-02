"use client";

import { Alert, Card, Descriptions, Empty, Tag, Typography } from "antd";
import type { MlExperiment } from "@/types/ml";
import { formatDateTime, formatNumber } from "@/utils/datasets";
import {
  getMlAlgorithmLabel,
  getMlExperimentStatusColor,
  getMlExperimentStatusLabel,
  getMlTaskTypeLabel,
} from "@/utils/ml";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DatasetAiExperimentContextCardProps {
  experiment?: MlExperiment;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
}

export const DatasetAiExperimentContextCard = ({
  experiment,
  isLoading = false,
  isError = false,
  errorMessage,
}: DatasetAiExperimentContextCardProps) => {
  const { styles } = useStyles();
  const metrics = experiment?.model?.metrics.slice(0, 4) ?? [];
  const featureImportances =
    experiment?.model?.featureImportances.slice(0, 4) ?? [];

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4}>Experiment context</Title>

      {isError ? (
        <Alert
          type="error"
          showIcon
          message="Unable to load the selected ML experiment"
          description={errorMessage || "Please try opening the experiment again."}
        />
      ) : !experiment ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Select or launch the assistant from an experiment to ground the thread in ML results."
        />
      ) : (
        <>
          <Paragraph className={styles.helperText}>
            The assistant is grounded in this experiment&apos;s configuration,
            metrics, feature importance, and warnings.
          </Paragraph>

          <Descriptions
            size="small"
            column={1}
            items={[
              {
                key: "workflow",
                label: "Workflow",
                children: getMlTaskTypeLabel(experiment.taskType),
              },
              {
                key: "algorithm",
                label: "Algorithm",
                children: getMlAlgorithmLabel(experiment.algorithmKey),
              },
              {
                key: "target",
                label: "Target",
                children: experiment.targetColumnName || "None",
              },
              {
                key: "status",
                label: "Status",
                children: (
                  <Tag color={getMlExperimentStatusColor(experiment.status)}>
                    {getMlExperimentStatusLabel(experiment.status)}
                  </Tag>
                ),
              },
              {
                key: "completed",
                label: "Completed",
                children: experiment.completedAtUtc
                  ? formatDateTime(experiment.completedAtUtc)
                  : "Not finished",
              },
            ]}
          />

          <Paragraph>
            <Text strong>Features</Text>
          </Paragraph>
          <div className={styles.tagWrap}>
            {experiment.features.map((feature) => (
              <Tag key={feature.datasetColumnId}>
                {feature.name || `Column ${feature.datasetColumnId}`}
              </Tag>
            ))}
          </div>

          {metrics.length > 0 ? (
            <>
              <Paragraph>
                <Text strong>Top metrics</Text>
              </Paragraph>
              <div className={styles.metricList}>
                {metrics.map((metric) => (
                  <div key={metric.metricName} className={styles.metricRow}>
                    <Text>{metric.metricName}</Text>
                    <Text strong>{formatNumber(metric.metricValue, 4)}</Text>
                  </div>
                ))}
              </div>
            </>
          ) : null}

          {featureImportances.length > 0 ? (
            <>
              <Paragraph>
                <Text strong>Top feature importance</Text>
              </Paragraph>
              <div className={styles.metricList}>
                {featureImportances.map((item) => (
                  <div
                    key={`${item.datasetColumnId}-${item.rank}`}
                    className={styles.metricRow}
                  >
                    <Text>
                      {item.columnName || `Column ${item.datasetColumnId}`}
                    </Text>
                    <Text strong>{formatNumber(item.importanceScore, 4)}</Text>
                  </div>
                ))}
              </div>
            </>
          ) : null}
        </>
      )}
    </Card>
  );
};
