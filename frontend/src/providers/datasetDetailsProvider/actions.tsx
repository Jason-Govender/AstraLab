import type { AbpPagedResult } from "@/types/api";
import type {
  DatasetColumnInsight,
  DatasetDetails,
  DatasetProfileColumnsRequest,
} from "@/types/datasets";
import type { IDatasetDetailsStateContext } from "./context";

type DatasetDetailsStatePatch = Partial<IDatasetDetailsStateContext>;

export enum DatasetDetailsActionEnums {
  getDetailsPending = "DATASET_DETAILS_PENDING",
  getDetailsSuccess = "DATASET_DETAILS_SUCCESS",
  getDetailsError = "DATASET_DETAILS_ERROR",
  getProfileColumnsPending = "DATASET_PROFILE_COLUMNS_PENDING",
  getProfileColumnsSuccess = "DATASET_PROFILE_COLUMNS_SUCCESS",
  getProfileColumnsError = "DATASET_PROFILE_COLUMNS_ERROR",
  clearProfileColumns = "DATASET_PROFILE_COLUMNS_CLEAR",
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

interface GetProfileColumnsPendingAction {
  type: DatasetDetailsActionEnums.getProfileColumnsPending;
  payload: DatasetDetailsStatePatch;
}

interface GetProfileColumnsSuccessAction {
  type: DatasetDetailsActionEnums.getProfileColumnsSuccess;
  payload: DatasetDetailsStatePatch;
}

interface GetProfileColumnsErrorAction {
  type: DatasetDetailsActionEnums.getProfileColumnsError;
  payload: DatasetDetailsStatePatch;
}

interface ClearProfileColumnsAction {
  type: DatasetDetailsActionEnums.clearProfileColumns;
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
  | GetProfileColumnsPendingAction
  | GetProfileColumnsSuccessAction
  | GetProfileColumnsErrorAction
  | ClearProfileColumnsAction
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

export const getProfileColumnsPending = (
  request: DatasetProfileColumnsRequest,
): GetProfileColumnsPendingAction => ({
  type: DatasetDetailsActionEnums.getProfileColumnsPending,
  payload: {
    isLoadingProfileColumns: true,
    isProfileColumnsError: false,
    profileColumnsErrorMessage: undefined,
    currentProfileVersionId: request.datasetVersionId,
    lastProfileColumnsRequest: request,
  },
});

export const getProfileColumnsSuccess = ({
  request,
  result,
}: {
  request: DatasetProfileColumnsRequest;
  result: AbpPagedResult<DatasetColumnInsight>;
}): GetProfileColumnsSuccessAction => ({
  type: DatasetDetailsActionEnums.getProfileColumnsSuccess,
  payload: {
    isLoadingProfileColumns: false,
    isProfileColumnsError: false,
    profileColumnsErrorMessage: undefined,
    currentProfileVersionId: request.datasetVersionId,
    lastProfileColumnsRequest: request,
    profileColumns: result.items,
    profileColumnsTotalCount: result.totalCount,
  },
});

export const getProfileColumnsError = ({
  request,
  errorMessage,
}: {
  request: DatasetProfileColumnsRequest;
  errorMessage: string;
}): GetProfileColumnsErrorAction => ({
  type: DatasetDetailsActionEnums.getProfileColumnsError,
  payload: {
    isLoadingProfileColumns: false,
    isProfileColumnsError: true,
    profileColumnsErrorMessage: errorMessage,
    currentProfileVersionId: request.datasetVersionId,
    lastProfileColumnsRequest: request,
    profileColumns: [],
    profileColumnsTotalCount: 0,
  },
});

export const clearProfileColumns = (): ClearProfileColumnsAction => ({
  type: DatasetDetailsActionEnums.clearProfileColumns,
  payload: {
    isLoadingProfileColumns: false,
    isProfileColumnsError: false,
    profileColumnsErrorMessage: undefined,
    currentProfileVersionId: undefined,
    lastProfileColumnsRequest: undefined,
    profileColumns: [],
    profileColumnsTotalCount: 0,
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
