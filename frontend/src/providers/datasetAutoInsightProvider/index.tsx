"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { getLatestAutomaticInsight as getLatestAutomaticInsightRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearInsight,
  getInsightError,
  getInsightPending,
  getInsightSuccess,
} from "./actions";
import {
  DatasetAutoInsightActionContext,
  DatasetAutoInsightStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetAutoInsightReducer } from "./reducer";

export const DatasetAutoInsightProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetAutoInsightReducer, INITIAL_STATE);

  const getLatestAutomaticInsight = async (datasetVersionId: number) => {
    dispatch(getInsightPending(datasetVersionId));

    try {
      const latestInsight = await getLatestAutomaticInsightRequest(datasetVersionId);
      dispatch(getInsightSuccess({ datasetVersionId, latestInsight }));
    } catch (error) {
      dispatch(
        getInsightError({
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the automatic dataset insight right now.",
          ),
        }),
      );
    }
  };

  const refreshLatestAutomaticInsight = async () => {
    if (!state.currentDatasetVersionId) {
      return;
    }

    await getLatestAutomaticInsight(state.currentDatasetVersionId);
  };

  const clearLatestAutomaticInsight = () => {
    dispatch(clearInsight());
  };

  return (
    <DatasetAutoInsightStateContext.Provider value={state}>
      <DatasetAutoInsightActionContext.Provider
        value={{
          getLatestAutomaticInsight,
          refreshLatestAutomaticInsight,
          clearLatestAutomaticInsight,
        }}
      >
        {children}
      </DatasetAutoInsightActionContext.Provider>
    </DatasetAutoInsightStateContext.Provider>
  );
};

export const useDatasetAutoInsightState = () => {
  const context = useContext(DatasetAutoInsightStateContext);

  if (!context) {
    throw new Error(
      "useDatasetAutoInsightState must be used within a DatasetAutoInsightProvider",
    );
  }

  return context;
};

export const useDatasetAutoInsightActions = () => {
  const context = useContext(DatasetAutoInsightActionContext);

  if (!context) {
    throw new Error(
      "useDatasetAutoInsightActions must be used within a DatasetAutoInsightProvider",
    );
  }

  return context;
};
