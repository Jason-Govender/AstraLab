import { createContext } from "react";
import type {
  DatasetColumnInsight,
  DatasetDetails,
  DatasetProfileColumnsRequest,
} from "@/types/datasets";

export interface IDatasetDetailsStateContext {
  isLoadingDetails: boolean;
  isLoadingProfileColumns: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  isProfileColumnsError: boolean;
  errorMessage?: string;
  profileColumnsErrorMessage?: string;
  currentDatasetId?: number;
  selectedVersionId?: number;
  currentProfileVersionId?: number;
  lastProfileColumnsRequest?: DatasetProfileColumnsRequest;
  details?: DatasetDetails;
  profileColumns: DatasetColumnInsight[];
  profileColumnsTotalCount: number;
}

export interface IDatasetDetailsActionContext {
  getDatasetDetails: (
    datasetId: number,
    selectedVersionId?: number,
  ) => Promise<void>;
  getProfileColumns: (request: DatasetProfileColumnsRequest) => Promise<void>;
  setSelectedVersionId: (selectedVersionId?: number) => void;
  clearProfileColumns: () => void;
  refreshDetails: () => Promise<void>;
  refreshProfileColumns: () => Promise<void>;
}

export const INITIAL_STATE: IDatasetDetailsStateContext = {
  isLoadingDetails: false,
  isLoadingProfileColumns: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  isProfileColumnsError: false,
  profileColumns: [],
  profileColumnsTotalCount: 0,
};

export const DatasetDetailsStateContext =
  createContext<IDatasetDetailsStateContext | undefined>(undefined);

export const DatasetDetailsActionContext =
  createContext<IDatasetDetailsActionContext | undefined>(undefined);
