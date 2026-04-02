"use client";

import { Button, Card, Typography } from "antd";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetAiQuickActionsCardProps {
  datasetVersionId?: number;
  isExperimentScoped?: boolean;
  isLoading?: boolean;
  onGenerateSummary: (datasetVersionId: number) => Promise<unknown>;
  onGenerateInsights: (datasetVersionId: number) => Promise<unknown>;
  onGenerateRecommendations: (datasetVersionId: number) => Promise<unknown>;
}

export const DatasetAiQuickActionsCard = ({
  datasetVersionId,
  isExperimentScoped = false,
  isLoading = false,
  onGenerateSummary,
  onGenerateInsights,
  onGenerateRecommendations,
}: DatasetAiQuickActionsCardProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={4}>Quick AI actions</Title>
      <Paragraph className={styles.helperText}>
        Start with a guided action if you want the assistant to do the first pass for you.
      </Paragraph>
      <div className={styles.actions}>
        <Button
          type="primary"
          loading={isLoading}
          disabled={!datasetVersionId}
          onClick={() => datasetVersionId && void onGenerateSummary(datasetVersionId)}
        >
          {isExperimentScoped ? "Summarize this experiment" : "Summarize this dataset"}
        </Button>
        <Button
          loading={isLoading}
          disabled={!datasetVersionId}
          onClick={() => datasetVersionId && void onGenerateInsights(datasetVersionId)}
        >
          {isExperimentScoped ? "Explain these results" : "Explain key issues"}
        </Button>
        <Button
          loading={isLoading}
          disabled={!datasetVersionId}
          onClick={() =>
            datasetVersionId && void onGenerateRecommendations(datasetVersionId)
          }
        >
          {isExperimentScoped ? "Recommend next steps" : "Recommend cleaning steps"}
        </Button>
      </div>
    </Card>
  );
};
