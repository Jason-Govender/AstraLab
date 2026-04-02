import { createContext } from "react";
import type { AIResponse } from "@/types/datasets";

export interface IDatasetAutoInsightStateContext {
  isLoading: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  currentDatasetVersionId?: number;
  latestInsight?: AIResponse | null;
}

export interface IDatasetAutoInsightActionContext {
  getLatestAutomaticInsight: (datasetVersionId: number) => Promise<void>;
  refreshLatestAutomaticInsight: () => Promise<void>;
  clearLatestAutomaticInsight: () => void;
}

export const INITIAL_STATE: IDatasetAutoInsightStateContext = {
  isLoading: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  latestInsight: null,
};

export const DatasetAutoInsightStateContext =
  createContext<IDatasetAutoInsightStateContext | undefined>(undefined);

export const DatasetAutoInsightActionContext =
  createContext<IDatasetAutoInsightActionContext | undefined>(undefined);
