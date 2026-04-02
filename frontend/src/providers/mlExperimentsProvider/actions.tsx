import type { IMlExperimentsStateContext } from "./context";

type MlExperimentsStatePatch = Partial<IMlExperimentsStateContext>;

export enum MlExperimentsActionEnums {
  loadPending = "ML_EXPERIMENTS_LOAD_PENDING",
  loadSuccess = "ML_EXPERIMENTS_LOAD_SUCCESS",
  loadError = "ML_EXPERIMENTS_LOAD_ERROR",
  submitPending = "ML_EXPERIMENTS_SUBMIT_PENDING",
  submitSuccess = "ML_EXPERIMENTS_SUBMIT_SUCCESS",
  mutationPending = "ML_EXPERIMENTS_MUTATION_PENDING",
  mutationSuccess = "ML_EXPERIMENTS_MUTATION_SUCCESS",
  mutationError = "ML_EXPERIMENTS_MUTATION_ERROR",
  selectExperiment = "ML_EXPERIMENTS_SELECT",
  clearFeedback = "ML_EXPERIMENTS_CLEAR_FEEDBACK",
  clearExperiments = "ML_EXPERIMENTS_CLEAR",
}

interface MlExperimentsAction {
  type: MlExperimentsActionEnums;
  payload: MlExperimentsStatePatch;
}

export const loadExperimentsPending = (
  datasetVersionId: number,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.loadPending,
  payload: {
    isLoadingExperiments: true,
    isError: false,
    errorMessage: undefined,
    currentDatasetVersionId: datasetVersionId,
  },
});

export const loadExperimentsSuccess = (
  payload: MlExperimentsStatePatch,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.loadSuccess,
  payload: {
    ...payload,
    isLoadingExperiments: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const loadExperimentsError = (
  errorMessage: string,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.loadError,
  payload: {
    isLoadingExperiments: false,
    isError: true,
    errorMessage,
  },
});

export const submitExperimentPending = (): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.submitPending,
  payload: {
    isSubmittingExperiment: true,
    isError: false,
    errorMessage: undefined,
    lastMutationMessage: undefined,
  },
});

export const submitExperimentSuccess = (
  payload: MlExperimentsStatePatch,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.submitSuccess,
  payload: {
    ...payload,
    isSubmittingExperiment: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const mutationPending = (): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.mutationPending,
  payload: {
    isMutatingExperiment: true,
    isError: false,
    errorMessage: undefined,
    lastMutationMessage: undefined,
  },
});

export const mutationSuccess = (
  payload: MlExperimentsStatePatch,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.mutationSuccess,
  payload: {
    ...payload,
    isMutatingExperiment: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const mutationError = (
  errorMessage: string,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.mutationError,
  payload: {
    isSubmittingExperiment: false,
    isMutatingExperiment: false,
    isError: true,
    errorMessage,
  },
});

export const selectExperimentAction = (
  selectedExperimentId?: number,
): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.selectExperiment,
  payload: {
    selectedExperimentId,
  },
});

export const clearFeedbackAction = (): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.clearFeedback,
  payload: {
    isError: false,
    errorMessage: undefined,
    lastMutationMessage: undefined,
  },
});

export const clearExperimentsAction = (): MlExperimentsAction => ({
  type: MlExperimentsActionEnums.clearExperiments,
  payload: {
    isLoadingExperiments: false,
    isSubmittingExperiment: false,
    isMutatingExperiment: false,
    isError: false,
    errorMessage: undefined,
    lastMutationMessage: undefined,
    currentDatasetVersionId: undefined,
    selectedExperimentId: undefined,
    experiments: [],
  },
});

export type { MlExperimentsAction };
