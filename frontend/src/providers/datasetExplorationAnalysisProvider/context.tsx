import { createContext } from "react";
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
import { DEFAULT_DATASET_EXPLORATION_TAB } from "@/constants/datasetExploration";

export interface IDatasetExplorationAnalysisStateContext {
  activeTab: DatasetExplorationWorkspaceTab;
  isLoadingAnalysis: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  histogram?: HistogramChart;
  barChart?: BarChart;
  scatterPlot?: ScatterPlot;
  distribution?: DistributionAnalysis;
  correlation?: CorrelationAnalysis;
  lastHistogramRequest?: DatasetHistogramRequest;
  lastBarChartRequest?: DatasetBarChartRequest;
  lastScatterPlotRequest?: DatasetScatterPlotRequest;
  lastDistributionRequest?: DatasetDistributionRequest;
  lastCorrelationRequest?: DatasetCorrelationRequest;
}

export interface IDatasetExplorationAnalysisActionContext {
  setActiveTab: (tab: DatasetExplorationWorkspaceTab) => void;
  getHistogram: (request: DatasetHistogramRequest) => Promise<void>;
  getBarChart: (request: DatasetBarChartRequest) => Promise<void>;
  getScatterPlot: (request: DatasetScatterPlotRequest) => Promise<void>;
  getDistribution: (request: DatasetDistributionRequest) => Promise<void>;
  getCorrelation: (request: DatasetCorrelationRequest) => Promise<void>;
  clearAnalysisResult: () => void;
}

export const INITIAL_STATE: IDatasetExplorationAnalysisStateContext = {
  activeTab: DEFAULT_DATASET_EXPLORATION_TAB,
  isLoadingAnalysis: false,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DatasetExplorationAnalysisStateContext =
  createContext<IDatasetExplorationAnalysisStateContext | undefined>(undefined);

export const DatasetExplorationAnalysisActionContext =
  createContext<IDatasetExplorationAnalysisActionContext | undefined>(undefined);
