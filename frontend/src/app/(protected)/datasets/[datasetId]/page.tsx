"use client";

import { useEffect, useEffectEvent } from "react";
import { Button, Card } from "antd";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { DatasetVersionSelector } from "@/components/datasets/details/datasetVersionSelector";
import { RawFileSummaryCard } from "@/components/datasets/details/rawFileSummaryCard";
import { DatasetColumnsTable } from "@/components/datasets/shared/datasetColumnsTable";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { DatasetSchemaPreview } from "@/components/datasets/shared/datasetSchemaPreview";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
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
  const { getDatasetDetails, refreshDetails } = useDatasetDetailsActions();
  const { details, isLoadingDetails, isError, errorMessage } =
    useDatasetDetailsState();

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
      queryString ? `/datasets/${datasetId}?${queryString}` : `/datasets/${datasetId}`,
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
            <Button
              type="primary"
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
            <DatasetSchemaPreview
              schemaJson={details.selectedVersion?.schemaJson}
            />
            <DatasetColumnsTable
              columns={details.columns}
              isLoading={isLoadingDetails}
              title="Version Columns"
            />
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
  <DatasetDetailsProvider>
    <DatasetDetailsContent />
  </DatasetDetailsProvider>
);

export default DatasetDetailsPage;
