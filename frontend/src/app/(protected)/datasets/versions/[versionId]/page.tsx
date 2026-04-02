"use client";

import { useEffect, useEffectEvent, useState } from "react";
import { Button } from "antd";
import { useParams, useRouter } from "next/navigation";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { RawFileSummaryCard } from "@/components/datasets/details/rawFileSummaryCard";
import { DatasetColumnsTable } from "@/components/datasets/shared/datasetColumnsTable";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { DatasetProfileColumnsTable } from "@/components/datasets/shared/datasetProfileColumnsTable";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetSchemaPreview } from "@/components/datasets/shared/datasetSchemaPreview";
import { DatasetProducedByTransformationCard } from "@/components/datasets/transformations/datasetProducedByTransformationCard";
import { WorkspaceLoadingCard } from "@/components/workspaceShell/WorkspaceLoadingCard";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { DEFAULT_DATASET_PROFILE_COLUMNS_PAGE_SIZE } from "@/constants/datasets";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import {
  ProcessedDatasetVersionProvider,
  useProcessedDatasetVersionActions,
  useProcessedDatasetVersionState,
} from "@/providers/processedDatasetVersionProvider";
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

const ProcessedDatasetVersionContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const params = useParams<{ versionId: string }>();
  const { getProcessedDatasetVersion, refreshProcessedDatasetVersion } =
    useProcessedDatasetVersionActions();
  const {
    errorMessage: processedVersionErrorMessage,
    isError: isProcessedVersionError,
    isLoadingProcessedVersion,
    processedVersion,
  } = useProcessedDatasetVersionState();
  const {
    clearProfileColumns,
    getDatasetDetails,
    getProfileColumns,
    refreshDetails,
    refreshProfileColumns,
  } = useDatasetDetailsActions();
  const {
    details,
    errorMessage: detailsErrorMessage,
    isError: isDetailsError,
    isLoadingDetails,
    isLoadingProfileColumns,
    isProfileColumnsError,
    profileColumns,
    profileColumnsErrorMessage,
    profileColumnsTotalCount,
  } = useDatasetDetailsState();
  const [profileColumnsPage, setProfileColumnsPage] = useState(1);
  const [profileColumnsPageSize, setProfileColumnsPageSize] = useState(
    DEFAULT_DATASET_PROFILE_COLUMNS_PAGE_SIZE,
  );

  const versionId = parseRouteNumber(params.versionId);
  const datasetId = processedVersion?.version.datasetId;

  const loadProcessedVersion = useEffectEvent(() => {
    if (!versionId) {
      return;
    }

    void getProcessedDatasetVersion(versionId);
  });

  useEffect(() => {
    loadProcessedVersion();
  }, [versionId]);

  const loadDetails = useEffectEvent(() => {
    if (!datasetId || !versionId) {
      return;
    }

    void getDatasetDetails(datasetId, versionId);
  });

  useEffect(() => {
    loadDetails();
  }, [datasetId, versionId]);

  const selectedVersionProfileId =
    processedVersion?.version.profile?.id || details?.selectedVersion?.profile?.id;

  const loadProfileColumns = useEffectEvent(() => {
    if (!versionId || !selectedVersionProfileId) {
      clearProfileColumns();
      return;
    }

    void getProfileColumns({
      datasetVersionId: versionId,
      page: profileColumnsPage,
      pageSize: profileColumnsPageSize,
    });
  });

  useEffect(() => {
    loadProfileColumns();
  }, [versionId, selectedVersionProfileId, profileColumnsPage, profileColumnsPageSize]);

  const handleProfileColumnsPageChange = (page: number, pageSize: number) => {
    setProfileColumnsPage(page);
    setProfileColumnsPageSize(pageSize);
  };

  if (!versionId) {
    return (
      <DatasetErrorState
        title="Invalid processed version route"
        message="The dataset version identifier in the URL is not valid."
        action={
          <Button type="primary" onClick={() => router.push("/datasets")}>
            Back to datasets
          </Button>
        }
      />
    );
  }

  const profileOverview = getDatasetProfileOverview(
    processedVersion?.version.profile || details?.selectedVersion?.profile,
  );
  const fallbackProfileColumns =
    processedVersion?.version.profile && processedVersion.columns.length > 0
      ? buildDatasetColumnInsights(
          processedVersion.columns,
          processedVersion.version.profile.columnProfiles,
        )
      : [];
  const visibleProfileColumns =
    profileColumns.length > 0 ? profileColumns : fallbackProfileColumns;
  const totalVisibleProfileColumns =
    profileColumns.length > 0 ? profileColumnsTotalCount : fallbackProfileColumns.length;
  const shouldShowProfileColumns =
    Boolean(processedVersion?.version.profile) ||
    isLoadingProfileColumns ||
    visibleProfileColumns.length > 0 ||
    isProfileColumnsError;

  return (
    <>
      <WorkspacePageHeader
        title={details?.dataset.name || "Processed Dataset Version"}
        description="Inspect the processed dataset version, its profile summary, transformed columns, and the transformation that produced it."
        actions={
          <div className={styles.actionGroup}>
            <Button
              size="large"
              onClick={() =>
                datasetId
                  ? router.push(`/datasets/${datasetId}?versionId=${versionId}`)
                  : router.push("/datasets")
              }
            >
              Back to dataset
            </Button>
            {datasetId ? (
              <Button
                type="primary"
                size="large"
                onClick={() =>
                  router.push(
                    `/datasets/${datasetId}/transform?sourceVersionId=${versionId}`,
                  )
                }
              >
                Transform from this version
              </Button>
            ) : null}
          </div>
        }
      />

      {isProcessedVersionError && !processedVersion ? (
        <DatasetErrorState
          title="Unable to load the processed version"
          message={
            processedVersionErrorMessage ||
            "Please try loading the processed version again."
          }
          action={
            <Button
              type="primary"
              onClick={() => void refreshProcessedDatasetVersion()}
            >
              Retry
            </Button>
          }
        />
      ) : null}

      {!processedVersion && !isProcessedVersionError ? (
        <WorkspaceLoadingCard className={styles.loadingCard} />
      ) : null}

      {processedVersion ? (
        <div className={styles.pageGrid}>
          <div className={styles.mainColumn}>
            {details ? (
              <DatasetOverviewCard
                dataset={details.dataset}
                selectedVersion={details.selectedVersion}
              />
            ) : isDetailsError ? (
              <DatasetErrorState
                title="Unable to load dataset context"
                message={
                  detailsErrorMessage ||
                  "The processed version loaded, but the parent dataset details could not be loaded."
                }
                action={
                  <Button type="primary" onClick={() => void refreshDetails()}>
                    Retry dataset context
                  </Button>
                }
              />
            ) : (
              <WorkspaceLoadingCard className={styles.loadingCard} />
            )}

            <DatasetProfileSummaryCard
              profile={profileOverview}
              isLoading={isLoadingProcessedVersion || isLoadingDetails}
              title="Processed Version Profile"
              emptyDescription="Profiling data is not available for this processed version."
            />
            <DatasetSchemaPreview
              schemaJson={processedVersion.version.schemaJson}
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
                  title="Processed Column Insights"
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
                columns={processedVersion.columns}
                isLoading={isLoadingProcessedVersion}
                title="Processed Columns"
              />
            )}
          </div>

          <div className={styles.sideColumn}>
            <DatasetProducedByTransformationCard
              datasetId={datasetId || processedVersion.version.datasetId}
              transformation={processedVersion.producedByTransformation}
            />
            <RawFileSummaryCard rawFile={processedVersion.version.rawFile} />
          </div>
        </div>
      ) : null}
    </>
  );
};

const ProcessedDatasetVersionPage = () => (
  <ProcessedDatasetVersionProvider>
    <DatasetDetailsProvider>
      <ProcessedDatasetVersionContent />
    </DatasetDetailsProvider>
  </ProcessedDatasetVersionProvider>
);

export default ProcessedDatasetVersionPage;
