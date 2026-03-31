import type { ProcessedDatasetVersion } from "@/types/datasets";
import type { IProcessedDatasetVersionStateContext } from "./context";

type ProcessedDatasetVersionStatePatch =
  Partial<IProcessedDatasetVersionStateContext>;

export enum ProcessedDatasetVersionActionEnums {
  getProcessedVersionPending = "PROCESSED_DATASET_VERSION_PENDING",
  getProcessedVersionSuccess = "PROCESSED_DATASET_VERSION_SUCCESS",
  getProcessedVersionError = "PROCESSED_DATASET_VERSION_ERROR",
}

interface GetProcessedVersionPendingAction {
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionPending;
  payload: ProcessedDatasetVersionStatePatch;
}

interface GetProcessedVersionSuccessAction {
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionSuccess;
  payload: ProcessedDatasetVersionStatePatch;
}

interface GetProcessedVersionErrorAction {
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionError;
  payload: ProcessedDatasetVersionStatePatch;
}

export type ProcessedDatasetVersionAction =
  | GetProcessedVersionPendingAction
  | GetProcessedVersionSuccessAction
  | GetProcessedVersionErrorAction;

export const getProcessedVersionPending = (
  datasetVersionId: number,
): GetProcessedVersionPendingAction => ({
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionPending,
  payload: {
    isLoadingProcessedVersion: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    currentVersionId: datasetVersionId,
  },
});

export const getProcessedVersionSuccess = ({
  datasetVersionId,
  processedVersion,
}: {
  datasetVersionId: number;
  processedVersion: ProcessedDatasetVersion;
}): GetProcessedVersionSuccessAction => ({
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionSuccess,
  payload: {
    isLoadingProcessedVersion: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    currentVersionId: datasetVersionId,
    processedVersion,
  },
});

export const getProcessedVersionError = ({
  datasetVersionId,
  errorMessage,
}: {
  datasetVersionId: number;
  errorMessage: string;
}): GetProcessedVersionErrorAction => ({
  type: ProcessedDatasetVersionActionEnums.getProcessedVersionError,
  payload: {
    isLoadingProcessedVersion: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
    currentVersionId: datasetVersionId,
  },
});
