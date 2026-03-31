"use client";

import { Card, Typography } from "antd";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

export const DatasetExplorationGuideCard = () => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Exploration Guide
      </Title>
      <Paragraph className={styles.helperText}>
        Rows are loaded directly from the selected dataset version so you can inspect raw and processed outputs without leaving the datasets workspace.
      </Paragraph>
      <Paragraph className={styles.helperText}>
        Analysis tabs only enable columns the backend says are valid for that chart or statistic.
      </Paragraph>
      <Paragraph className={styles.helperText}>
        Histograms, distributions, scatter plots, and correlations all come from backend computation, so the frontend stays aligned with the canonical dataset rules.
      </Paragraph>
    </Card>
  );
};
