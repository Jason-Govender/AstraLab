"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import type { DatasetCatalogFilters } from "@/types/datasets";
import { getDatasets as getDatasetsRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  getDatasetsError,
  getDatasetsPending,
  getDatasetsSuccess,
  setFilters as setFiltersAction,
} from "./actions";
import {
  DatasetCatalogActionContext,
  DatasetCatalogStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetCatalogReducer } from "./reducer";

export const DatasetCatalogProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetCatalogReducer, INITIAL_STATE);

  const setFilters = (filters: DatasetCatalogFilters) => {
    dispatch(setFiltersAction(filters));
  };

  const getDatasets = async (filters: DatasetCatalogFilters) => {
    dispatch(getDatasetsPending(filters));

    try {
      const result = await getDatasetsRequest(filters);
      dispatch(
        getDatasetsSuccess({
          filters,
          datasets: result.items,
          totalCount: result.totalCount,
        }),
      );
    } catch (error) {
      dispatch(
        getDatasetsError({
          filters,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load datasets right now.",
          ),
        }),
      );
    }
  };

  const refreshDatasets = async () => {
    await getDatasets(state.filters);
  };

  return (
    <DatasetCatalogStateContext.Provider value={state}>
      <DatasetCatalogActionContext.Provider
        value={{
          setFilters,
          getDatasets,
          refreshDatasets,
        }}
      >
        {children}
      </DatasetCatalogActionContext.Provider>
    </DatasetCatalogStateContext.Provider>
  );
};

export const useDatasetCatalogState = () => {
  const context = useContext(DatasetCatalogStateContext);

  if (!context) {
    throw new Error(
      "useDatasetCatalogState must be used within a DatasetCatalogProvider",
    );
  }

  return context;
};

export const useDatasetCatalogActions = () => {
  const context = useContext(DatasetCatalogActionContext);

  if (!context) {
    throw new Error(
      "useDatasetCatalogActions must be used within a DatasetCatalogProvider",
    );
  }

  return context;
};
