"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  getDatasetBarChart as getDatasetBarChartRequest,
  getDatasetCorrelation as getDatasetCorrelationRequest,
  getDatasetDistribution as getDatasetDistributionRequest,
  getDatasetHistogram as getDatasetHistogramRequest,
  getDatasetScatterPlot as getDatasetScatterPlotRequest,
} from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  analysisError,
  analysisPending,
  barChartSuccess,
  clearAnalysisResult as clearAnalysisResultAction,
  correlationSuccess,
  distributionSuccess,
  histogramSuccess,
  scatterPlotSuccess,
  setActiveTab as setActiveTabAction,
} from "./actions";
import {
  DatasetExplorationAnalysisActionContext,
  DatasetExplorationAnalysisStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetExplorationAnalysisReducer } from "./reducer";
import type {
  DatasetBarChartRequest,
  DatasetCorrelationRequest,
  DatasetDistributionRequest,
  DatasetExplorationWorkspaceTab,
  DatasetHistogramRequest,
  DatasetScatterPlotRequest,
} from "@/types/datasets";

export const DatasetExplorationAnalysisProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(
    DatasetExplorationAnalysisReducer,
    INITIAL_STATE,
  );

  const setActiveTab = (tab: DatasetExplorationWorkspaceTab) => {
    dispatch(setActiveTabAction(tab));
  };

  const getHistogram = async (request: DatasetHistogramRequest) => {
    dispatch(analysisPending("histogram"));

    try {
      const histogram = await getDatasetHistogramRequest(request);
      dispatch(histogramSuccess({ activeTab: "histogram", request, histogram }));
    } catch (error) {
      dispatch(
        analysisError({
          activeTab: "histogram",
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load histogram analysis right now.",
          ),
        }),
      );
    }
  };

  const getBarChart = async (request: DatasetBarChartRequest) => {
    dispatch(analysisPending("barChart"));

    try {
      const barChart = await getDatasetBarChartRequest(request);
      dispatch(barChartSuccess({ activeTab: "barChart", request, barChart }));
    } catch (error) {
      dispatch(
        analysisError({
          activeTab: "barChart",
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load bar chart analysis right now.",
          ),
        }),
      );
    }
  };

  const getScatterPlot = async (request: DatasetScatterPlotRequest) => {
    dispatch(analysisPending("scatterPlot"));

    try {
      const scatterPlot = await getDatasetScatterPlotRequest(request);
      dispatch(
        scatterPlotSuccess({
          activeTab: "scatterPlot",
          request,
          scatterPlot,
        }),
      );
    } catch (error) {
      dispatch(
        analysisError({
          activeTab: "scatterPlot",
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load scatter plot analysis right now.",
          ),
        }),
      );
    }
  };

  const getDistribution = async (request: DatasetDistributionRequest) => {
    dispatch(analysisPending("distribution"));

    try {
      const distribution = await getDatasetDistributionRequest(request);
      dispatch(
        distributionSuccess({
          activeTab: "distribution",
          request,
          distribution,
        }),
      );
    } catch (error) {
      dispatch(
        analysisError({
          activeTab: "distribution",
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load distribution analysis right now.",
          ),
        }),
      );
    }
  };

  const getCorrelation = async (request: DatasetCorrelationRequest) => {
    dispatch(analysisPending("correlation"));

    try {
      const correlation = await getDatasetCorrelationRequest(request);
      dispatch(
        correlationSuccess({
          activeTab: "correlation",
          request,
          correlation,
        }),
      );
    } catch (error) {
      dispatch(
        analysisError({
          activeTab: "correlation",
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load correlation analysis right now.",
          ),
        }),
      );
    }
  };

  const clearAnalysisResult = () => {
    dispatch(clearAnalysisResultAction());
  };

  return (
    <DatasetExplorationAnalysisStateContext.Provider value={state}>
      <DatasetExplorationAnalysisActionContext.Provider
        value={{
          setActiveTab,
          getHistogram,
          getBarChart,
          getScatterPlot,
          getDistribution,
          getCorrelation,
          clearAnalysisResult,
        }}
      >
        {children}
      </DatasetExplorationAnalysisActionContext.Provider>
    </DatasetExplorationAnalysisStateContext.Provider>
  );
};

export const useDatasetExplorationAnalysisState = () => {
  const context = useContext(DatasetExplorationAnalysisStateContext);

  if (!context) {
    throw new Error(
      "useDatasetExplorationAnalysisState must be used within a DatasetExplorationAnalysisProvider",
    );
  }

  return context;
};

export const useDatasetExplorationAnalysisActions = () => {
  const context = useContext(DatasetExplorationAnalysisActionContext);

  if (!context) {
    throw new Error(
      "useDatasetExplorationAnalysisActions must be used within a DatasetExplorationAnalysisProvider",
    );
  }

  return context;
};
