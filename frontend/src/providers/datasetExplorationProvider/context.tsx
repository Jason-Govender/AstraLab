import { createContext } from "react";
import type {
  DatasetExplorationColumns,
  DatasetRow,
  PagedDatasetRowsRequest,
} from "@/types/datasets";

export interface IDatasetExplorationStateContext {
  isLoadingColumns: boolean;
  isLoadingRows: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  isColumnsError: boolean;
  isRowsError: boolean;
  errorMessage?: string;
  columnsErrorMessage?: string;
  rowsErrorMessage?: string;
  currentDatasetVersionId?: number;
  rowQuery?: PagedDatasetRowsRequest;
  columns?: DatasetExplorationColumns;
  rows: DatasetRow[];
  totalRowCount: number;
}

export interface IDatasetExplorationActionContext {
  getExplorationColumns: (datasetVersionId: number) => Promise<void>;
  getRows: (request: PagedDatasetRowsRequest) => Promise<void>;
  setRowQuery: (request: PagedDatasetRowsRequest) => void;
  clearExplorationData: () => void;
  refreshColumns: () => Promise<void>;
  refreshRows: () => Promise<void>;
}

export const INITIAL_STATE: IDatasetExplorationStateContext = {
  isLoadingColumns: false,
  isLoadingRows: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  isColumnsError: false,
  isRowsError: false,
  rows: [],
  totalRowCount: 0,
};

export const DatasetExplorationStateContext =
  createContext<IDatasetExplorationStateContext | undefined>(undefined);

export const DatasetExplorationActionContext =
  createContext<IDatasetExplorationActionContext | undefined>(undefined);
