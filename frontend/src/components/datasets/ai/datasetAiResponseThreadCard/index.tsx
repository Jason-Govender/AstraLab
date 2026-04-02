"use client";

import { Alert, Card, Empty, Tag, Typography } from "antd";
import { AIResponseType, type AIResponse } from "@/types/datasets";
import { formatDateTime } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DatasetAiResponseThreadCardProps {
  responses: AIResponse[];
  activeConversationId?: number;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
}

const getResponseTypeLabel = (responseType: AIResponseType) => {
  switch (responseType) {
    case AIResponseType.Summary:
      return "Summary";
    case AIResponseType.Recommendation:
      return "Recommendation";
    case AIResponseType.Explanation:
      return "Explanation";
    case AIResponseType.Insight:
      return "Insight";
    case AIResponseType.QuestionAnswer:
      return "Q&A";
    default:
      return "AI response";
  }
};

export const DatasetAiResponseThreadCard = ({
  responses,
  activeConversationId,
  isLoading = false,
  isError = false,
  errorMessage,
}: DatasetAiResponseThreadCardProps) => {
  const { styles, cx } = useStyles();

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4}>Assistant thread</Title>

      {isError ? (
        <Alert
          type="error"
          showIcon
          message="Unable to load the stored assistant thread"
          description={errorMessage || "Please try opening the thread again."}
        />
      ) : !activeConversationId ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Open an existing conversation or create a new assistant response to start a thread."
        />
      ) : responses.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="No stored assistant turns are available in this conversation yet."
        />
      ) : (
        <div className={styles.thread}>
          {responses.map((response) => (
            <div key={response.id} className={styles.turn}>
              {response.userQuery?.trim() ? (
                <div className={cx(styles.bubble, styles.userBubble)}>
                  <div className={styles.bubbleHeader}>
                    <Text strong>You</Text>
                    <Text type="secondary">{formatDateTime(response.creationTime)}</Text>
                  </div>
                  <Paragraph className={styles.bubbleText}>
                    {response.userQuery}
                  </Paragraph>
                </div>
              ) : null}

              <div className={cx(styles.bubble, styles.assistantBubble)}>
                <div className={styles.bubbleHeader}>
                  <Text strong>Assistant</Text>
                  <Tag>{getResponseTypeLabel(response.responseType)}</Tag>
                </div>
                <Paragraph className={styles.bubbleText}>
                  {response.responseContent}
                </Paragraph>
                <Paragraph className={styles.helperText}>
                  Generated {formatDateTime(response.creationTime)}
                </Paragraph>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
