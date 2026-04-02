"use client";

import { Alert, Card, Empty, Tag, Typography } from "antd";
import type { AIConversation } from "@/types/datasets";
import { formatRelativeTime } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DatasetAiConversationListCardProps {
  conversations: AIConversation[];
  activeConversationId?: number;
  isExperimentScoped?: boolean;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
  onSelectConversation: (conversationId?: number) => void;
}

export const DatasetAiConversationListCard = ({
  conversations,
  activeConversationId,
  isExperimentScoped = false,
  isLoading = false,
  isError = false,
  errorMessage,
  onSelectConversation,
}: DatasetAiConversationListCardProps) => {
  const { styles, cx } = useStyles();

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4}>Stored conversations</Title>
      <Paragraph className={styles.helperText}>
        Reopen the most relevant thread for this context and keep the assistant grounded in prior turns instead of starting from scratch.
      </Paragraph>

      {isError ? (
        <Alert
          type="error"
          showIcon
          message="Unable to load stored conversations"
          description={errorMessage || "Please try loading the assistant again."}
        />
      ) : conversations.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            isExperimentScoped
              ? "No AI conversations have been created for this experiment yet."
              : "No AI conversations have been created for this dataset version yet."
          }
        />
      ) : (
        <div className={styles.list}>
          {conversations.map((conversation) => (
            <button
              key={conversation.id}
              type="button"
              className={cx(
                styles.item,
                activeConversationId === conversation.id && styles.activeItem,
              )}
              onClick={() => onSelectConversation(conversation.id)}
            >
              <div className={styles.itemHeader}>
                <Text strong>
                  {conversation.latestUserQuery?.trim() || "Assistant response"}
                </Text>
                <Tag>{conversation.responseCount} turns</Tag>
              </div>
              <Paragraph className={styles.preview}>
                {conversation.latestResponsePreview ||
                  "Open this thread to view the stored assistant response."}
              </Paragraph>
              <Text className={styles.metaText}>
                Updated {formatRelativeTime(conversation.lastInteractionTime)}
              </Text>
            </button>
          ))}
        </div>
      )}
    </Card>
  );
};
