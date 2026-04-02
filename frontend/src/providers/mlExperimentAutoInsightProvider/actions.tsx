import type { AIResponse } from "@/types/datasets";
import type { IMlExperimentAutoInsightStateContext } from "./context";

type MlExperimentAutoInsightStatePatch =
  Partial<IMlExperimentAutoInsightStateContext>;

export enum MlExperimentAutoInsightActionEnums {
  getInsightPending = "ML_EXPERIMENT_AUTO_INSIGHT_GET_PENDING",
  getInsightSuccess = "ML_EXPERIMENT_AUTO_INSIGHT_GET_SUCCESS",
  getInsightError = "ML_EXPERIMENT_AUTO_INSIGHT_GET_ERROR",
  clearInsight = "ML_EXPERIMENT_AUTO_INSIGHT_CLEAR",
}

interface GetInsightPendingAction {
  type: MlExperimentAutoInsightActionEnums.getInsightPending;
  payload: MlExperimentAutoInsightStatePatch;
}

interface GetInsightSuccessAction {
  type: MlExperimentAutoInsightActionEnums.getInsightSuccess;
  payload: MlExperimentAutoInsightStatePatch;
}

interface GetInsightErrorAction {
  type: MlExperimentAutoInsightActionEnums.getInsightError;
  payload: MlExperimentAutoInsightStatePatch;
}

interface ClearInsightAction {
  type: MlExperimentAutoInsightActionEnums.clearInsight;
  payload: MlExperimentAutoInsightStatePatch;
}

export type MlExperimentAutoInsightAction =
  | GetInsightPendingAction
  | GetInsightSuccessAction
  | GetInsightErrorAction
  | ClearInsightAction;

export const getInsightPending = ({
  experimentId,
}: {
  experimentId?: number;
}): GetInsightPendingAction => ({
  type: MlExperimentAutoInsightActionEnums.getInsightPending,
  payload: {
    currentExperimentId: experimentId,
    latestInsight: null,
    isLoadingInsight: Boolean(experimentId),
    isError: false,
    errorMessage: undefined,
  },
});

export const getInsightSuccess = ({
  experimentId,
  insight,
}: {
  experimentId?: number;
  insight?: AIResponse | null;
}): GetInsightSuccessAction => ({
  type: MlExperimentAutoInsightActionEnums.getInsightSuccess,
  payload: {
    currentExperimentId: experimentId,
    latestInsight: insight ?? null,
    isLoadingInsight: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const getInsightError = ({
  experimentId,
  errorMessage,
}: {
  experimentId?: number;
  errorMessage: string;
}): GetInsightErrorAction => ({
  type: MlExperimentAutoInsightActionEnums.getInsightError,
  payload: {
    currentExperimentId: experimentId,
    latestInsight: null,
    isLoadingInsight: false,
    isError: true,
    errorMessage,
  },
});

export const clearInsight = (): ClearInsightAction => ({
  type: MlExperimentAutoInsightActionEnums.clearInsight,
  payload: {
    currentExperimentId: undefined,
    latestInsight: null,
    isLoadingInsight: false,
    isError: false,
    errorMessage: undefined,
  },
});
