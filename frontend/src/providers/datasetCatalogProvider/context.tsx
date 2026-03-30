import { createContext } from "react";
import type { DatasetCatalogFilters, DatasetListItem } from "@/types/datasets";
import { DEFAULT_DATASET_PAGE_SIZE } from "@/constants/datasets";

export interface IDatasetCatalogStateContext {
  isLoadingCatalog: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  filters: DatasetCatalogFilters;
  datasets: DatasetListItem[];
  totalCount: number;
}

export interface IDatasetCatalogActionContext {
  setFilters: (filters: DatasetCatalogFilters) => void;
  getDatasets: (filters: DatasetCatalogFilters) => Promise<void>;
  refreshDatasets: () => Promise<void>;
}

export const INITIAL_FILTERS: DatasetCatalogFilters = {
  keyword: "",
  page: 1,
  pageSize: DEFAULT_DATASET_PAGE_SIZE,
};

export const INITIAL_STATE: IDatasetCatalogStateContext = {
  isLoadingCatalog: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  filters: INITIAL_FILTERS,
  datasets: [],
  totalCount: 0,
};

export const DatasetCatalogStateContext =
  createContext<IDatasetCatalogStateContext | undefined>(undefined);

export const DatasetCatalogActionContext =
  createContext<IDatasetCatalogActionContext | undefined>(undefined);
