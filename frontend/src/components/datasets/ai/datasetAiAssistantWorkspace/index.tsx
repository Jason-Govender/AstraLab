"use client";

import { Alert } from "antd";
import type { AIConversation, AIResponse } from "@/types/datasets";
import { DatasetAiConversationListCard } from "../datasetAiConversationListCard";
import { DatasetAiPromptComposer } from "../datasetAiPromptComposer";
import { DatasetAiQuickActionsCard } from "../datasetAiQuickActionsCard";
import { DatasetAiResponseThreadCard } from "../datasetAiResponseThreadCard";
import { useStyles } from "./style";

interface DatasetAiAssistantWorkspaceProps {
  datasetVersionId?: number;
  activeConversationId?: number;
  conversations: AIConversation[];
  responses: AIResponse[];
  isLoadingConversations?: boolean;
  isLoadingResponses?: boolean;
  isGenerating?: boolean;
  conversationErrorMessage?: string;
  responseErrorMessage?: string;
  generationErrorMessage?: string;
  onSelectConversation: (conversationId?: number) => void;
  onGenerateSummary: (datasetVersionId: number) => Promise<void>;
  onGenerateInsights: (datasetVersionId: number) => Promise<void>;
  onGenerateRecommendations: (datasetVersionId: number) => Promise<void>;
  onAskQuestion: (question: string) => Promise<void>;
}

export const DatasetAiAssistantWorkspace = ({
  datasetVersionId,
  activeConversationId,
  conversations,
  responses,
  isLoadingConversations = false,
  isLoadingResponses = false,
  isGenerating = false,
  conversationErrorMessage,
  responseErrorMessage,
  generationErrorMessage,
  onSelectConversation,
  onGenerateSummary,
  onGenerateInsights,
  onGenerateRecommendations,
  onAskQuestion,
}: DatasetAiAssistantWorkspaceProps) => {
  const { styles } = useStyles();

  return (
    <div className={styles.layout}>
      <div className={styles.sideColumn}>
        <DatasetAiConversationListCard
          conversations={conversations}
          activeConversationId={activeConversationId}
          isLoading={isLoadingConversations}
          isError={Boolean(conversationErrorMessage)}
          errorMessage={conversationErrorMessage}
          onSelectConversation={onSelectConversation}
        />
      </div>

      <div className={styles.mainColumn}>
        {generationErrorMessage ? (
          <Alert
            type="error"
            showIcon
            message="Unable to generate the AI response"
            description={generationErrorMessage}
          />
        ) : null}

        <DatasetAiQuickActionsCard
          datasetVersionId={datasetVersionId}
          isLoading={isGenerating}
          onGenerateSummary={onGenerateSummary}
          onGenerateInsights={onGenerateInsights}
          onGenerateRecommendations={onGenerateRecommendations}
        />
        <DatasetAiPromptComposer
          datasetVersionId={datasetVersionId}
          activeConversationId={activeConversationId}
          isSubmitting={isGenerating}
          onSubmit={onAskQuestion}
        />
        <DatasetAiResponseThreadCard
          responses={responses}
          activeConversationId={activeConversationId}
          isLoading={isLoadingResponses}
          isError={Boolean(responseErrorMessage)}
          errorMessage={responseErrorMessage}
        />
      </div>
    </div>
  );
};
