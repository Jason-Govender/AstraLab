import type { DatasetTransformationHistory } from "@/types/datasets";
import type { IDatasetTransformationHistoryStateContext } from "./context";

type DatasetTransformationHistoryStatePatch =
  Partial<IDatasetTransformationHistoryStateContext>;

export enum DatasetTransformationHistoryActionEnums {
  getHistoryPending = "DATASET_TRANSFORMATION_HISTORY_PENDING",
  getHistorySuccess = "DATASET_TRANSFORMATION_HISTORY_SUCCESS",
  getHistoryError = "DATASET_TRANSFORMATION_HISTORY_ERROR",
}

interface GetHistoryPendingAction {
  type: DatasetTransformationHistoryActionEnums.getHistoryPending;
  payload: DatasetTransformationHistoryStatePatch;
}

interface GetHistorySuccessAction {
  type: DatasetTransformationHistoryActionEnums.getHistorySuccess;
  payload: DatasetTransformationHistoryStatePatch;
}

interface GetHistoryErrorAction {
  type: DatasetTransformationHistoryActionEnums.getHistoryError;
  payload: DatasetTransformationHistoryStatePatch;
}

export type DatasetTransformationHistoryAction =
  | GetHistoryPendingAction
  | GetHistorySuccessAction
  | GetHistoryErrorAction;

export const getHistoryPending = (
  datasetId: number,
): GetHistoryPendingAction => ({
  type: DatasetTransformationHistoryActionEnums.getHistoryPending,
  payload: {
    isLoadingHistory: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    currentDatasetId: datasetId,
  },
});

export const getHistorySuccess = ({
  datasetId,
  history,
}: {
  datasetId: number;
  history: DatasetTransformationHistory;
}): GetHistorySuccessAction => ({
  type: DatasetTransformationHistoryActionEnums.getHistorySuccess,
  payload: {
    isLoadingHistory: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    currentDatasetId: datasetId,
    history,
  },
});

export const getHistoryError = ({
  datasetId,
  errorMessage,
}: {
  datasetId: number;
  errorMessage: string;
}): GetHistoryErrorAction => ({
  type: DatasetTransformationHistoryActionEnums.getHistoryError,
  payload: {
    isLoadingHistory: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
    currentDatasetId: datasetId,
  },
});
