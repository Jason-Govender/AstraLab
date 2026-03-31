"use client";

import { useEffect, useEffectEvent } from "react";
import { Button, Card } from "antd";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { DatasetVersionSelector } from "@/components/datasets/details/datasetVersionSelector";
import { RawFileSummaryCard } from "@/components/datasets/details/rawFileSummaryCard";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { DatasetExplorationGuideCard } from "@/components/datasets/exploration/datasetExplorationGuideCard";
import { DatasetExplorationWorkspace } from "@/components/datasets/exploration/datasetExplorationWorkspace";
import {
  DEFAULT_DATASET_EXPLORATION_ROW_PAGE_SIZE,
  DEFAULT_DATASET_EXPLORATION_SORT_DIRECTION,
} from "@/constants/datasetExploration";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import {
  DatasetExplorationProvider,
  useDatasetExplorationActions,
  useDatasetExplorationState,
} from "@/providers/datasetExplorationProvider";
import { DatasetExplorationAnalysisProvider } from "@/providers/datasetExplorationAnalysisProvider";
import { getDatasetProfileOverview } from "@/utils/datasets";
import { useStyles } from "./style";

const parseRouteNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const DatasetExplorationContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const params = useParams<{ datasetId: string }>();
  const searchParams = useSearchParams();
  const { getDatasetDetails, refreshDetails } = useDatasetDetailsActions();
  const { details, errorMessage, isError, isLoadingDetails } =
    useDatasetDetailsState();
  const {
    clearExplorationData,
    getExplorationColumns,
    getRows,
    refreshColumns,
  } = useDatasetExplorationActions();
  const {
    columns: explorationColumns,
    columnsErrorMessage,
    isColumnsError,
    isLoadingColumns,
  } = useDatasetExplorationState();

  const datasetId = parseRouteNumber(params.datasetId);
  const selectedVersionId = parseRouteNumber(searchParams.get("versionId"));
  const effectiveSelectedVersionId =
    selectedVersionId ?? details?.selectedVersion?.id;

  const loadDetails = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getDatasetDetails(datasetId, selectedVersionId);
  });

  useEffect(() => {
    loadDetails();
  }, [datasetId, selectedVersionId]);

  const loadExplorationData = useEffectEvent(() => {
    if (!effectiveSelectedVersionId) {
      clearExplorationData();
      return;
    }

    clearExplorationData();
    void Promise.all([
      getExplorationColumns(effectiveSelectedVersionId),
      getRows({
        datasetVersionId: effectiveSelectedVersionId,
        page: 1,
        pageSize: DEFAULT_DATASET_EXPLORATION_ROW_PAGE_SIZE,
        sortDirection: DEFAULT_DATASET_EXPLORATION_SORT_DIRECTION,
      }),
    ]);
  });

  useEffect(() => {
    loadExplorationData();
  }, [effectiveSelectedVersionId]);

  const handleVersionChange = (versionId?: number) => {
    if (!datasetId) {
      return;
    }

    const nextSearchParams = new URLSearchParams(searchParams.toString());

    if (versionId) {
      nextSearchParams.set("versionId", String(versionId));
    } else {
      nextSearchParams.delete("versionId");
    }

    const queryString = nextSearchParams.toString();
    router.push(
      queryString
        ? `/datasets/${datasetId}/explore?${queryString}`
        : `/datasets/${datasetId}/explore`,
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

  return (
    <>
      <WorkspacePageHeader
        title={details?.dataset.name || "Explore Dataset"}
        description="Inspect rows and run backend-driven exploration analyses against the selected dataset version."
        actions={
          <div className={styles.actionGroup}>
            <Button
              size="large"
              onClick={() =>
                router.push(
                  effectiveSelectedVersionId
                    ? `/datasets/${datasetId}?versionId=${effectiveSelectedVersionId}`
                    : `/datasets/${datasetId}`,
                )
              }
            >
              Back to dataset
            </Button>
          </div>
        }
      />

      {isError && !details ? (
        <DatasetErrorState
          title="Unable to load dataset exploration"
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
            {isLoadingColumns && !explorationColumns ? (
              <Card loading className={styles.loadingCard} />
            ) : isColumnsError && !explorationColumns ? (
              <DatasetErrorState
                title="Unable to load exploration metadata"
                message={
                  columnsErrorMessage ||
                  "Please try loading the exploration metadata again."
                }
                action={
                  <Button type="primary" onClick={() => void refreshColumns()}>
                    Retry exploration metadata
                  </Button>
                }
              />
            ) : effectiveSelectedVersionId ? (
              <DatasetExplorationWorkspace
                datasetVersionId={effectiveSelectedVersionId}
                explorationColumns={explorationColumns}
              />
            ) : (
              <Card loading className={styles.loadingCard} />
            )}
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
              title="Selected Version Profile"
              emptyDescription="Profiling data is not available for the selected version."
            />
            <RawFileSummaryCard rawFile={details.selectedVersion?.rawFile} />
            <DatasetExplorationGuideCard />
          </div>
        </div>
      ) : null}
    </>
  );
};

const DatasetExplorationPage = () => (
  <DatasetDetailsProvider>
    <DatasetExplorationProvider>
      <DatasetExplorationAnalysisProvider>
        <DatasetExplorationContent />
      </DatasetExplorationAnalysisProvider>
    </DatasetExplorationProvider>
  </DatasetDetailsProvider>
);

export default DatasetExplorationPage;
