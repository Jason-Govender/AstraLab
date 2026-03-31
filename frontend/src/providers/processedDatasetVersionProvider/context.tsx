import { createContext } from "react";
import type { ProcessedDatasetVersion } from "@/types/datasets";

export interface IProcessedDatasetVersionStateContext {
  isLoadingProcessedVersion: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  currentVersionId?: number;
  processedVersion?: ProcessedDatasetVersion;
}

export interface IProcessedDatasetVersionActionContext {
  getProcessedDatasetVersion: (datasetVersionId: number) => Promise<void>;
  refreshProcessedDatasetVersion: () => Promise<void>;
}

export const INITIAL_STATE: IProcessedDatasetVersionStateContext = {
  isLoadingProcessedVersion: false,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ProcessedDatasetVersionStateContext =
  createContext<IProcessedDatasetVersionStateContext | undefined>(undefined);

export const ProcessedDatasetVersionActionContext =
  createContext<IProcessedDatasetVersionActionContext | undefined>(undefined);
