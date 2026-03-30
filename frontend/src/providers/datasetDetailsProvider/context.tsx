import { createContext } from "react";
import type { DatasetDetails } from "@/types/datasets";

export interface IDatasetDetailsStateContext {
  isLoadingDetails: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  currentDatasetId?: number;
  selectedVersionId?: number;
  details?: DatasetDetails;
}

export interface IDatasetDetailsActionContext {
  getDatasetDetails: (
    datasetId: number,
    selectedVersionId?: number,
  ) => Promise<void>;
  setSelectedVersionId: (selectedVersionId?: number) => void;
  refreshDetails: () => Promise<void>;
}

export const INITIAL_STATE: IDatasetDetailsStateContext = {
  isLoadingDetails: false,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DatasetDetailsStateContext =
  createContext<IDatasetDetailsStateContext | undefined>(undefined);

export const DatasetDetailsActionContext =
  createContext<IDatasetDetailsActionContext | undefined>(undefined);
