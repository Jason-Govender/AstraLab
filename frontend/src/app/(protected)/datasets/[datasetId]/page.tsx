"use client";

import { useEffect, useEffectEvent, useState } from "react";
import { Button, Card } from "antd";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetProfileColumnsTable } from "@/components/datasets/shared/datasetProfileColumnsTable";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { DatasetAutomaticInsightCard } from "@/components/datasets/details/datasetAutomaticInsightCard";
import { DatasetVersionSelector } from "@/components/datasets/details/datasetVersionSelector";
import { RawFileSummaryCard } from "@/components/datasets/details/rawFileSummaryCard";
import { DatasetAiAssistantLauncherButton } from "@/components/datasets/ai/datasetAiAssistantLauncherButton";
import { DatasetColumnsTable } from "@/components/datasets/shared/datasetColumnsTable";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { DatasetSchemaPreview } from "@/components/datasets/shared/datasetSchemaPreview";
import { DatasetTransformationHistoryCard } from "@/components/datasets/transformations/datasetTransformationHistoryCard";
import { MlWorkspaceLauncherCard } from "@/components/datasets/ml/mlWorkspaceLauncherCard";
import { DatasetExplorationLauncherButton } from "@/components/datasets/exploration/datasetExplorationLauncherButton";
import { DEFAULT_DATASET_PROFILE_COLUMNS_PAGE_SIZE } from "@/constants/datasets";
import {
  DatasetAutoInsightProvider,
  useDatasetAutoInsightActions,
  useDatasetAutoInsightState,
} from "@/providers/datasetAutoInsightProvider";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import {
  DatasetTransformationHistoryProvider,
  useDatasetTransformationHistoryActions,
  useDatasetTransformationHistoryState,
} from "@/providers/datasetTransformationHistoryProvider";
import {
  buildDatasetColumnInsights,
  getDatasetProfileOverview,
} from "@/utils/datasets";
import { useStyles } from "./style";

const parseRouteNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const DatasetDetailsContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const params = useParams<{ datasetId: string }>();
  const searchParams = useSearchParams();
  const {
    clearProfileColumns,
    getDatasetDetails,
    getProfileColumns,
    refreshDetails,
    refreshProfileColumns,
  } = useDatasetDetailsActions();
  const {
    details,
    errorMessage,
    isError,
    isLoadingDetails,
    isLoadingProfileColumns,
    isProfileColumnsError,
    profileColumns,
    profileColumnsErrorMessage,
    profileColumnsTotalCount,
  } = useDatasetDetailsState();
  const {
    clearLatestAutomaticInsight,
    getLatestAutomaticInsight,
    refreshLatestAutomaticInsight,
  } = useDatasetAutoInsightActions();
  const {
    errorMessage: autoInsightErrorMessage,
    isError: isAutoInsightError,
    isLoading: isLoadingAutoInsight,
    latestInsight,
  } = useDatasetAutoInsightState();
  const {
    getTransformationHistory,
    refreshTransformationHistory,
  } = useDatasetTransformationHistoryActions();
  const {
    errorMessage: transformationHistoryErrorMessage,
    history,
    isError: isTransformationHistoryError,
    isLoadingHistory,
  } = useDatasetTransformationHistoryState();
  const [profileColumnsPage, setProfileColumnsPage] = useState(1);
  const [profileColumnsPageSize, setProfileColumnsPageSize] = useState(
    DEFAULT_DATASET_PROFILE_COLUMNS_PAGE_SIZE,
  );

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

  const loadTransformationHistory = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getTransformationHistory(datasetId);
  });

  useEffect(() => {
    loadTransformationHistory();
  }, [datasetId]);

  const selectedVersionProfileId = details?.selectedVersion?.profile?.id;
  const selectedVersionDetailsId = details?.selectedVersion?.id;

  const loadProfileColumns = useEffectEvent(() => {
    if (!selectedVersionDetailsId || !selectedVersionProfileId) {
      clearProfileColumns();
      return;
    }

    void getProfileColumns({
      datasetVersionId: selectedVersionDetailsId,
      page: profileColumnsPage,
      pageSize: profileColumnsPageSize,
    });
  });

  useEffect(() => {
    loadProfileColumns();
  }, [
    selectedVersionDetailsId,
    selectedVersionProfileId,
    profileColumnsPage,
    profileColumnsPageSize,
  ]);

  const loadAutomaticInsight = useEffectEvent(() => {
    if (!selectedVersionDetailsId) {
      clearLatestAutomaticInsight();
      return;
    }

    void getLatestAutomaticInsight(selectedVersionDetailsId);
  });

  useEffect(() => {
    loadAutomaticInsight();
  }, [selectedVersionDetailsId]);

  const handleVersionChange = (versionId?: number) => {
    if (!datasetId) {
      return;
    }

    clearProfileColumns();
    setProfileColumnsPage(1);

    const nextSearchParams = new URLSearchParams(searchParams.toString());

    if (versionId) {
      nextSearchParams.set("versionId", String(versionId));
    } else {
      nextSearchParams.delete("versionId");
    }

    const queryString = nextSearchParams.toString();
    router.push(
      queryString ? `/datasets/${datasetId}?${queryString}` : `/datasets/${datasetId}`,
    );
  };

  const handleProfileColumnsPageChange = (page: number, pageSize: number) => {
    setProfileColumnsPage(page);
    setProfileColumnsPageSize(pageSize);
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
  const fallbackProfileColumns =
    details?.selectedVersion?.profile && details.columns.length > 0
      ? buildDatasetColumnInsights(
          details.columns,
          details.selectedVersion.profile.columnProfiles,
        )
      : [];
  const visibleProfileColumns =
    profileColumns.length > 0 ? profileColumns : fallbackProfileColumns;
  const totalVisibleProfileColumns =
    profileColumns.length > 0 ? profileColumnsTotalCount : fallbackProfileColumns.length;
  const shouldShowProfileColumns =
    Boolean(details?.selectedVersion?.profile) ||
    isLoadingProfileColumns ||
    visibleProfileColumns.length > 0 ||
    isProfileColumnsError;

  return (
    <>
      <WorkspacePageHeader
        title={details?.dataset.name || "Dataset Details"}
        description="Inspect ingestion metadata, schema output, stored raw file information, and version-specific columns."
        actions={
          <div className={styles.actionGroup}>
            <Button size="large" onClick={() => router.push("/datasets")}>
              Back to datasets
            </Button>
            <DatasetExplorationLauncherButton
              datasetId={datasetId}
              versionId={effectiveSelectedVersionId}
              size="large"
            />
            <DatasetAiAssistantLauncherButton
              datasetId={datasetId}
              versionId={effectiveSelectedVersionId}
              size="large"
            />
            <Button
              type="primary"
              size="large"
              onClick={() =>
                router.push(
                  effectiveSelectedVersionId
                    ? `/datasets/${datasetId}/transform?sourceVersionId=${effectiveSelectedVersionId}`
                    : `/datasets/${datasetId}/transform`,
                )
              }
            >
              Transform version
            </Button>
            <Button
              size="large"
              onClick={() => router.push("/datasets/upload")}
            >
              Upload dataset
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
            <DatasetOverviewCard
              dataset={details.dataset}
              selectedVersion={details.selectedVersion}
            />
            <DatasetProfileSummaryCard
              profile={profileOverview}
              isLoading={isLoadingDetails}
              title="Profiling Summary"
              emptyDescription="Profiling insights will appear here when they are available for the selected version."
            />
            <DatasetAutomaticInsightCard
              insight={latestInsight}
              isLoading={isLoadingAutoInsight}
              isError={isAutoInsightError}
              errorMessage={autoInsightErrorMessage}
              onRetry={() => void refreshLatestAutomaticInsight()}
            />
            <DatasetSchemaPreview
              schemaJson={details.selectedVersion?.schemaJson}
            />
            {shouldShowProfileColumns ? (
              isProfileColumnsError && visibleProfileColumns.length === 0 ? (
                <DatasetErrorState
                  title="Unable to load profiling insights"
                  message={
                    profileColumnsErrorMessage ||
                    "Please try loading the profiling insights again."
                  }
                  action={
                    <Button type="primary" onClick={() => void refreshProfileColumns()}>
                      Retry profiling insights
                    </Button>
                  }
                />
              ) : (
                <DatasetProfileColumnsTable
                  columns={visibleProfileColumns}
                  isLoading={isLoadingProfileColumns}
                  title="Profiled Column Insights"
                  page={profileColumnsPage}
                  pageSize={profileColumnsPageSize}
                  totalCount={totalVisibleProfileColumns}
                  onPageChange={
                    profileColumns.length > 0
                      ? handleProfileColumnsPageChange
                      : undefined
                  }
                />
              )
            ) : (
              <DatasetColumnsTable
                columns={details.columns}
                isLoading={isLoadingDetails}
                title="Version Columns"
              />
            )}

            <MlWorkspaceLauncherCard
              datasetId={datasetId}
              datasetVersionId={effectiveSelectedVersionId}
            />

            {isTransformationHistoryError && !history ? (
              <DatasetErrorState
                title="Unable to load transformation history"
                message={
                  transformationHistoryErrorMessage ||
                  "Please try loading the transformation history again."
                }
                action={
                  <Button
                    type="primary"
                    onClick={() => void refreshTransformationHistory()}
                  >
                    Retry history
                  </Button>
                }
              />
            ) : (
              <DatasetTransformationHistoryCard
                datasetId={datasetId}
                history={history}
                isLoading={isLoadingHistory}
              />
            )}
          </div>

          <div className={styles.sideColumn}>
            <DatasetVersionSelector
              versions={details.versions}
              selectedVersionId={effectiveSelectedVersionId}
              onChange={handleVersionChange}
            />
            <RawFileSummaryCard rawFile={details.selectedVersion?.rawFile} />
          </div>
        </div>
      ) : null}
    </>
  );
};

const DatasetDetailsPage = () => (
  <DatasetTransformationHistoryProvider>
    <DatasetAutoInsightProvider>
      <DatasetDetailsProvider>
        <DatasetDetailsContent />
      </DatasetDetailsProvider>
    </DatasetAutoInsightProvider>
  </DatasetTransformationHistoryProvider>
);

export default DatasetDetailsPage;
