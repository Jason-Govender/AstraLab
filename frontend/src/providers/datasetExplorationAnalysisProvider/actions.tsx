import type {
  BarChart,
  CorrelationAnalysis,
  DatasetBarChartRequest,
  DatasetCorrelationRequest,
  DatasetDistributionRequest,
  DatasetExplorationWorkspaceTab,
  DatasetHistogramRequest,
  DatasetScatterPlotRequest,
  DistributionAnalysis,
  HistogramChart,
  ScatterPlot,
} from "@/types/datasets";
import type { IDatasetExplorationAnalysisStateContext } from "./context";

type DatasetExplorationAnalysisStatePatch =
  Partial<IDatasetExplorationAnalysisStateContext>;

export enum DatasetExplorationAnalysisActionEnums {
  setActiveTab = "DATASET_EXPLORATION_ANALYSIS_SET_ACTIVE_TAB",
  analysisPending = "DATASET_EXPLORATION_ANALYSIS_PENDING",
  histogramSuccess = "DATASET_EXPLORATION_ANALYSIS_HISTOGRAM_SUCCESS",
  barChartSuccess = "DATASET_EXPLORATION_ANALYSIS_BAR_CHART_SUCCESS",
  scatterPlotSuccess = "DATASET_EXPLORATION_ANALYSIS_SCATTER_PLOT_SUCCESS",
  distributionSuccess = "DATASET_EXPLORATION_ANALYSIS_DISTRIBUTION_SUCCESS",
  correlationSuccess = "DATASET_EXPLORATION_ANALYSIS_CORRELATION_SUCCESS",
  analysisError = "DATASET_EXPLORATION_ANALYSIS_ERROR",
  clear = "DATASET_EXPLORATION_ANALYSIS_CLEAR",
}

interface DatasetExplorationAnalysisActionShape {
  type: DatasetExplorationAnalysisActionEnums;
  payload: DatasetExplorationAnalysisStatePatch;
}

export type DatasetExplorationAnalysisAction =
  DatasetExplorationAnalysisActionShape;

export const setActiveTab = (
  activeTab: DatasetExplorationWorkspaceTab,
): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.setActiveTab,
  payload: {
    activeTab,
  },
});

export const analysisPending = (
  activeTab: DatasetExplorationWorkspaceTab,
): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.analysisPending,
  payload: {
    activeTab,
    isLoadingAnalysis: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const histogramSuccess = ({
  activeTab,
  request,
  histogram,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  request: DatasetHistogramRequest;
  histogram: HistogramChart;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.histogramSuccess,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    histogram,
    lastHistogramRequest: request,
  },
});

export const barChartSuccess = ({
  activeTab,
  request,
  barChart,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  request: DatasetBarChartRequest;
  barChart: BarChart;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.barChartSuccess,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    barChart,
    lastBarChartRequest: request,
  },
});

export const scatterPlotSuccess = ({
  activeTab,
  request,
  scatterPlot,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  request: DatasetScatterPlotRequest;
  scatterPlot: ScatterPlot;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.scatterPlotSuccess,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    scatterPlot,
    lastScatterPlotRequest: request,
  },
});

export const distributionSuccess = ({
  activeTab,
  request,
  distribution,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  request: DatasetDistributionRequest;
  distribution: DistributionAnalysis;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.distributionSuccess,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    distribution,
    lastDistributionRequest: request,
  },
});

export const correlationSuccess = ({
  activeTab,
  request,
  correlation,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  request: DatasetCorrelationRequest;
  correlation: CorrelationAnalysis;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.correlationSuccess,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    correlation,
    lastCorrelationRequest: request,
  },
});

export const analysisError = ({
  activeTab,
  errorMessage,
}: {
  activeTab: DatasetExplorationWorkspaceTab;
  errorMessage: string;
}): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.analysisError,
  payload: {
    activeTab,
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  },
});

export const clearAnalysisResult = (): DatasetExplorationAnalysisAction => ({
  type: DatasetExplorationAnalysisActionEnums.clear,
  payload: {
    isLoadingAnalysis: false,
    isPending: false,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    histogram: undefined,
    barChart: undefined,
    scatterPlot: undefined,
    distribution: undefined,
    correlation: undefined,
    lastHistogramRequest: undefined,
    lastBarChartRequest: undefined,
    lastScatterPlotRequest: undefined,
    lastDistributionRequest: undefined,
    lastCorrelationRequest: undefined,
  },
});
