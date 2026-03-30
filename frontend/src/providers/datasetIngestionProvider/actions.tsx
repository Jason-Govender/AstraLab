import type { UploadedRawDatasetResult } from "@/types/datasets";
import type { IDatasetIngestionStateContext } from "./context";

type DatasetIngestionStatePatch = Partial<IDatasetIngestionStateContext>;

export enum DatasetIngestionActionEnums {
  uploadPending = "DATASET_UPLOAD_PENDING",
  uploadSuccess = "DATASET_UPLOAD_SUCCESS",
  uploadError = "DATASET_UPLOAD_ERROR",
  clearFeedback = "DATASET_UPLOAD_CLEAR_FEEDBACK",
  reset = "DATASET_UPLOAD_RESET",
}

interface UploadPendingAction {
  type: DatasetIngestionActionEnums.uploadPending;
  payload: DatasetIngestionStatePatch;
}

interface UploadSuccessAction {
  type: DatasetIngestionActionEnums.uploadSuccess;
  payload: DatasetIngestionStatePatch;
}

interface UploadErrorAction {
  type: DatasetIngestionActionEnums.uploadError;
  payload: DatasetIngestionStatePatch;
}

interface ClearFeedbackAction {
  type: DatasetIngestionActionEnums.clearFeedback;
  payload: DatasetIngestionStatePatch;
}

interface ResetUploadAction {
  type: DatasetIngestionActionEnums.reset;
  payload: DatasetIngestionStatePatch;
}

export type DatasetIngestionAction =
  | UploadPendingAction
  | UploadSuccessAction
  | UploadErrorAction
  | ClearFeedbackAction
  | ResetUploadAction;

export const uploadPending = (): UploadPendingAction => ({
  type: DatasetIngestionActionEnums.uploadPending,
  payload: {
    isUploading: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const uploadSuccess = (
  uploadResult: UploadedRawDatasetResult,
): UploadSuccessAction => ({
  type: DatasetIngestionActionEnums.uploadSuccess,
  payload: {
    isUploading: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    uploadResult,
  },
});

export const uploadError = (errorMessage: string): UploadErrorAction => ({
  type: DatasetIngestionActionEnums.uploadError,
  payload: {
    isUploading: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  },
});

export const clearFeedback = (): ClearFeedbackAction => ({
  type: DatasetIngestionActionEnums.clearFeedback,
  payload: {
    isPending: false,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
  },
});

export const resetUpload = (): ResetUploadAction => ({
  type: DatasetIngestionActionEnums.reset,
  payload: {
    isUploading: false,
    isPending: false,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    uploadResult: undefined,
  },
});
