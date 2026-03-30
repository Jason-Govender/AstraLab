import type { DatasetCatalogFilters, DatasetListItem } from "@/types/datasets";
import type { IDatasetCatalogStateContext } from "./context";

type DatasetCatalogStatePatch = Partial<IDatasetCatalogStateContext>;

export enum DatasetCatalogActionEnums {
  setFilters = "DATASET_CATALOG_SET_FILTERS",
  getDatasetsPending = "DATASET_CATALOG_PENDING",
  getDatasetsSuccess = "DATASET_CATALOG_SUCCESS",
  getDatasetsError = "DATASET_CATALOG_ERROR",
}

interface SetFiltersAction {
  type: DatasetCatalogActionEnums.setFilters;
  payload: DatasetCatalogStatePatch;
}

interface GetDatasetsPendingAction {
  type: DatasetCatalogActionEnums.getDatasetsPending;
  payload: DatasetCatalogStatePatch;
}

interface GetDatasetsSuccessAction {
  type: DatasetCatalogActionEnums.getDatasetsSuccess;
  payload: DatasetCatalogStatePatch;
}

interface GetDatasetsErrorAction {
  type: DatasetCatalogActionEnums.getDatasetsError;
  payload: DatasetCatalogStatePatch;
}

export type DatasetCatalogAction =
  | SetFiltersAction
  | GetDatasetsPendingAction
  | GetDatasetsSuccessAction
  | GetDatasetsErrorAction;

export const setFilters = (
  filters: DatasetCatalogFilters,
): SetFiltersAction => ({
  type: DatasetCatalogActionEnums.setFilters,
  payload: {
    filters,
  },
});

export const getDatasetsPending = (
  filters: DatasetCatalogFilters,
): GetDatasetsPendingAction => ({
  type: DatasetCatalogActionEnums.getDatasetsPending,
  payload: {
    isLoadingCatalog: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    filters,
  },
});

export const getDatasetsSuccess = ({
  filters,
  datasets,
  totalCount,
}: {
  filters: DatasetCatalogFilters;
  datasets: DatasetListItem[];
  totalCount: number;
}): GetDatasetsSuccessAction => ({
  type: DatasetCatalogActionEnums.getDatasetsSuccess,
  payload: {
    isLoadingCatalog: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    filters,
    datasets,
    totalCount,
  },
});

export const getDatasetsError = ({
  filters,
  errorMessage,
}: {
  filters: DatasetCatalogFilters;
  errorMessage: string;
}): GetDatasetsErrorAction => ({
  type: DatasetCatalogActionEnums.getDatasetsError,
  payload: {
    isLoadingCatalog: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
    filters,
  },
});
