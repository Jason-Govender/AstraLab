"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  cancelMlExperiment,
  createMlExperiment,
  getMlExperimentsByDatasetVersion,
  retryMlExperiment,
} from "@/services/mlExperimentService";
import type { CreateMlExperimentRequest, MlExperiment } from "@/types/ml";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearExperimentsAction,
  loadExperimentsError,
  loadExperimentsPending,
  loadExperimentsSuccess,
  mutationError,
  mutationPending,
  mutationSuccess,
  selectExperimentAction,
  submitExperimentPending,
  submitExperimentSuccess,
} from "./actions";
import {
  INITIAL_STATE,
  MlExperimentsActionContext,
  MlExperimentsStateContext,
} from "./context";
import { MlExperimentsReducer } from "./reducer";

const selectExperimentId = (
  experiments: MlExperiment[],
  previousSelectionId?: number,
  preferredSelectionId?: number,
): number | undefined => {
  const candidateId = preferredSelectionId || previousSelectionId;
  if (candidateId && experiments.some((experiment) => experiment.id === candidateId)) {
    return candidateId;
  }

  return experiments[0]?.id;
};

export const MlExperimentsProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(MlExperimentsReducer, INITIAL_STATE);

  const loadExperiments = async (
    datasetVersionId: number,
    preferredSelectionId?: number,
  ) => {
    dispatch(loadExperimentsPending(datasetVersionId));

    try {
      const experiments = await getMlExperimentsByDatasetVersion(datasetVersionId);
      dispatch(
        loadExperimentsSuccess({
          currentDatasetVersionId: datasetVersionId,
          experiments,
          selectedExperimentId: selectExperimentId(
            experiments,
            state.selectedExperimentId,
            preferredSelectionId,
          ),
        }),
      );
    } catch (error) {
      dispatch(
        loadExperimentsError(
          getApiErrorMessage(
            error,
            "Unable to load ML experiments right now.",
          ),
        ),
      );
    }
  };

  const clearExperiments = () => {
    dispatch(clearExperimentsAction());
  };

  const selectExperiment = (experimentId?: number) => {
    dispatch(selectExperimentAction(experimentId));
  };

  const createExperimentAction = async (request: CreateMlExperimentRequest) => {
    dispatch(submitExperimentPending());

    try {
      const experiment = await createMlExperiment(request);
      dispatch(
        submitExperimentSuccess({
          selectedExperimentId: experiment.id,
        }),
      );
      await loadExperiments(request.datasetVersionId, experiment.id);
    } catch (error) {
      dispatch(
        mutationError(
          getApiErrorMessage(
            error,
            "Unable to create the ML experiment right now.",
          ),
        ),
      );
    }
  };

  const cancelExperiment = async (experimentId: number) => {
    if (!state.currentDatasetVersionId) {
      return;
    }

    dispatch(mutationPending());

    try {
      const experiment = await cancelMlExperiment(experimentId);
      dispatch(mutationSuccess({ selectedExperimentId: experiment.id }));
      await loadExperiments(state.currentDatasetVersionId, experiment.id);
    } catch (error) {
      dispatch(
        mutationError(
          getApiErrorMessage(
            error,
            "Unable to cancel the ML experiment right now.",
          ),
        ),
      );
    }
  };

  const retryExperiment = async (experimentId: number) => {
    if (!state.currentDatasetVersionId) {
      return;
    }

    dispatch(mutationPending());

    try {
      const experiment = await retryMlExperiment(experimentId);
      dispatch(mutationSuccess({ selectedExperimentId: experiment.id }));
      await loadExperiments(state.currentDatasetVersionId, experiment.id);
    } catch (error) {
      dispatch(
        mutationError(
          getApiErrorMessage(
            error,
            "Unable to retry the ML experiment right now.",
          ),
        ),
      );
    }
  };

  const refreshExperiments = async () => {
    if (!state.currentDatasetVersionId) {
      return;
    }

    await loadExperiments(state.currentDatasetVersionId, state.selectedExperimentId);
  };

  return (
    <MlExperimentsStateContext.Provider value={state}>
      <MlExperimentsActionContext.Provider
        value={{
          loadExperiments,
          clearExperiments,
          selectExperiment,
          createExperiment: createExperimentAction,
          cancelExperiment,
          retryExperiment,
          refreshExperiments,
        }}
      >
        {children}
      </MlExperimentsActionContext.Provider>
    </MlExperimentsStateContext.Provider>
  );
};

export const useMlExperimentsState = () => {
  const context = useContext(MlExperimentsStateContext);

  if (!context) {
    throw new Error(
      "useMlExperimentsState must be used within a MlExperimentsProvider",
    );
  }

  return context;
};

export const useMlExperimentsActions = () => {
  const context = useContext(MlExperimentsActionContext);

  if (!context) {
    throw new Error(
      "useMlExperimentsActions must be used within a MlExperimentsProvider",
    );
  }

  return context;
};
