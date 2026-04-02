"use client";

import { useDeferredValue, useEffect, useEffectEvent, useMemo, useState } from "react";
import { Alert, Button, Card, Empty, Select, Typography } from "antd";
import { useRouter, useSearchParams } from "next/navigation";
import { DatasetAiAssistantWorkspace } from "@/components/datasets/ai/datasetAiAssistantWorkspace";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { DatasetVersionSelector } from "@/components/datasets/details/datasetVersionSelector";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { WorkspaceLoadingCard } from "@/components/workspaceShell/WorkspaceLoadingCard";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import {
  DatasetAiAssistantProvider,
  useDatasetAiAssistantActions,
  useDatasetAiAssistantState,
} from "@/providers/datasetAiAssistantProvider";
import {
  DatasetCatalogProvider,
  useDatasetCatalogActions,
  useDatasetCatalogState,
} from "@/providers/datasetCatalogProvider";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import {
  MlExperimentsProvider,
  useMlExperimentsActions,
  useMlExperimentsState,
} from "@/providers/mlExperimentsProvider";
import { buildAiAssistantHref } from "@/utils/aiAssistant";
import { formatDateTime, getDatasetProfileOverview, getDatasetVersionTypeLabel } from "@/utils/datasets";
import {
  getMlAlgorithmLabel,
  getMlExperimentStatusLabel,
  getMlTaskTypeLabel,
} from "@/utils/ml";
import { useStyles } from "./style";

const { Paragraph, Text } = Typography;

const DATASET_SELECTOR_PAGE_SIZE = 100;
const DEFAULT_CONVERSATION_PAGE_SIZE = 20;
const DEFAULT_RESPONSE_PAGE_SIZE = 100;

const parseQueryNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const AiAssistantPageContent = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { styles } = useStyles();
  const [datasetKeyword, setDatasetKeyword] = useState("");
  const deferredDatasetKeyword = useDeferredValue(datasetKeyword);
  const datasetId = parseQueryNumber(searchParams.get("datasetId"));
  const selectedVersionId = parseQueryNumber(searchParams.get("versionId"));
  const selectedExperimentId = parseQueryNumber(searchParams.get("experimentId"));
  const { getDatasets, refreshDatasets } = useDatasetCatalogActions();
  const {
    datasets,
    errorMessage: datasetCatalogErrorMessage,
    isError: isDatasetCatalogError,
    isLoadingCatalog,
  } = useDatasetCatalogState();
  const { getDatasetDetails, refreshDetails } = useDatasetDetailsActions();
  const {
    details,
    errorMessage: datasetDetailsErrorMessage,
    isError: isDatasetDetailsError,
    isLoadingDetails,
  } = useDatasetDetailsState();
  const {
    clearExperiments,
    loadExperiments,
  } = useMlExperimentsActions();
  const {
    experiments,
    isLoadingExperiments,
  } = useMlExperimentsState();
  const {
    askQuestion,
    clearFeedback,
    clearConversationState,
    generateCleaningRecommendations,
    generateInsights,
    generateSummary,
    getConversations,
    getResponses,
    loadExperimentContext,
    setActiveConversation,
  } = useDatasetAiAssistantActions();
  const {
    activeConversationId,
    activeDatasetVersionId,
    activeExperiment,
    activeMlExperimentId,
    conversationErrorMessage,
    conversations,
    experimentContextErrorMessage,
    generationErrorMessage,
    lastGeneratedResponse,
    isGenerating,
    isLoadingConversations,
    isLoadingExperimentContext,
    isLoadingResponses,
    responseErrorMessage,
    responses,
  } = useDatasetAiAssistantState();

  const currentDetails =
    datasetId && details?.dataset.id === datasetId ? details : undefined;
  const selectedVersionIds = useMemo(
    () => currentDetails?.versions.map((version) => version.id) ?? [],
    [currentDetails?.versions],
  );
  const effectiveSelectedVersionId =
    selectedVersionId ?? currentDetails?.selectedVersion?.id;
  const hasVersions = (currentDetails?.versions.length ?? 0) > 0;

  const loadDatasetCatalog = useEffectEvent(() => {
    void getDatasets({
      keyword: deferredDatasetKeyword.trim(),
      page: 1,
      pageSize: DATASET_SELECTOR_PAGE_SIZE,
    });
  });

  useEffect(() => {
    loadDatasetCatalog();
  }, [deferredDatasetKeyword]);

  const loadDatasetContext = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getDatasetDetails(datasetId, selectedVersionId);
  });

  useEffect(() => {
    loadDatasetContext();
  }, [datasetId, selectedVersionId]);

  useEffect(() => {
    if (!datasetId || !currentDetails) {
      return;
    }

    if (selectedVersionId && !selectedVersionIds.includes(selectedVersionId)) {
      router.replace(
        buildAiAssistantHref({
          datasetId,
          versionId: currentDetails.selectedVersion?.id,
        }),
        { scroll: false },
      );
      return;
    }

    if (!selectedVersionId && currentDetails.selectedVersion?.id) {
      router.replace(
        buildAiAssistantHref({
          datasetId,
          versionId: currentDetails.selectedVersion.id,
        }),
        { scroll: false },
      );
    }
  }, [currentDetails, datasetId, router, selectedVersionId, selectedVersionIds]);

  const loadVersionExperiments = useEffectEvent(() => {
    if (!effectiveSelectedVersionId) {
      clearExperiments();
      return;
    }

    void loadExperiments(effectiveSelectedVersionId, selectedExperimentId);
  });

  useEffect(() => {
    loadVersionExperiments();
  }, [effectiveSelectedVersionId, selectedExperimentId]);

  const loadSelectedExperimentContext = useEffectEvent(() => {
    if (!selectedExperimentId) {
      void loadExperimentContext(undefined);
      return;
    }

    if (!effectiveSelectedVersionId || isLoadingExperiments) {
      return;
    }

    const experimentExists = experiments.some(
      (experiment) => experiment.id === selectedExperimentId,
    );

    if (!experimentExists) {
      router.replace(
        buildAiAssistantHref({
          datasetId,
          versionId: effectiveSelectedVersionId,
        }),
        { scroll: false },
      );
      return;
    }

    void loadExperimentContext(selectedExperimentId);
  });

  useEffect(() => {
    loadSelectedExperimentContext();
  }, [datasetId, effectiveSelectedVersionId, experiments, isLoadingExperiments, selectedExperimentId]);

  const loadConversationsForContext = useEffectEvent(() => {
    if (!datasetId || !effectiveSelectedVersionId) {
      clearConversationState();
      return;
    }

    void getConversations({
      datasetId,
      datasetVersionId: effectiveSelectedVersionId,
      mlExperimentId: activeMlExperimentId ?? selectedExperimentId,
      page: 1,
      pageSize: DEFAULT_CONVERSATION_PAGE_SIZE,
    });
  });

  useEffect(() => {
    loadConversationsForContext();
  }, [datasetId, effectiveSelectedVersionId, activeMlExperimentId, selectedExperimentId]);

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

  const loadResponsesForConversation = useEffectEvent(() => {
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
    loadResponsesForConversation();
  }, [activeConversationId]);

  const datasetOptions = useMemo(() => {
    const optionMap = new Map<number, { label: string; value: number }>();

    datasets.forEach((dataset) => {
      optionMap.set(dataset.id, {
        label: dataset.name,
        value: dataset.id,
      });
    });

    if (currentDetails?.dataset) {
      optionMap.set(currentDetails.dataset.id, {
        label: currentDetails.dataset.name,
        value: currentDetails.dataset.id,
      });
    }

    return Array.from(optionMap.values()).sort((left, right) =>
      left.label.localeCompare(right.label),
    );
  }, [currentDetails, datasets]);

  const versionOptions = useMemo(
    () =>
      currentDetails?.versions.map((version) => ({
        label: `v${version.versionNumber} · ${getDatasetVersionTypeLabel(version.versionType)}`,
        value: version.id,
      })) ?? [],
    [currentDetails?.versions],
  );

  const experimentOptions = useMemo(
    () =>
      experiments.map((experiment) => ({
        label: `${getMlAlgorithmLabel(experiment.algorithmKey)} · ${getMlTaskTypeLabel(experiment.taskType)} · ${getMlExperimentStatusLabel(experiment.status)}`,
        value: experiment.id,
      })),
    [experiments],
  );

  const handleDatasetChange = (nextDatasetId?: number) => {
    clearConversationState();
    router.push(
      buildAiAssistantHref({
        datasetId: nextDatasetId,
      }),
    );
  };

  const handleVersionChange = (nextVersionId?: number) => {
    clearConversationState();
    router.push(
      buildAiAssistantHref({
        datasetId,
        versionId: nextVersionId,
      }),
    );
  };

  const handleExperimentChange = (nextExperimentId?: number) => {
    clearConversationState();
    router.push(
      buildAiAssistantHref({
        datasetId,
        versionId: effectiveSelectedVersionId,
        experimentId: nextExperimentId,
      }),
    );
  };

  const profileOverview = getDatasetProfileOverview(currentDetails?.selectedVersion?.profile);
  const assistantDescription = activeExperiment
    ? "Choose a dataset, version, and optional ML experiment context to ask grounded questions, interpret model results, and revisit stored assistant threads."
    : "Choose a dataset and version to generate summaries, ask grounded questions, and revisit stored assistant threads from one shared assistant workspace.";
  const currentVersionLabel = currentDetails?.selectedVersion
    ? `v${currentDetails.selectedVersion.versionNumber}`
    : undefined;
  const currentExperimentLabel = activeExperiment
    ? getMlAlgorithmLabel(activeExperiment.algorithmKey)
    : undefined;

  return (
    <>
      <WorkspacePageHeader
        title="AI Assistant"
        description={assistantDescription}
        actions={
          <div className={styles.actionGroup}>
            <Button size="large" onClick={() => router.push("/datasets")}>
              Browse datasets
            </Button>
          </div>
        }
      />

      <div className={styles.contentStack}>
          <Card className={styles.selectionCard}>
            <div className={styles.selectionGrid}>
            <div className={styles.selectorField}>
              <Text strong>Dataset</Text>
              <Select
                allowClear
                showSearch
                size="large"
                value={datasetId}
                placeholder="Select a dataset"
                options={datasetOptions}
                filterOption={false}
                loading={isLoadingCatalog}
                onChange={handleDatasetChange}
                onSearch={setDatasetKeyword}
                notFoundContent={
                  isLoadingCatalog ? "Loading datasets..." : "No datasets found."
                }
                className={styles.selectInput}
              />
            </div>

            <div className={styles.selectorField}>
              <Text strong>Version</Text>
              <Select
                allowClear
                size="large"
                value={effectiveSelectedVersionId}
                placeholder={
                  datasetId ? "Select a dataset version" : "Choose a dataset first"
                }
                options={versionOptions}
                disabled={!datasetId || !hasVersions || isLoadingDetails}
                onChange={handleVersionChange}
                className={styles.selectInput}
              />
            </div>

            <div className={styles.selectorField}>
              <Text strong>Experiment Context</Text>
              <Select
                allowClear
                size="large"
                value={selectedExperimentId}
                placeholder={
                  effectiveSelectedVersionId
                    ? "Optional: focus on an ML experiment"
                    : "Choose a version first"
                }
                options={experimentOptions}
                disabled={!effectiveSelectedVersionId || isLoadingExperiments}
                onChange={handleExperimentChange}
                className={styles.selectInput}
              />
            </div>
          </div>

          {currentDetails?.dataset ? (
            <Paragraph className={styles.selectionMeta}>
              Working in <strong>{currentDetails.dataset.name}</strong>
              {currentVersionLabel ? ` · ${currentVersionLabel}` : ""}
              {currentExperimentLabel ? ` · ${currentExperimentLabel}` : ""}
              {currentDetails.selectedVersion?.creationTime
                ? ` · ${formatDateTime(currentDetails.selectedVersion.creationTime)}`
                : ""}
            </Paragraph>
          ) : null}
        </Card>

        {isDatasetCatalogError ? (
          <Alert
            type="error"
            showIcon
            message="Unable to load datasets"
            description={
              datasetCatalogErrorMessage || "Please try loading the datasets again."
            }
            action={
              <Button size="small" onClick={() => void refreshDatasets()}>
                Retry
              </Button>
            }
          />
        ) : null}

        {!datasetId ? (
          <Card className={styles.selectionCard}>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="Select a dataset to start an AI conversation, then narrow the context with a version or experiment if needed."
            />
          </Card>
        ) : null}

        {datasetId && isDatasetDetailsError && !currentDetails ? (
          <DatasetErrorState
            title="Unable to load assistant dataset context"
            message={
              datasetDetailsErrorMessage || "Please try loading the dataset again."
            }
            action={
              <Button type="primary" onClick={() => void refreshDetails()}>
                Retry
              </Button>
            }
          />
        ) : null}

        {datasetId && !currentDetails && !isDatasetDetailsError ? (
          <WorkspaceLoadingCard className={styles.loadingCard} />
        ) : null}

        {datasetId && currentDetails && !hasVersions ? (
          <Card className={styles.selectionCard}>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="This dataset does not have any versions yet."
            />
          </Card>
        ) : null}

        {datasetId && currentDetails && hasVersions ? (
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
                lastGeneratedResponse={lastGeneratedResponse}
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
                    return Promise.resolve(false);
                  }

                  return askQuestion({
                    datasetVersionId,
                    question,
                    conversationId: activeConversationId,
                    mlExperimentId: activeMlExperimentId,
                  });
                }}
                onClearFeedback={clearFeedback}
              />
            </div>

            <div className={styles.sideColumn}>
              <DatasetVersionSelector
                versions={currentDetails.versions}
                selectedVersionId={effectiveSelectedVersionId}
                onChange={handleVersionChange}
              />
              <DatasetOverviewCard
                dataset={currentDetails.dataset}
                selectedVersion={currentDetails.selectedVersion}
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
      </div>
    </>
  );
};

const AiAssistantPage = () => (
  <DatasetCatalogProvider>
    <DatasetDetailsProvider>
      <MlExperimentsProvider>
        <DatasetAiAssistantProvider>
          <AiAssistantPageContent />
        </DatasetAiAssistantProvider>
      </MlExperimentsProvider>
    </DatasetDetailsProvider>
  </DatasetCatalogProvider>
);

export default AiAssistantPage;
