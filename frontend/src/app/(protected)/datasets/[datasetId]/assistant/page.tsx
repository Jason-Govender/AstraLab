"use client";

import { useEffect, useEffectEvent } from "react";
import { Button, Card } from "antd";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { DatasetAiAssistantWorkspace } from "@/components/datasets/ai/datasetAiAssistantWorkspace";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { DatasetVersionSelector } from "@/components/datasets/details/datasetVersionSelector";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import {
  DatasetAiAssistantProvider,
  useDatasetAiAssistantActions,
  useDatasetAiAssistantState,
} from "@/providers/datasetAiAssistantProvider";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import { getDatasetProfileOverview } from "@/utils/datasets";
import { useStyles } from "./style";

const DEFAULT_CONVERSATION_PAGE_SIZE = 20;
const DEFAULT_RESPONSE_PAGE_SIZE = 100;

const parseRouteNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const DatasetAssistantContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const params = useParams<{ datasetId: string }>();
  const searchParams = useSearchParams();
  const { details, errorMessage, isError, isLoadingDetails } =
    useDatasetDetailsState();
  const { getDatasetDetails, refreshDetails } = useDatasetDetailsActions();
  const {
    activeConversationId,
    activeExperiment,
    activeDatasetVersionId,
    activeMlExperimentId,
    conversationErrorMessage,
    conversations,
    experimentContextErrorMessage,
    generationErrorMessage,
    isGenerating,
    isLoadingExperimentContext,
    isLoadingConversations,
    isLoadingResponses,
    responseErrorMessage,
    responses,
  } = useDatasetAiAssistantState();
  const {
    askQuestion,
    clearConversationState,
    generateCleaningRecommendations,
    generateInsights,
    generateSummary,
    getConversations,
    getResponses,
    loadExperimentContext,
    setActiveConversation,
  } = useDatasetAiAssistantActions();

  const datasetId = parseRouteNumber(params.datasetId);
  const selectedVersionId = parseRouteNumber(searchParams.get("versionId"));
  const selectedExperimentId = parseRouteNumber(searchParams.get("experimentId"));
  const effectiveSelectedVersionId =
    selectedVersionId ?? details?.selectedVersion?.id;
  const effectiveExperimentId = activeMlExperimentId ?? selectedExperimentId;

  const loadDetails = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getDatasetDetails(datasetId, selectedVersionId);
  });

  useEffect(() => {
    loadDetails();
  }, [datasetId, selectedVersionId]);

  const loadExperiment = useEffectEvent(() => {
    void loadExperimentContext(selectedExperimentId);
  });

  useEffect(() => {
    loadExperiment();
  }, [selectedExperimentId]);

  const loadConversations = useEffectEvent(() => {
    if (!datasetId) {
      clearConversationState();
      return;
    }

    if (!effectiveSelectedVersionId) {
      return;
    }

    void getConversations({
      datasetId,
      datasetVersionId: effectiveSelectedVersionId,
      mlExperimentId: effectiveExperimentId,
      page: 1,
      pageSize: DEFAULT_CONVERSATION_PAGE_SIZE,
    });
  });

  useEffect(() => {
    loadConversations();
  }, [datasetId, effectiveExperimentId, effectiveSelectedVersionId]);

  useEffect(() => {
    if (!datasetId || !activeExperiment) {
      return;
    }

    if (activeExperiment.datasetVersionId === effectiveSelectedVersionId) {
      return;
    }

    const nextSearchParams = new URLSearchParams(searchParams.toString());
    nextSearchParams.set("versionId", String(activeExperiment.datasetVersionId));
    nextSearchParams.set("experimentId", String(activeExperiment.id));

    router.replace(`/datasets/${datasetId}/assistant?${nextSearchParams.toString()}`, {
      scroll: false,
    });
  }, [
    activeExperiment,
    datasetId,
    effectiveSelectedVersionId,
    router,
    searchParams,
  ]);

  useEffect(() => {
    if (conversations.length === 0) {
      if (activeConversationId) {
        setActiveConversation(undefined);
      }

      return;
    }

    const activeConversationStillVisible = conversations.some(
      (conversation) => conversation.id === activeConversationId,
    );

    if (!activeConversationStillVisible) {
      setActiveConversation(conversations[0]?.id);
    }
  }, [activeConversationId, conversations, setActiveConversation]);

  const loadResponses = useEffectEvent(() => {
    if (!activeConversationId) {
      return;
    }

    void getResponses({
      conversationId: activeConversationId,
      page: 1,
      pageSize: DEFAULT_RESPONSE_PAGE_SIZE,
      isChronological: true,
    });
  });

  useEffect(() => {
    loadResponses();
  }, [activeConversationId]);

  const handleVersionChange = (versionId?: number) => {
    if (!datasetId) {
      return;
    }

    clearConversationState();

    const nextSearchParams = new URLSearchParams(searchParams.toString());

    if (versionId) {
      nextSearchParams.set("versionId", String(versionId));
    } else {
      nextSearchParams.delete("versionId");
    }

    nextSearchParams.delete("experimentId");

    const queryString = nextSearchParams.toString();
    router.push(
      queryString
        ? `/datasets/${datasetId}/assistant?${queryString}`
        : `/datasets/${datasetId}/assistant`,
    );
  };

  if (!datasetId) {
    return (
      <DatasetErrorState
        title="Invalid dataset route"
        message="The dataset identifier in the URL is not valid."
        action={
          <Button type="primary" onClick={() => router.push("/datasets")}>
            Back to datasets
          </Button>
        }
      />
    );
  }

  const profileOverview = getDatasetProfileOverview(details?.selectedVersion?.profile);
  const datasetDetailsHref = effectiveSelectedVersionId
    ? `/datasets/${datasetId}?versionId=${effectiveSelectedVersionId}`
    : `/datasets/${datasetId}`;
  const assistantDescription = activeExperiment
    ? "Ask grounded questions about the selected ML experiment, explain results in plain language, and revisit stored assistant threads anchored to this run."
    : "Ask grounded questions, generate summaries, and revisit stored assistant threads for the selected dataset version.";

  return (
    <>
      <WorkspacePageHeader
        title={details?.dataset.name || "Dataset AI Assistant"}
        description={assistantDescription}
        actions={
          <div className={styles.actionGroup}>
            <Button size="large" onClick={() => router.push(datasetDetailsHref)}>
              Back to details
            </Button>
            <Button size="large" onClick={() => router.push("/datasets")}>
              Back to datasets
            </Button>
          </div>
        }
      />

      {isError && !details ? (
        <DatasetErrorState
          title="Unable to load dataset details"
          message={errorMessage || "Please try again in a moment."}
          action={
            <Button type="primary" onClick={() => void refreshDetails()}>
              Retry
            </Button>
          }
        />
      ) : null}

      {!details && !isError ? <Card loading className={styles.loadingCard} /> : null}

      {details ? (
        <div className={styles.pageGrid}>
          <div className={styles.mainColumn}>
            <DatasetAiAssistantWorkspace
              datasetVersionId={activeDatasetVersionId ?? effectiveSelectedVersionId}
              experiment={activeExperiment}
              activeConversationId={activeConversationId}
              conversations={conversations}
              responses={responses}
              isLoadingExperimentContext={isLoadingExperimentContext}
              isLoadingConversations={isLoadingConversations}
              isLoadingResponses={isLoadingResponses}
              isGenerating={isGenerating}
              experimentContextErrorMessage={experimentContextErrorMessage}
              conversationErrorMessage={conversationErrorMessage}
              responseErrorMessage={responseErrorMessage}
              generationErrorMessage={generationErrorMessage}
              onSelectConversation={setActiveConversation}
              onGenerateSummary={generateSummary}
              onGenerateInsights={(datasetVersionId) => {
                if (activeExperiment) {
                  return askQuestion({
                    datasetVersionId,
                    mlExperimentId: activeExperiment.id,
                    question:
                      "Explain these experiment results in plain language and highlight the most important signals.",
                    conversationId: activeConversationId,
                  });
                }

                return generateInsights(datasetVersionId);
              }}
              onGenerateRecommendations={generateCleaningRecommendations}
              onAskQuestion={(question) => {
                const datasetVersionId =
                  activeDatasetVersionId ?? effectiveSelectedVersionId;

                if (!datasetVersionId) {
                  return Promise.resolve();
                }

                return askQuestion({
                  datasetVersionId,
                  question,
                  conversationId: activeConversationId,
                  mlExperimentId: activeMlExperimentId,
                });
              }}
            />
          </div>

          <div className={styles.sideColumn}>
            <DatasetVersionSelector
              versions={details.versions}
              selectedVersionId={effectiveSelectedVersionId}
              onChange={handleVersionChange}
            />
            <DatasetOverviewCard
              dataset={details.dataset}
              selectedVersion={details.selectedVersion}
            />
            <DatasetProfileSummaryCard
              profile={profileOverview}
              isLoading={isLoadingDetails}
              title="Selected Version Context"
              emptyDescription="Profiling context for the selected version will appear here when it is available."
            />
          </div>
        </div>
      ) : null}
    </>
  );
};

const DatasetAssistantPage = () => (
  <DatasetAiAssistantProvider>
    <DatasetDetailsProvider>
      <DatasetAssistantContent />
    </DatasetDetailsProvider>
  </DatasetAiAssistantProvider>
);

export default DatasetAssistantPage;
