"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { getLatestAutomaticExperimentInsight } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearInsight,
  getInsightError,
  getInsightPending,
  getInsightSuccess,
} from "./actions";
import {
  INITIAL_STATE,
  MlExperimentAutoInsightActionContext,
  MlExperimentAutoInsightStateContext,
} from "./context";
import { MlExperimentAutoInsightReducer } from "./reducer";

export const MlExperimentAutoInsightProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(
    MlExperimentAutoInsightReducer,
    INITIAL_STATE,
  );

  const getLatestAutomaticInsight = async (experimentId?: number) => {
    dispatch(getInsightPending({ experimentId }));

    if (!experimentId) {
      dispatch(getInsightSuccess({ experimentId: undefined, insight: null }));
      return;
    }

    try {
      const insight = await getLatestAutomaticExperimentInsight(experimentId);
      dispatch(getInsightSuccess({ experimentId, insight }));
    } catch (error) {
      dispatch(
        getInsightError({
          experimentId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the latest automatic ML insight right now.",
          ),
        }),
      );
    }
  };

  const clearLatestAutomaticInsight = () => {
    dispatch(clearInsight());
  };

  return (
    <MlExperimentAutoInsightStateContext.Provider value={state}>
      <MlExperimentAutoInsightActionContext.Provider
        value={{
          getLatestAutomaticInsight,
          clearLatestAutomaticInsight,
        }}
      >
        {children}
      </MlExperimentAutoInsightActionContext.Provider>
    </MlExperimentAutoInsightStateContext.Provider>
  );
};

export const useMlExperimentAutoInsightState = () => {
  const context = useContext(MlExperimentAutoInsightStateContext);

  if (!context) {
    throw new Error(
      "useMlExperimentAutoInsightState must be used within a MlExperimentAutoInsightProvider",
    );
  }

  return context;
};

export const useMlExperimentAutoInsightActions = () => {
  const context = useContext(MlExperimentAutoInsightActionContext);

  if (!context) {
    throw new Error(
      "useMlExperimentAutoInsightActions must be used within a MlExperimentAutoInsightProvider",
    );
  }

  return context;
};
