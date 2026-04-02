import { Card, Typography } from "antd";
import { BrandLogo } from "@/components/BrandLogo";
import { useStyles } from "../style";

const { Paragraph, Text, Title } = Typography;
interface Metric {
  label: string;
  value: string;
}

interface LoginHeroProps {
  metrics: Metric[];
}

export function LoginHero({ metrics }: LoginHeroProps) {
  const { styles } = useStyles();

  return (
    <div className={styles.hero}>
      <div className={styles.brand}>
        <BrandLogo variant="auth" priority />
      </div>
      <Title level={1} className={styles.heroTitle}>
        Turn raw data into
        <span className={styles.heroAccent}> intelligent insight</span>
      </Title>
      <Paragraph className={styles.heroDescription}>
        Profile datasets, clean data, chat with AI, run machine learning
        experiments, and generate reports from one unified workspace.
      </Paragraph>

      <div className={styles.metrics}>
        {metrics.map((metric) => (
          <Card key={metric.label} bordered className={styles.metricCard}>
            <Text className={styles.metricLabel}>{metric.label}</Text>
            <Text className={styles.metricValue}>{metric.value}</Text>
          </Card>
        ))}
      </div>

      <Card bordered className={styles.summaryCard}>
        <Title level={4} className={styles.summaryTitle}>
          One platform for data profiling, AI analysis, and ML workflows
        </Title>
        <Paragraph className={styles.summaryDescription}>
          Built for analysts, engineers, and teams who need clarity fast.
        </Paragraph>
      </Card>
    </div>
  );
}
