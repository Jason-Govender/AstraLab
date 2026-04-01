import type { AbpApiResponse } from "@/types/api";
import type {
  CreateMlExperimentRequest,
  MlExperiment,
} from "@/types/ml";
import { axiosInstance } from "@/utils/axiosInstance";

const ML_EXPERIMENT_CREATE_ENDPOINT = "/api/services/app/MLExperiment/Create";
const ML_EXPERIMENT_GET_ENDPOINT = "/api/services/app/MLExperiment/Get";
const ML_EXPERIMENT_GET_BY_DATASET_VERSION_ENDPOINT =
  "/api/services/app/MLExperiment/GetByDatasetVersion";
const ML_EXPERIMENT_CANCEL_ENDPOINT = "/api/services/app/MLExperiment/Cancel";
const ML_EXPERIMENT_RETRY_ENDPOINT = "/api/services/app/MLExperiment/Retry";

interface AbpListResult<T> {
  items: T[];
}

export const createMlExperiment = async (
  request: CreateMlExperimentRequest,
): Promise<MlExperiment> => {
  const response = await axiosInstance.post<AbpApiResponse<MlExperiment>>(
    ML_EXPERIMENT_CREATE_ENDPOINT,
    request,
  );

  return response.data.result;
};

export const getMlExperiment = async (
  experimentId: number,
): Promise<MlExperiment> => {
  const response = await axiosInstance.get<AbpApiResponse<MlExperiment>>(
    ML_EXPERIMENT_GET_ENDPOINT,
    {
      params: {
        id: experimentId,
      },
    },
  );

  return response.data.result;
};

export const getMlExperimentsByDatasetVersion = async (
  datasetVersionId: number,
): Promise<MlExperiment[]> => {
  const response = await axiosInstance.get<AbpApiResponse<AbpListResult<MlExperiment>>>(
    ML_EXPERIMENT_GET_BY_DATASET_VERSION_ENDPOINT,
    {
      params: {
        id: datasetVersionId,
      },
    },
  );

  return response.data.result.items;
};

export const cancelMlExperiment = async (
  experimentId: number,
): Promise<MlExperiment> => {
  const response = await axiosInstance.post<AbpApiResponse<MlExperiment>>(
    ML_EXPERIMENT_CANCEL_ENDPOINT,
    {
      id: experimentId,
    },
  );

  return response.data.result;
};

export const retryMlExperiment = async (
  experimentId: number,
): Promise<MlExperiment> => {
  const response = await axiosInstance.post<AbpApiResponse<MlExperiment>>(
    ML_EXPERIMENT_RETRY_ENDPOINT,
    {
      id: experimentId,
    },
  );

  return response.data.result;
};
