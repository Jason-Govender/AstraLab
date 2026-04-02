import type { AIResponse } from "@/types/datasets";
import type { IDatasetAutoInsightStateContext } from "./context";

type DatasetAutoInsightStatePatch = Partial<IDatasetAutoInsightStateContext>;

export enum DatasetAutoInsightActionEnums {
  getInsightPending = "DATASET_AUTO_INSIGHT_PENDING",
  getInsightSuccess = "DATASET_AUTO_INSIGHT_SUCCESS",
  getInsightError = "DATASET_AUTO_INSIGHT_ERROR",
  clearInsight = "DATASET_AUTO_INSIGHT_CLEAR",
}

interface GetInsightPendingAction {
  type: DatasetAutoInsightActionEnums.getInsightPending;
  payload: DatasetAutoInsightStatePatch;
}

interface GetInsightSuccessAction {
  type: DatasetAutoInsightActionEnums.getInsightSuccess;
  payload: DatasetAutoInsightStatePatch;
}

interface GetInsightErrorAction {
  type: DatasetAutoInsightActionEnums.getInsightError;
  payload: DatasetAutoInsightStatePatch;
}

interface ClearInsightAction {
  type: DatasetAutoInsightActionEnums.clearInsight;
  payload: DatasetAutoInsightStatePatch;
}

export type DatasetAutoInsightAction =
  | GetInsightPendingAction
  | GetInsightSuccessAction
  | GetInsightErrorAction
  | ClearInsightAction;

export const getInsightPending = (
  datasetVersionId: number,
): GetInsightPendingAction => ({
  type: DatasetAutoInsightActionEnums.getInsightPending,
  payload: {
    isLoading: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    currentDatasetVersionId: datasetVersionId,
  },
});

export const getInsightSuccess = ({
  datasetVersionId,
  latestInsight,
}: {
  datasetVersionId: number;
  latestInsight: AIResponse | null;
}): GetInsightSuccessAction => ({
  type: DatasetAutoInsightActionEnums.getInsightSuccess,
  payload: {
    isLoading: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    currentDatasetVersionId: datasetVersionId,
    latestInsight,
  },
});

export const getInsightError = ({
  datasetVersionId,
  errorMessage,
}: {
  datasetVersionId: number;
  errorMessage: string;
}): GetInsightErrorAction => ({
  type: DatasetAutoInsightActionEnums.getInsightError,
  payload: {
    isLoading: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
    currentDatasetVersionId: datasetVersionId,
    latestInsight: null,
  },
});

export const clearInsight = (): ClearInsightAction => ({
  type: DatasetAutoInsightActionEnums.clearInsight,
  payload: {
    isLoading: false,
    isPending: false,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    currentDatasetVersionId: undefined,
    latestInsight: null,
  },
});
