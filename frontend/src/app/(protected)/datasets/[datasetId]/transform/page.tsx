"use client";

import { useEffect, useEffectEvent } from "react";
import { Button, Card, Typography } from "antd";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { DatasetOverviewCard } from "@/components/datasets/details/datasetOverviewCard";
import { RawFileSummaryCard } from "@/components/datasets/details/rawFileSummaryCard";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { DatasetSchemaPreview } from "@/components/datasets/shared/datasetSchemaPreview";
import { DatasetTransformationPipelineBuilder } from "@/components/datasets/transformations/datasetTransformationPipelineBuilder";
import { DatasetTransformationResultState } from "@/components/datasets/transformations/datasetTransformationResultState";
import { DatasetTransformationSourceVersionSelector } from "@/components/datasets/transformations/datasetTransformationSourceVersionSelector";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import {
  DatasetDetailsProvider,
  useDatasetDetailsActions,
  useDatasetDetailsState,
} from "@/providers/datasetDetailsProvider";
import {
  DatasetTransformationBuilderProvider,
  useDatasetTransformationBuilderActions,
  useDatasetTransformationBuilderState,
} from "@/providers/datasetTransformationBuilderProvider";
import { getDatasetProfileOverview } from "@/utils/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

const parseRouteNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const DatasetTransformationBuilderContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const params = useParams<{ datasetId: string }>();
  const searchParams = useSearchParams();
  const { getDatasetDetails, refreshDetails } = useDatasetDetailsActions();
  const { details, errorMessage, isError, isLoadingDetails } =
    useDatasetDetailsState();
  const {
    addStep,
    moveStep,
    removeStep,
    resetTransformation,
    setSourceVersionId,
    transformDatasetVersion,
    updateStep,
  } = useDatasetTransformationBuilderActions();
  const {
    errorMessage: transformationErrorMessage,
    isSubmittingTransformation,
    steps,
    transformResult,
  } = useDatasetTransformationBuilderState();

  const datasetId = parseRouteNumber(params.datasetId);
  const sourceVersionId = parseRouteNumber(searchParams.get("sourceVersionId"));
  const effectiveSourceVersionId =
    sourceVersionId ?? details?.selectedVersion?.id;

  const loadDetails = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getDatasetDetails(datasetId, sourceVersionId);
  });

  useEffect(() => {
    loadDetails();
  }, [datasetId, sourceVersionId]);

  const syncSourceVersion = useEffectEvent(() => {
    setSourceVersionId(effectiveSourceVersionId);
  });

  useEffect(() => {
    syncSourceVersion();
  }, [effectiveSourceVersionId]);

  useEffect(() => {
    if (!transformResult) {
      return undefined;
    }

    const redirectTimeout = window.setTimeout(() => {
      router.push(`/datasets/versions/${transformResult.finalDatasetVersionId}`);
    }, 1200);

    return () => window.clearTimeout(redirectTimeout);
  }, [router, transformResult]);

  const handleSourceVersionChange = (nextSourceVersionId?: number) => {
    if (!datasetId) {
      return;
    }

    const nextSearchParams = new URLSearchParams(searchParams.toString());

    if (nextSourceVersionId) {
      nextSearchParams.set("sourceVersionId", String(nextSourceVersionId));
    } else {
      nextSearchParams.delete("sourceVersionId");
    }

    const queryString = nextSearchParams.toString();
    router.push(
      queryString
        ? `/datasets/${datasetId}/transform?${queryString}`
        : `/datasets/${datasetId}/transform`,
    );
  };

  const handleSubmit = async () => {
    await transformDatasetVersion();
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
        title={details?.dataset.name || "Transform Dataset"}
        description="Build an ordered transformation pipeline on top of the selected dataset version and let the backend create the processed result."
        actions={
          <div className={styles.actionGroup}>
            <Button
              size="large"
              onClick={() =>
                router.push(
                  effectiveSourceVersionId
                    ? `/datasets/${datasetId}?versionId=${effectiveSourceVersionId}`
                    : `/datasets/${datasetId}`,
                )
              }
            >
              Back to dataset
            </Button>
            <Button size="large" onClick={() => resetTransformation()}>
              Reset pipeline
            </Button>
          </div>
        }
      />

      {isError && !details ? (
        <DatasetErrorState
          title="Unable to load transformation builder"
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
            {transformResult ? (
              <DatasetTransformationResultState transformResult={transformResult} />
            ) : (
              <DatasetTransformationPipelineBuilder
                columns={details.columns}
                steps={steps}
                isSubmitting={isSubmittingTransformation}
                errorMessage={transformationErrorMessage}
                onAddStep={addStep}
                onUpdateStep={updateStep}
                onRemoveStep={removeStep}
                onMoveStep={moveStep}
                onSubmit={handleSubmit}
              />
            )}
          </div>

          <div className={styles.sideColumn}>
            <DatasetTransformationSourceVersionSelector
              versions={details.versions}
              selectedVersionId={effectiveSourceVersionId}
              onChange={handleSourceVersionChange}
            />
            <DatasetOverviewCard
              dataset={details.dataset}
              selectedVersion={details.selectedVersion}
            />
            <DatasetProfileSummaryCard
              profile={profileOverview}
              isLoading={isLoadingDetails}
              title="Source Version Profile"
              emptyDescription="Profiling data is not available for the selected source version."
            />
            <DatasetSchemaPreview schemaJson={details.selectedVersion?.schemaJson} />
            <RawFileSummaryCard rawFile={details.selectedVersion?.rawFile} />
            <Card className={styles.helperCard}>
              <Title level={4} className={styles.helperTitle}>
                Transformation Notes
              </Title>
              <Paragraph className={styles.helperText}>
                Every step runs on the backend using the ordered pipeline you build here.
              </Paragraph>
              <Paragraph className={styles.helperText}>
                The backend creates processed versions as it moves through the pipeline and profiles the final result automatically.
              </Paragraph>
              <Paragraph className={styles.helperText}>
                Validation messages come directly from the backend, including unsupported conversions and malformed step configuration.
              </Paragraph>
            </Card>
          </div>
        </div>
      ) : null}
    </>
  );
};

const DatasetTransformationBuilderPage = () => (
  <DatasetDetailsProvider>
    <DatasetTransformationBuilderProvider>
      <DatasetTransformationBuilderContent />
    </DatasetTransformationBuilderProvider>
  </DatasetDetailsProvider>
);

export default DatasetTransformationBuilderPage;
