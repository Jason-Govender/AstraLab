"use client";

import {
  useDeferredValue,
  useEffect,
  useEffectEvent,
  useMemo,
  useState,
} from "react";
import { Alert, Button, Card, Empty, Select, Typography } from "antd";
import { useRouter, useSearchParams } from "next/navigation";
import { MlExperimentWorkspace } from "@/components/datasets/ml/mlExperimentWorkspace";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
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
import { MlExperimentAutoInsightProvider } from "@/providers/mlExperimentAutoInsightProvider";
import { MlExperimentsProvider } from "@/providers/mlExperimentsProvider";
import { formatDateTime, getDatasetVersionTypeLabel } from "@/utils/datasets";
import { buildMlWorkspaceHref } from "@/utils/ml";
import { useStyles } from "./style";

const { Paragraph, Text } = Typography;

const DATASET_SELECTOR_PAGE_SIZE = 100;

const parseQueryNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const MlWorkspacePageContent = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { styles } = useStyles();
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
  const [datasetKeyword, setDatasetKeyword] = useState("");
  const deferredDatasetKeyword = useDeferredValue(datasetKeyword);

  const datasetId = parseQueryNumber(searchParams.get("datasetId"));
  const versionId = parseQueryNumber(searchParams.get("versionId"));
  const currentDetails =
    datasetId && details?.dataset.id === datasetId ? details : undefined;

  const loadDatasets = useEffectEvent(() => {
    void getDatasets({
      keyword: deferredDatasetKeyword.trim(),
      page: 1,
      pageSize: DATASET_SELECTOR_PAGE_SIZE,
    });
  });

  useEffect(() => {
    loadDatasets();
  }, [deferredDatasetKeyword]);

  const loadDatasetDetails = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void getDatasetDetails(datasetId, versionId);
  });

  useEffect(() => {
    if (!datasetId) {
      return;
    }

    loadDatasetDetails();
  }, [datasetId, versionId]);

  const selectedVersionIds = useMemo(
    () => currentDetails?.versions.map((version) => version.id) ?? [],
    [currentDetails?.versions],
  );

  useEffect(() => {
    if (!datasetId || !currentDetails) {
      return;
    }

    if (versionId && !selectedVersionIds.includes(versionId)) {
      const fallbackVersionId = currentDetails.selectedVersion?.id;

      router.replace(buildMlWorkspaceHref(datasetId, fallbackVersionId), {
        scroll: false,
      });
      return;
    }

    if (!versionId && currentDetails.selectedVersion?.id) {
      router.replace(
        buildMlWorkspaceHref(datasetId, currentDetails.selectedVersion.id),
        {
          scroll: false,
        },
      );
      return;
    }
  }, [currentDetails, datasetId, router, selectedVersionIds, versionId]);

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

  const effectiveSelectedVersionId = versionId ?? currentDetails?.selectedVersion?.id;
  const hasVersions = (currentDetails?.versions.length ?? 0) > 0;
  const currentVersionLabel = currentDetails?.selectedVersion
    ? `v${currentDetails.selectedVersion.versionNumber}`
    : undefined;

  const handleDatasetChange = (nextDatasetId?: number) => {
    router.push(buildMlWorkspaceHref(nextDatasetId));
  };

  const handleVersionChange = (nextVersionId?: number) => {
    if (!datasetId) {
      return;
    }

    router.push(buildMlWorkspaceHref(datasetId, nextVersionId));
  };

  return (
    <>
      <WorkspacePageHeader
        title="ML Workspace"
        description="Choose a dataset and version, then run machine learning experiments from one dedicated workspace."
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
            <div>
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

            <div>
              <Text strong>Version</Text>
              <Select
                allowClear
                size="large"
                value={effectiveSelectedVersionId}
                placeholder={
                  datasetId
                    ? "Select a dataset version"
                    : "Choose a dataset first"
                }
                options={versionOptions}
                disabled={!datasetId || !hasVersions || isLoadingDetails}
                onChange={handleVersionChange}
                className={styles.selectInput}
              />
            </div>
          </div>

          {currentDetails?.dataset ? (
            <Paragraph className={styles.selectionMeta}>
              Working in <strong>{currentDetails.dataset.name}</strong>
              {currentVersionLabel ? ` · ${currentVersionLabel}` : ""}
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
              description="Select a dataset to open its ML workspace and choose the version you want to model."
            />
          </Card>
        ) : null}

        {datasetId && isDatasetDetailsError && !currentDetails ? (
          <DatasetErrorState
            title="Unable to load ML workspace dataset"
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
          <Card loading className={styles.loadingCard} />
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
          <MlExperimentWorkspace
            datasetId={currentDetails.dataset.id}
            datasetVersionId={effectiveSelectedVersionId}
            columns={currentDetails.columns}
            hasRawFile={Boolean(currentDetails.selectedVersion?.rawFile)}
          />
        ) : null}
      </div>
    </>
  );
};

const MlWorkspacePage = () => (
  <DatasetCatalogProvider>
    <DatasetDetailsProvider>
      <MlExperimentsProvider>
        <MlExperimentAutoInsightProvider>
          <MlWorkspacePageContent />
        </MlExperimentAutoInsightProvider>
      </MlExperimentsProvider>
    </DatasetDetailsProvider>
  </DatasetCatalogProvider>
);

export default MlWorkspacePage;
