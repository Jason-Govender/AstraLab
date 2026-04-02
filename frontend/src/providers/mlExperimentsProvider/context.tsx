import { createContext } from "react";
import type { CreateMlExperimentRequest, MlExperiment } from "@/types/ml";

export interface IMlExperimentsStateContext {
  isLoadingExperiments: boolean;
  isSubmittingExperiment: boolean;
  isMutatingExperiment: boolean;
  isError: boolean;
  errorMessage?: string;
  lastMutationMessage?: string;
  currentDatasetVersionId?: number;
  selectedExperimentId?: number;
  experiments: MlExperiment[];
}

export interface IMlExperimentsActionContext {
  loadExperiments: (
    datasetVersionId: number,
    preferredSelectionId?: number,
  ) => Promise<void>;
  clearExperiments: () => void;
  selectExperiment: (experimentId?: number) => void;
  createExperiment: (request: CreateMlExperimentRequest) => Promise<void>;
  cancelExperiment: (experimentId: number) => Promise<void>;
  retryExperiment: (experimentId: number) => Promise<void>;
  refreshExperiments: () => Promise<void>;
  clearFeedback: () => void;
}

export const INITIAL_STATE: IMlExperimentsStateContext = {
  isLoadingExperiments: false,
  isSubmittingExperiment: false,
  isMutatingExperiment: false,
  isError: false,
  experiments: [],
};

export const MlExperimentsStateContext =
  createContext<IMlExperimentsStateContext | undefined>(undefined);

export const MlExperimentsActionContext =
  createContext<IMlExperimentsActionContext | undefined>(undefined);
