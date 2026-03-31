"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { transformDatasetVersion as transformDatasetVersionRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  createDatasetTransformationBuilderStep,
  serializeDatasetTransformationSteps,
} from "@/utils/datasetTransformations";
import {
  addStep as addStepAction,
  moveStep as moveStepAction,
  resetTransformation as resetTransformationAction,
  setSourceVersionId as setSourceVersionIdAction,
  transformError,
  transformPending,
  transformSuccess,
  updateStep as updateStepAction,
  removeStep as removeStepAction,
} from "./actions";
import {
  DatasetTransformationBuilderActionContext,
  DatasetTransformationBuilderStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetTransformationBuilderReducer } from "./reducer";
import type {
  DatasetTransformationBuilderStep,
  DatasetTransformationType,
} from "@/types/datasets";

export const DatasetTransformationBuilderProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(
    DatasetTransformationBuilderReducer,
    INITIAL_STATE,
  );

  const setSourceVersionId = (sourceVersionId?: number) => {
    dispatch(setSourceVersionIdAction(sourceVersionId));
  };

  const addStep = (transformationType: DatasetTransformationType) => {
    dispatch(addStepAction(createDatasetTransformationBuilderStep(transformationType)));
  };

  const updateStep = (step: DatasetTransformationBuilderStep) => {
    dispatch(updateStepAction(step));
  };

  const removeStep = (stepId: string) => {
    dispatch(removeStepAction(stepId));
  };

  const moveStep = (stepId: string, direction: "up" | "down") => {
    dispatch(moveStepAction(stepId, direction));
  };

  const transformDatasetVersion = async () => {
    if (!state.selectedSourceVersionId) {
      dispatch(transformError("Select a source dataset version before transforming."));
      return undefined;
    }

    if (state.steps.length === 0) {
      dispatch(transformError("Add at least one transformation step before running the pipeline."));
      return undefined;
    }

    dispatch(transformPending());

    try {
      const transformResult = await transformDatasetVersionRequest({
        sourceDatasetVersionId: state.selectedSourceVersionId,
        steps: serializeDatasetTransformationSteps(state.steps),
      });

      dispatch(transformSuccess(transformResult));
      return transformResult;
    } catch (error) {
      dispatch(
        transformError(
          getApiErrorMessage(
            error,
            "Unable to transform this dataset version right now.",
          ),
        ),
      );

      return undefined;
    }
  };

  const resetTransformation = () => {
    dispatch(resetTransformationAction(state.selectedSourceVersionId));
  };

  return (
    <DatasetTransformationBuilderStateContext.Provider value={state}>
      <DatasetTransformationBuilderActionContext.Provider
        value={{
          setSourceVersionId,
          addStep,
          updateStep,
          removeStep,
          moveStep,
          transformDatasetVersion,
          resetTransformation,
        }}
      >
        {children}
      </DatasetTransformationBuilderActionContext.Provider>
    </DatasetTransformationBuilderStateContext.Provider>
  );
};

export const useDatasetTransformationBuilderState = () => {
  const context = useContext(DatasetTransformationBuilderStateContext);

  if (!context) {
    throw new Error(
      "useDatasetTransformationBuilderState must be used within a DatasetTransformationBuilderProvider",
    );
  }

  return context;
};

export const useDatasetTransformationBuilderActions = () => {
  const context = useContext(DatasetTransformationBuilderActionContext);

  if (!context) {
    throw new Error(
      "useDatasetTransformationBuilderActions must be used within a DatasetTransformationBuilderProvider",
    );
  }

  return context;
};
