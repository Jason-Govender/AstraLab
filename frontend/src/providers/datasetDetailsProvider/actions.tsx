import type { DatasetDetails } from "@/types/datasets";
import type { IDatasetDetailsStateContext } from "./context";

type DatasetDetailsStatePatch = Partial<IDatasetDetailsStateContext>;

export enum DatasetDetailsActionEnums {
  getDetailsPending = "DATASET_DETAILS_PENDING",
  getDetailsSuccess = "DATASET_DETAILS_SUCCESS",
  getDetailsError = "DATASET_DETAILS_ERROR",
  setSelectedVersionId = "DATASET_DETAILS_SET_SELECTED_VERSION_ID",
}

interface GetDetailsPendingAction {
  type: DatasetDetailsActionEnums.getDetailsPending;
  payload: DatasetDetailsStatePatch;
}

interface GetDetailsSuccessAction {
  type: DatasetDetailsActionEnums.getDetailsSuccess;
  payload: DatasetDetailsStatePatch;
}

interface GetDetailsErrorAction {
  type: DatasetDetailsActionEnums.getDetailsError;
  payload: DatasetDetailsStatePatch;
}

interface SetSelectedVersionIdAction {
  type: DatasetDetailsActionEnums.setSelectedVersionId;
  payload: DatasetDetailsStatePatch;
}

export type DatasetDetailsAction =
  | GetDetailsPendingAction
  | GetDetailsSuccessAction
  | GetDetailsErrorAction
  | SetSelectedVersionIdAction;

export const getDetailsPending = ({
  datasetId,
  selectedVersionId,
}: {
  datasetId: number;
  selectedVersionId?: number;
}): GetDetailsPendingAction => ({
  type: DatasetDetailsActionEnums.getDetailsPending,
  payload: {
    isLoadingDetails: true,
    isPending: true,
    isSuccess: false,
    isError: false,
    errorMessage: undefined,
    currentDatasetId: datasetId,
    selectedVersionId,
  },
});

export const getDetailsSuccess = ({
  datasetId,
  selectedVersionId,
  details,
}: {
  datasetId: number;
  selectedVersionId?: number;
  details: DatasetDetails;
}): GetDetailsSuccessAction => ({
  type: DatasetDetailsActionEnums.getDetailsSuccess,
  payload: {
    isLoadingDetails: false,
    isPending: false,
    isSuccess: true,
    isError: false,
    errorMessage: undefined,
    currentDatasetId: datasetId,
    selectedVersionId,
    details,
  },
});

export const getDetailsError = ({
  datasetId,
  selectedVersionId,
  errorMessage,
}: {
  datasetId: number;
  selectedVersionId?: number;
  errorMessage: string;
}): GetDetailsErrorAction => ({
  type: DatasetDetailsActionEnums.getDetailsError,
  payload: {
    isLoadingDetails: false,
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
    currentDatasetId: datasetId,
    selectedVersionId,
  },
});

export const setSelectedVersionId = (
  selectedVersionId?: number,
): SetSelectedVersionIdAction => ({
  type: DatasetDetailsActionEnums.setSelectedVersionId,
  payload: {
    selectedVersionId,
  },
});
