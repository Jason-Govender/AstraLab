"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  getDatasetDetails as getDatasetDetailsRequest,
  getDatasetProfileColumns as getDatasetProfileColumnsRequest,
} from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearProfileColumns as clearProfileColumnsAction,
  getDetailsError,
  getDetailsPending,
  getDetailsSuccess,
  getProfileColumnsError,
  getProfileColumnsPending,
  getProfileColumnsSuccess,
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

  const getProfileColumns = async (
    request: Parameters<typeof getDatasetProfileColumnsRequest>[0],
  ) => {
    dispatch(getProfileColumnsPending(request));

    try {
      const result = await getDatasetProfileColumnsRequest(request);
      dispatch(getProfileColumnsSuccess({ request, result }));
    } catch (error) {
      dispatch(
        getProfileColumnsError({
          request,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load dataset profiling insights right now.",
          ),
        }),
      );
    }
  };

  const clearProfileColumns = () => {
    dispatch(clearProfileColumnsAction());
  };

  const refreshDetails = async () => {
    if (!state.currentDatasetId) {
      return;
    }

    await getDatasetDetails(state.currentDatasetId, state.selectedVersionId);
  };

  const refreshProfileColumns = async () => {
    if (!state.lastProfileColumnsRequest) {
      return;
    }

    await getProfileColumns(state.lastProfileColumnsRequest);
  };

  return (
    <DatasetDetailsStateContext.Provider value={state}>
      <DatasetDetailsActionContext.Provider
        value={{
          getDatasetDetails,
          getProfileColumns,
          setSelectedVersionId,
          clearProfileColumns,
          refreshDetails,
          refreshProfileColumns,
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
