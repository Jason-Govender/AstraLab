"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { getDatasetTransformationHistory as getDatasetTransformationHistoryRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  getHistoryError,
  getHistoryPending,
  getHistorySuccess,
} from "./actions";
import {
  DatasetTransformationHistoryActionContext,
  DatasetTransformationHistoryStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetTransformationHistoryReducer } from "./reducer";

export const DatasetTransformationHistoryProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(
    DatasetTransformationHistoryReducer,
    INITIAL_STATE,
  );

  const getTransformationHistory = async (datasetId: number) => {
    dispatch(getHistoryPending(datasetId));

    try {
      const history = await getDatasetTransformationHistoryRequest(datasetId);
      dispatch(getHistorySuccess({ datasetId, history }));
    } catch (error) {
      dispatch(
        getHistoryError({
          datasetId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load dataset transformation history right now.",
          ),
        }),
      );
    }
  };

  const refreshTransformationHistory = async () => {
    if (!state.currentDatasetId) {
      return;
    }

    await getTransformationHistory(state.currentDatasetId);
  };

  return (
    <DatasetTransformationHistoryStateContext.Provider value={state}>
      <DatasetTransformationHistoryActionContext.Provider
        value={{
          getTransformationHistory,
          refreshTransformationHistory,
        }}
      >
        {children}
      </DatasetTransformationHistoryActionContext.Provider>
    </DatasetTransformationHistoryStateContext.Provider>
  );
};

export const useDatasetTransformationHistoryState = () => {
  const context = useContext(DatasetTransformationHistoryStateContext);

  if (!context) {
    throw new Error(
      "useDatasetTransformationHistoryState must be used within a DatasetTransformationHistoryProvider",
    );
  }

  return context;
};

export const useDatasetTransformationHistoryActions = () => {
  const context = useContext(DatasetTransformationHistoryActionContext);

  if (!context) {
    throw new Error(
      "useDatasetTransformationHistoryActions must be used within a DatasetTransformationHistoryProvider",
    );
  }

  return context;
};
