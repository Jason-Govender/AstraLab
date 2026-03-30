import { createContext } from "react";
import type { UploadedRawDatasetResult, UploadRawDatasetFormValues } from "@/types/datasets";

export interface IDatasetIngestionStateContext {
  isUploading: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage?: string;
  uploadResult?: UploadedRawDatasetResult;
}

export interface IDatasetIngestionActionContext {
  uploadRawDataset: (
    values: UploadRawDatasetFormValues,
  ) => Promise<UploadedRawDatasetResult>;
  clearFeedback: () => void;
  resetUpload: () => void;
}

export const INITIAL_STATE: IDatasetIngestionStateContext = {
  isUploading: false,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DatasetIngestionStateContext =
  createContext<IDatasetIngestionStateContext | undefined>(undefined);

export const DatasetIngestionActionContext =
  createContext<IDatasetIngestionActionContext | undefined>(undefined);
