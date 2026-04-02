"use client";

import { Alert, Button, Card, Empty, Tag, Typography } from "antd";
import type { ReactNode } from "react";
import type { AIResponse } from "@/types/datasets";
import { formatDateTime } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface InsightSection {
  title: string;
  body: string;
}

interface DatasetAutomaticInsightCardProps {
  insight?: AIResponse | null;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
  onRetry?: () => void;
}

const EXPECTED_SECTION_TITLES = [
  "Summary",
  "Key data quality issues",
  "Notable patterns or anomalies",
  "Suggested next steps",
];

const normalizeSectionTitle = (value: string) => value.trim().replace(/:$/, "");

const parseInsightSections = (content?: string | null): InsightSection[] => {
  if (!content?.trim()) {
    return [];
  }

  const normalizedContent = content.replace(/\r\n/g, "\n").trim();
  const blocks = normalizedContent
    .split(/\n\s*\n/)
    .map((block) => block.trim())
    .filter((block) => block.length > 0);

  const sections = blocks
    .map((block) => {
      const [firstLine, ...remainingLines] = block.split("\n");
      const normalizedTitle = normalizeSectionTitle(firstLine);

      if (!EXPECTED_SECTION_TITLES.includes(normalizedTitle)) {
        return null;
      }

      return {
        title: normalizedTitle,
        body: remainingLines.join("\n").trim(),
      };
    })
    .filter((section): section is InsightSection => section !== null);

  return sections.length === EXPECTED_SECTION_TITLES.length ? sections : [];
};

export const DatasetAutomaticInsightCard = ({
  insight,
  isLoading = false,
  isError = false,
  errorMessage,
  onRetry,
}: DatasetAutomaticInsightCardProps) => {
  const { styles } = useStyles();
  const sections = parseInsightSections(insight?.responseContent);
  const extra: ReactNode = insight ? (
    <Tag color="blue">Generated {formatDateTime(insight.creationTime)}</Tag>
  ) : null;

  return (
    <Card loading={isLoading} className={styles.card}>
      <div className={styles.header}>
        <div>
          <Title level={4} className={styles.title}>
            AI Insight
          </Title>
          <Paragraph className={styles.helperText}>
            Automatic plain-language guidance generated from the latest
            profiling context for this dataset version.
          </Paragraph>
        </div>
        {extra}
      </div>

      {isError ? (
        <div className={styles.body}>
          <Alert
            type="error"
            showIcon
            message="Unable to load the AI insight"
            description={
              errorMessage || "Please try loading the AI insight again."
            }
          />
          {onRetry ? (
            <Button type="primary" onClick={onRetry}>
              Retry AI insight
            </Button>
          ) : null}
        </div>
      ) : !insight ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="An automatic AI insight has not been generated for this version yet."
        />
      ) : sections.length > 0 ? (
        <div className={styles.body}>
          {sections.map((section) => (
            <div key={section.title} className={styles.section}>
              <Title level={5} className={styles.sectionTitle}>
                {section.title}
              </Title>
              <Paragraph className={styles.sectionBody}>
                {section.body || "No additional detail was provided."}
              </Paragraph>
            </div>
          ))}
        </div>
      ) : (
        <Paragraph className={styles.fullText}>{insight.responseContent}</Paragraph>
      )}
    </Card>
  );
};
