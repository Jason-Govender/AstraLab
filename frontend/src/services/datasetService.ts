import type { AbpApiResponse, AbpPagedResult } from "@/types/api";
import type {
  DatasetCatalogFilters,
  DatasetColumnInsight,
  DatasetDetails,
  DatasetListItem,
  DatasetProfileColumnsRequest,
  DatasetProfileSummary,
  DatasetTransformationHistory,
  ProcessedDatasetVersion,
  TransformDatasetVersionRequest,
  TransformDatasetVersionResult,
  UploadedRawDatasetResult,
  UploadRawDatasetPayload,
} from "@/types/datasets";
import { axiosInstance } from "@/utils/axiosInstance";

const DATASET_UPLOAD_ENDPOINT = "/api/services/app/datasets/upload-raw";
const DATASET_GET_ALL_ENDPOINT = "/api/services/app/Dataset/GetAll";
const DATASET_GET_DETAILS_ENDPOINT = "/api/services/app/Dataset/GetDetails";
const DATASET_PROFILE_GET_ENDPOINT = "/api/services/app/DatasetProfiling/Get";
const DATASET_PROFILE_GET_COLUMNS_ENDPOINT =
  "/api/services/app/DatasetProfiling/GetColumns";
const DATASET_TRANSFORMATION_HISTORY_ENDPOINT =
  "/api/services/app/DatasetTransformation/GetHistory";
const DATASET_TRANSFORMATION_PROCESSED_VERSION_ENDPOINT =
  "/api/services/app/DatasetTransformation/GetProcessedVersion";
const DATASET_TRANSFORMATION_TRANSFORM_ENDPOINT =
  "/api/services/app/DatasetTransformation/Transform";

const buildUploadFormData = ({
  name,
  description,
  file,
}: UploadRawDatasetPayload): FormData => {
  const formData = new FormData();

  formData.append("name", name);

  if (description?.trim()) {
    formData.append("description", description.trim());
  }

  formData.append("file", file);

  return formData;
};

export const uploadRawDataset = async (
  payload: UploadRawDatasetPayload,
): Promise<UploadedRawDatasetResult> => {
  const response = await axiosInstance.post<UploadedRawDatasetResult>(
    DATASET_UPLOAD_ENDPOINT,
    buildUploadFormData(payload),
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  );

  return response.data;
};

export const getDatasets = async (
  filters: DatasetCatalogFilters,
): Promise<AbpPagedResult<DatasetListItem>> => {
  const response = await axiosInstance.get<
    AbpApiResponse<AbpPagedResult<DatasetListItem>>
  >(DATASET_GET_ALL_ENDPOINT, {
    params: {
      keyword: filters.keyword || undefined,
      status: filters.status,
      skipCount: (filters.page - 1) * filters.pageSize,
      maxResultCount: filters.pageSize,
    },
  });

  return response.data.result;
};

export const getDatasetDetails = async (
  datasetId: number,
  selectedVersionId?: number,
): Promise<DatasetDetails> => {
  const response = await axiosInstance.get<AbpApiResponse<DatasetDetails>>(
    DATASET_GET_DETAILS_ENDPOINT,
    {
      params: {
        datasetId,
        selectedVersionId,
      },
    },
  );

  return response.data.result;
};

export const getDatasetProfile = async (
  datasetVersionId: number,
): Promise<DatasetProfileSummary> => {
  const response = await axiosInstance.get<AbpApiResponse<DatasetProfileSummary>>(
    DATASET_PROFILE_GET_ENDPOINT,
    {
      params: {
        id: datasetVersionId,
      },
    },
  );

  return response.data.result;
};

export const getDatasetProfileColumns = async (
  request: DatasetProfileColumnsRequest,
): Promise<AbpPagedResult<DatasetColumnInsight>> => {
  const response = await axiosInstance.get<
    AbpApiResponse<AbpPagedResult<DatasetColumnInsight>>
  >(DATASET_PROFILE_GET_COLUMNS_ENDPOINT, {
    params: {
      datasetVersionId: request.datasetVersionId,
      skipCount: (request.page - 1) * request.pageSize,
      maxResultCount: request.pageSize,
      hasAnomalies: request.hasAnomalies,
      inferredDataType: request.inferredDataType || undefined,
    },
  });

  return response.data.result;
};

export const getDatasetTransformationHistory = async (
  datasetId: number,
): Promise<DatasetTransformationHistory> => {
  const response = await axiosInstance.get<
    AbpApiResponse<DatasetTransformationHistory>
  >(DATASET_TRANSFORMATION_HISTORY_ENDPOINT, {
    params: {
      id: datasetId,
    },
  });

  return response.data.result;
};

export const getProcessedDatasetVersion = async (
  datasetVersionId: number,
): Promise<ProcessedDatasetVersion> => {
  const response = await axiosInstance.get<
    AbpApiResponse<ProcessedDatasetVersion>
  >(DATASET_TRANSFORMATION_PROCESSED_VERSION_ENDPOINT, {
    params: {
      id: datasetVersionId,
    },
  });

  return response.data.result;
};

export const transformDatasetVersion = async (
  request: TransformDatasetVersionRequest,
): Promise<TransformDatasetVersionResult> => {
  const response = await axiosInstance.post<
    AbpApiResponse<TransformDatasetVersionResult>
  >(DATASET_TRANSFORMATION_TRANSFORM_ENDPOINT, request);

  return response.data.result;
};
