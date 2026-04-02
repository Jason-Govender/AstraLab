"use client";

import { useEffect, useEffectEvent } from "react";
import { Button, Space } from "antd";
import {
  useRouter,
  useSearchParams,
  type ReadonlyURLSearchParams,
} from "next/navigation";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { WorkspaceLoadingCard } from "@/components/workspaceShell/WorkspaceLoadingCard";
import { DatasetCatalogFilters } from "@/components/datasets/catalog/datasetCatalogFilters";
import { DatasetCatalogTable } from "@/components/datasets/catalog/datasetCatalogTable";
import { DatasetEmptyState } from "@/components/datasets/shared/datasetEmptyState";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { DEFAULT_DATASET_PAGE_SIZE } from "@/constants/datasets";
import {
  DatasetCatalogProvider,
  useDatasetCatalogActions,
  useDatasetCatalogState,
} from "@/providers/datasetCatalogProvider";
import type { DatasetCatalogFilters as DatasetCatalogFiltersShape, DatasetStatus } from "@/types/datasets";
import { useStyles } from "./style";

const parsePositiveInteger = (
  value: string | null,
  fallbackValue: number,
): number => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return fallbackValue;
  }

  return parsedValue;
};

const parseDatasetStatus = (
  value: string | null,
): DatasetStatus | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue as DatasetStatus;
};

const buildFiltersFromSearchParams = (
  searchParams: ReadonlyURLSearchParams,
): DatasetCatalogFiltersShape => ({
  keyword: searchParams.get("keyword")?.trim() || "",
  status: parseDatasetStatus(searchParams.get("status")),
  page: parsePositiveInteger(searchParams.get("page"), 1),
  pageSize: parsePositiveInteger(
    searchParams.get("pageSize"),
    DEFAULT_DATASET_PAGE_SIZE,
  ),
});

const DatasetCatalogContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { getDatasets, refreshDatasets } = useDatasetCatalogActions();
  const {
    datasets,
    totalCount,
    isLoadingCatalog,
    isError,
    errorMessage,
  } = useDatasetCatalogState();

  const filters = buildFiltersFromSearchParams(searchParams);

  const loadDatasets = useEffectEvent(() => {
    void getDatasets(filters);
  });

  useEffect(() => {
    loadDatasets();
  }, [filters.keyword, filters.status, filters.page, filters.pageSize]);

  const updateRouteFilters = (nextFilters: DatasetCatalogFiltersShape) => {
    const nextSearchParams = new URLSearchParams();

    if (nextFilters.keyword) {
      nextSearchParams.set("keyword", nextFilters.keyword);
    }

    if (nextFilters.status) {
      nextSearchParams.set("status", String(nextFilters.status));
    }

    if (nextFilters.page > 1) {
      nextSearchParams.set("page", String(nextFilters.page));
    }

    if (nextFilters.pageSize !== DEFAULT_DATASET_PAGE_SIZE) {
      nextSearchParams.set("pageSize", String(nextFilters.pageSize));
    }

    const queryString = nextSearchParams.toString();
    router.push(queryString ? `/datasets?${queryString}` : "/datasets");
  };

  const handleOpenDataset = (datasetId: number, versionId?: number | null) => {
    if (versionId) {
      router.push(`/datasets/${datasetId}?versionId=${versionId}`);
      return;
    }

    router.push(`/datasets/${datasetId}`);
  };

  return (
    <>
      <WorkspacePageHeader
        title="Datasets"
        description="Browse ingested datasets, inspect their versions, and jump straight into schema review."
        actions={
          <Space className={styles.actionGroup}>
            <Button size="large" onClick={() => void refreshDatasets()}>
              Refresh
            </Button>
            <Button
              type="primary"
              size="large"
              onClick={() => router.push("/datasets/upload")}
            >
              Upload dataset
            </Button>
          </Space>
        }
      />

      <div className={styles.stackedContent}>
        <DatasetCatalogFilters
          filters={filters}
          isPending={isLoadingCatalog}
          onApplyFilters={updateRouteFilters}
        />

        {isError && datasets.length === 0 ? (
          <DatasetErrorState
            title="Unable to load datasets"
            message={errorMessage || "Please try again in a moment."}
            action={
              <Button type="primary" onClick={() => void refreshDatasets()}>
                Retry
              </Button>
            }
          />
        ) : null}

        {!isLoadingCatalog && !isError && datasets.length === 0 ? (
          <DatasetEmptyState
            title="No datasets yet"
            description="Upload your first CSV or JSON dataset to start building the workspace catalog."
            action={
              <Button
                type="primary"
                size="large"
                onClick={() => router.push("/datasets/upload")}
              >
                Upload dataset
              </Button>
            }
          />
        ) : null}

        {datasets.length > 0 || isLoadingCatalog ? (
          <DatasetCatalogTable
            datasets={datasets}
            totalCount={totalCount}
            currentPage={filters.page}
            pageSize={filters.pageSize}
            isLoading={isLoadingCatalog}
            onOpenDataset={handleOpenDataset}
            onChangePage={(page, pageSize) => {
              updateRouteFilters({
                ...filters,
                page,
                pageSize,
              });
            }}
          />
        ) : null}

        {isLoadingCatalog && datasets.length === 0 ? (
          <WorkspaceLoadingCard className={styles.loadingCard} />
        ) : null}
      </div>
    </>
  );
};

const DatasetsPage = () => (
  <DatasetCatalogProvider>
    <DatasetCatalogContent />
  </DatasetCatalogProvider>
);

export default DatasetsPage;
