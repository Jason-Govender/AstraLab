import type {
  DatasetExplorationColumns,
  DatasetRow,
  PagedDatasetRowsRequest,
} from "@/types/datasets";
import type { IDatasetExplorationStateContext } from "./context";

type DatasetExplorationStatePatch = Partial<IDatasetExplorationStateContext>;

export enum DatasetExplorationActionEnums {
  getColumnsPending = "DATASET_EXPLORATION_GET_COLUMNS_PENDING",
  getColumnsSuccess = "DATASET_EXPLORATION_GET_COLUMNS_SUCCESS",
  getColumnsError = "DATASET_EXPLORATION_GET_COLUMNS_ERROR",
  getRowsPending = "DATASET_EXPLORATION_GET_ROWS_PENDING",
  getRowsSuccess = "DATASET_EXPLORATION_GET_ROWS_SUCCESS",
  getRowsError = "DATASET_EXPLORATION_GET_ROWS_ERROR",
  setRowQuery = "DATASET_EXPLORATION_SET_ROW_QUERY",
  clear = "DATASET_EXPLORATION_CLEAR",
}

interface DatasetExplorationActionShape {
  type: DatasetExplorationActionEnums;
  payload: DatasetExplorationStatePatch;
}

export type DatasetExplorationAction = DatasetExplorationActionShape;

export const getColumnsPending = (
  datasetVersionId: number,
): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getColumnsPending,
  payload: {
    isLoadingColumns: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    isColumnsError: false,
    errorMessage: undefined,
    columnsErrorMessage: undefined,
    currentDatasetVersionId: datasetVersionId,
  },
});

export const getColumnsSuccess = ({
  datasetVersionId,
  columns,
}: {
  datasetVersionId: number;
  columns: DatasetExplorationColumns;
}): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getColumnsSuccess,
  payload: {
    isLoadingColumns: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    isColumnsError: false,
    errorMessage: undefined,
    columnsErrorMessage: undefined,
    currentDatasetVersionId: datasetVersionId,
    columns,
  },
});

export const getColumnsError = ({
  datasetVersionId,
  errorMessage,
}: {
  datasetVersionId: number;
  errorMessage: string;
}): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getColumnsError,
  payload: {
    isLoadingColumns: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    isColumnsError: true,
    errorMessage,
    columnsErrorMessage: errorMessage,
    currentDatasetVersionId: datasetVersionId,
  },
});

export const getRowsPending = (
  request: PagedDatasetRowsRequest,
): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getRowsPending,
  payload: {
    isLoadingRows: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    isRowsError: false,
    errorMessage: undefined,
    rowsErrorMessage: undefined,
    currentDatasetVersionId: request.datasetVersionId,
    rowQuery: request,
  },
});

export const getRowsSuccess = ({
  request,
  rows,
  totalRowCount,
}: {
  request: PagedDatasetRowsRequest;
  rows: DatasetRow[];
  totalRowCount: number;
}): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getRowsSuccess,
  payload: {
    isLoadingRows: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    isRowsError: false,
    errorMessage: undefined,
    rowsErrorMessage: undefined,
    currentDatasetVersionId: request.datasetVersionId,
    rowQuery: request,
    rows,
    totalRowCount,
  },
});

export const getRowsError = ({
  request,
  errorMessage,
}: {
  request: PagedDatasetRowsRequest;
  errorMessage: string;
}): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.getRowsError,
  payload: {
    isLoadingRows: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    isRowsError: true,
    errorMessage,
    rowsErrorMessage: errorMessage,
    currentDatasetVersionId: request.datasetVersionId,
    rowQuery: request,
  },
});

export const setRowQuery = (
  rowQuery: PagedDatasetRowsRequest,
): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.setRowQuery,
  payload: {
    rowQuery,
  },
});

export const clearExplorationData = (): DatasetExplorationAction => ({
  type: DatasetExplorationActionEnums.clear,
  payload: {
    isLoadingColumns: false,
    isLoadingRows: false,
    isPending: false,
    isSuccess: false,
    isError: false,
    isColumnsError: false,
    isRowsError: false,
    errorMessage: undefined,
    columnsErrorMessage: undefined,
    rowsErrorMessage: undefined,
    currentDatasetVersionId: undefined,
    rowQuery: undefined,
    columns: undefined,
    rows: [],
    totalRowCount: 0,
  },
});
