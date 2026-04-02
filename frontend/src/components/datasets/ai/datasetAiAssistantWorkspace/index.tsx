"use client";

import { AIResponseType, type AIConversation, type AIResponse } from "@/types/datasets";
import type { MlExperiment } from "@/types/ml";
import { WorkspaceFeedbackAlert } from "@/components/workspaceShell/WorkspaceFeedbackAlert";
import { DatasetAiConversationListCard } from "../datasetAiConversationListCard";
import { DatasetAiExperimentContextCard } from "../datasetAiExperimentContextCard";
import { DatasetAiPromptComposer } from "../datasetAiPromptComposer";
import { DatasetAiQuickActionsCard } from "../datasetAiQuickActionsCard";
import { DatasetAiResponseThreadCard } from "../datasetAiResponseThreadCard";
import { useStyles } from "./style";

interface DatasetAiAssistantWorkspaceProps {
  datasetVersionId?: number;
  experiment?: MlExperiment;
  activeConversationId?: number;
  conversations: AIConversation[];
  responses: AIResponse[];
  isLoadingExperimentContext?: boolean;
  isLoadingConversations?: boolean;
  isLoadingResponses?: boolean;
  isGenerating?: boolean;
  experimentContextErrorMessage?: string;
  conversationErrorMessage?: string;
  responseErrorMessage?: string;
  generationErrorMessage?: string;
  lastGeneratedResponse?: AIResponse;
  onSelectConversation: (conversationId?: number) => void;
  onGenerateSummary: (datasetVersionId: number) => Promise<unknown>;
  onGenerateInsights: (datasetVersionId: number) => Promise<unknown>;
  onGenerateRecommendations: (datasetVersionId: number) => Promise<unknown>;
  onAskQuestion: (question: string) => Promise<boolean>;
  onClearFeedback?: () => void;
}

const getGenerationSuccessDescription = (response: AIResponse): string => {
  switch (response.responseType) {
    case AIResponseType.Summary:
      return "The latest summary has been added to the current conversation.";
    case AIResponseType.Recommendation:
      return "The latest recommendations have been added to the current conversation.";
    case AIResponseType.Explanation:
      return "The latest explanation has been added to the current conversation.";
    case AIResponseType.Insight:
      return "The latest insight has been added to the current conversation.";
    case AIResponseType.QuestionAnswer:
    default:
      return "The assistant added a new response to the current conversation.";
  }
};

export const DatasetAiAssistantWorkspace = ({
  datasetVersionId,
  experiment,
  activeConversationId,
  conversations,
  responses,
  isLoadingExperimentContext = false,
  isLoadingConversations = false,
  isLoadingResponses = false,
  isGenerating = false,
  experimentContextErrorMessage,
  conversationErrorMessage,
  responseErrorMessage,
  generationErrorMessage,
  lastGeneratedResponse,
  onSelectConversation,
  onGenerateSummary,
  onGenerateInsights,
  onGenerateRecommendations,
  onAskQuestion,
  onClearFeedback,
}: DatasetAiAssistantWorkspaceProps) => {
  const { styles } = useStyles();
  const isExperimentScoped = Boolean(experiment);

  return (
    <div className={styles.layout}>
      <div className={styles.sideColumn}>
        <DatasetAiExperimentContextCard
          experiment={experiment}
          isLoading={isLoadingExperimentContext}
          isError={Boolean(experimentContextErrorMessage)}
          errorMessage={experimentContextErrorMessage}
        />
        <DatasetAiConversationListCard
          conversations={conversations}
          activeConversationId={activeConversationId}
          isExperimentScoped={isExperimentScoped}
          isLoading={isLoadingConversations}
          isError={Boolean(conversationErrorMessage)}
          errorMessage={conversationErrorMessage}
          onSelectConversation={onSelectConversation}
        />
      </div>

      <div className={styles.mainColumn}>
        {isGenerating ? (
          <WorkspaceFeedbackAlert
            type="info"
            title="Currently running"
            description="The assistant is preparing a response for the selected dataset context. This can take a few moments."
          />
        ) : null}

        {lastGeneratedResponse && !isGenerating ? (
          <WorkspaceFeedbackAlert
            type="success"
            title="Action completed"
            description={getGenerationSuccessDescription(lastGeneratedResponse)}
            closable
            onClose={onClearFeedback}
          />
        ) : null}

        {generationErrorMessage ? (
          <WorkspaceFeedbackAlert
            type="error"
            title="Action failed"
            description={generationErrorMessage}
            closable
            onClose={onClearFeedback}
          />
        ) : null}

        <DatasetAiQuickActionsCard
          datasetVersionId={datasetVersionId}
          isExperimentScoped={isExperimentScoped}
          isLoading={isGenerating}
          onGenerateSummary={onGenerateSummary}
          onGenerateInsights={onGenerateInsights}
          onGenerateRecommendations={onGenerateRecommendations}
        />
        <DatasetAiPromptComposer
          datasetVersionId={datasetVersionId}
          activeConversationId={activeConversationId}
          isSubmitting={isGenerating}
          onInteraction={onClearFeedback}
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
