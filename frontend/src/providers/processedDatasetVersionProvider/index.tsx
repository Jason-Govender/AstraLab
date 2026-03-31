"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { getProcessedDatasetVersion as getProcessedDatasetVersionRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  getProcessedVersionError,
  getProcessedVersionPending,
  getProcessedVersionSuccess,
} from "./actions";
import {
  INITIAL_STATE,
  ProcessedDatasetVersionActionContext,
  ProcessedDatasetVersionStateContext,
} from "./context";
import { ProcessedDatasetVersionReducer } from "./reducer";

export const ProcessedDatasetVersionProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(
    ProcessedDatasetVersionReducer,
    INITIAL_STATE,
  );

  const getProcessedDatasetVersion = async (datasetVersionId: number) => {
    dispatch(getProcessedVersionPending(datasetVersionId));

    try {
      const processedVersion =
        await getProcessedDatasetVersionRequest(datasetVersionId);
      dispatch(
        getProcessedVersionSuccess({
          datasetVersionId,
          processedVersion,
        }),
      );
    } catch (error) {
      dispatch(
        getProcessedVersionError({
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the processed dataset version right now.",
          ),
        }),
      );
    }
  };

  const refreshProcessedDatasetVersion = async () => {
    if (!state.currentVersionId) {
      return;
    }

    await getProcessedDatasetVersion(state.currentVersionId);
  };

  return (
    <ProcessedDatasetVersionStateContext.Provider value={state}>
      <ProcessedDatasetVersionActionContext.Provider
        value={{
          getProcessedDatasetVersion,
          refreshProcessedDatasetVersion,
        }}
      >
        {children}
      </ProcessedDatasetVersionActionContext.Provider>
    </ProcessedDatasetVersionStateContext.Provider>
  );
};

export const useProcessedDatasetVersionState = () => {
  const context = useContext(ProcessedDatasetVersionStateContext);

  if (!context) {
    throw new Error(
      "useProcessedDatasetVersionState must be used within a ProcessedDatasetVersionProvider",
    );
  }

  return context;
};

export const useProcessedDatasetVersionActions = () => {
  const context = useContext(ProcessedDatasetVersionActionContext);

  if (!context) {
    throw new Error(
      "useProcessedDatasetVersionActions must be used within a ProcessedDatasetVersionProvider",
    );
  }

  return context;
};
