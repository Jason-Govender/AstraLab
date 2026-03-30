"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import { getDatasetDetails as getDatasetDetailsRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  getDetailsError,
  getDetailsPending,
  getDetailsSuccess,
  setSelectedVersionId as setSelectedVersionIdAction,
} from "./actions";
import {
  DatasetDetailsActionContext,
  DatasetDetailsStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetDetailsReducer } from "./reducer";

export const DatasetDetailsProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetDetailsReducer, INITIAL_STATE);

  const getDatasetDetails = async (
    datasetId: number,
    selectedVersionId?: number,
  ) => {
    dispatch(getDetailsPending({ datasetId, selectedVersionId }));

    try {
      const details = await getDatasetDetailsRequest(datasetId, selectedVersionId);
      dispatch(getDetailsSuccess({ datasetId, selectedVersionId, details }));
    } catch (error) {
      dispatch(
        getDetailsError({
          datasetId,
          selectedVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load dataset details right now.",
          ),
        }),
      );
    }
  };

  const setSelectedVersionId = (selectedVersionId?: number) => {
    dispatch(setSelectedVersionIdAction(selectedVersionId));
  };

  const refreshDetails = async () => {
    if (!state.currentDatasetId) {
      return;
    }

    await getDatasetDetails(state.currentDatasetId, state.selectedVersionId);
  };

  return (
    <DatasetDetailsStateContext.Provider value={state}>
      <DatasetDetailsActionContext.Provider
        value={{
          getDatasetDetails,
          setSelectedVersionId,
          refreshDetails,
        }}
      >
        {children}
      </DatasetDetailsActionContext.Provider>
    </DatasetDetailsStateContext.Provider>
  );
};

export const useDatasetDetailsState = () => {
  const context = useContext(DatasetDetailsStateContext);

  if (!context) {
    throw new Error(
      "useDatasetDetailsState must be used within a DatasetDetailsProvider",
    );
  }

  return context;
};

export const useDatasetDetailsActions = () => {
  const context = useContext(DatasetDetailsActionContext);

  if (!context) {
    throw new Error(
      "useDatasetDetailsActions must be used within a DatasetDetailsProvider",
    );
  }

  return context;
};
