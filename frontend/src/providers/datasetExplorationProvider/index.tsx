"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  getDatasetExplorationColumns as getDatasetExplorationColumnsRequest,
  getDatasetRows as getDatasetRowsRequest,
} from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearExplorationData as clearExplorationDataAction,
  getColumnsError,
  getColumnsPending,
  getColumnsSuccess,
  getRowsError,
  getRowsPending,
  getRowsSuccess,
  setRowQuery as setRowQueryAction,
} from "./actions";
import {
  DatasetExplorationActionContext,
  DatasetExplorationStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetExplorationReducer } from "./reducer";
import type { PagedDatasetRowsRequest } from "@/types/datasets";

export const DatasetExplorationProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetExplorationReducer, INITIAL_STATE);

  const getExplorationColumns = async (datasetVersionId: number) => {
    dispatch(getColumnsPending(datasetVersionId));

    try {
      const columns = await getDatasetExplorationColumnsRequest(datasetVersionId);
      dispatch(getColumnsSuccess({ datasetVersionId, columns }));
    } catch (error) {
      dispatch(
        getColumnsError({
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load exploration columns right now.",
          ),
        }),
      );
    }
  };

  const getRows = async (request: PagedDatasetRowsRequest) => {
    dispatch(getRowsPending(request));

    try {
      const result = await getDatasetRowsRequest(request);
      dispatch(
        getRowsSuccess({
          request,
          rows: result.items,
          totalRowCount: result.totalCount,
        }),
      );
    } catch (error) {
      dispatch(
        getRowsError({
          request,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load dataset rows right now.",
          ),
        }),
      );
    }
  };

  const setRowQuery = (request: PagedDatasetRowsRequest) => {
    dispatch(setRowQueryAction(request));
  };

  const clearExplorationData = () => {
    dispatch(clearExplorationDataAction());
  };

  const refreshColumns = async () => {
    if (!state.currentDatasetVersionId) {
      return;
    }

    await getExplorationColumns(state.currentDatasetVersionId);
  };

  const refreshRows = async () => {
    if (!state.rowQuery) {
      return;
    }

    await getRows(state.rowQuery);
  };

  return (
    <DatasetExplorationStateContext.Provider value={state}>
      <DatasetExplorationActionContext.Provider
        value={{
          getExplorationColumns,
          getRows,
          setRowQuery,
          clearExplorationData,
          refreshColumns,
          refreshRows,
        }}
      >
        {children}
      </DatasetExplorationActionContext.Provider>
    </DatasetExplorationStateContext.Provider>
  );
};

export const useDatasetExplorationState = () => {
  const context = useContext(DatasetExplorationStateContext);

  if (!context) {
    throw new Error(
      "useDatasetExplorationState must be used within a DatasetExplorationProvider",
    );
  }

  return context;
};

export const useDatasetExplorationActions = () => {
  const context = useContext(DatasetExplorationActionContext);

  if (!context) {
    throw new Error(
      "useDatasetExplorationActions must be used within a DatasetExplorationProvider",
    );
  }

  return context;
};
