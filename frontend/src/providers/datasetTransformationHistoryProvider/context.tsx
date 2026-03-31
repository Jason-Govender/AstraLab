import { createContext } from "react";
import type { DatasetTransformationHistory } from "@/types/datasets";

export interface IDatasetTransformationHistoryStateContext {
  isLoadingHistory: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  currentDatasetId?: number;
  history?: DatasetTransformationHistory;
}

export interface IDatasetTransformationHistoryActionContext {
  getTransformationHistory: (datasetId: number) => Promise<void>;
  refreshTransformationHistory: () => Promise<void>;
}

export const INITIAL_STATE: IDatasetTransformationHistoryStateContext = {
  isLoadingHistory: false,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DatasetTransformationHistoryStateContext =
  createContext<IDatasetTransformationHistoryStateContext | undefined>(undefined);

export const DatasetTransformationHistoryActionContext =
  createContext<IDatasetTransformationHistoryActionContext | undefined>(undefined);
