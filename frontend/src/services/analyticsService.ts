"use strict";

import type { AbpApiResponse, AbpPagedResult } from "@/types/api";
import type {
    AnalyticsDashboardSummary,
    AnalyticsExport,
    DatasetAnalyticsSummary,
    GeneratedAnalyticsExportResult,
    GeneratedDatasetReportResult,
    GetAnalyticsExportsRequest,
    GetInsightsRequest,
    GetReportsRequest,
    InsightRecord,
    ReportRecord,
} from "@/types/analytics";
import { axiosInstance } from "@/utils/axiosInstance";

const ANALYTICS_GET_DATASET_SUMMARY_ENDPOINT =
    "/api/services/app/Analytics/GetDatasetAnalyticsSummary";
const ANALYTICS_GET_DASHBOARD_SUMMARY_ENDPOINT =
    "/api/services/app/Analytics/GetDatasetDashboardSummary";
const ANALYTICS_GET_INSIGHTS_ENDPOINT =
    "/api/services/app/Analytics/GetInsights";
const ANALYTICS_GET_REPORTS_ENDPOINT =
    "/api/services/app/Analytics/GetReports";
const ANALYTICS_GET_EXPORTS_ENDPOINT =
    "/api/services/app/Analytics/GetExports";
const ANALYTICS_GENERATE_REPORT_ENDPOINT =
    "/api/services/app/Analytics/GenerateDatasetReport";
const ANALYTICS_EXPORT_REPORT_PDF_ENDPOINT =
    "/api/services/app/Analytics/ExportDatasetReportPdf";
const ANALYTICS_EXPORT_INSIGHTS_CSV_ENDPOINT =
    "/api/services/app/Analytics/ExportDatasetInsightsCsv";

export const getDatasetAnalyticsSummary = async (
    datasetVersionId: number,
): Promise<DatasetAnalyticsSummary> => {
    const response = await axiosInstance.get<AbpApiResponse<DatasetAnalyticsSummary>>(
        ANALYTICS_GET_DATASET_SUMMARY_ENDPOINT,
        {
            params: {
                id: datasetVersionId,
            },
        },
    );

    return response.data.result;
};

export const getDatasetDashboardSummary = async (
    datasetVersionId: number,
): Promise<AnalyticsDashboardSummary> => {
    const response = await axiosInstance.get<
        AbpApiResponse<AnalyticsDashboardSummary>
    >(ANALYTICS_GET_DASHBOARD_SUMMARY_ENDPOINT, {
        params: {
            id: datasetVersionId,
        },
    });

    return response.data.result;
};

export const getInsights = async (
    request: GetInsightsRequest,
): Promise<AbpPagedResult<InsightRecord>> => {
    const response = await axiosInstance.get<
        AbpApiResponse<AbpPagedResult<InsightRecord>>
    >(ANALYTICS_GET_INSIGHTS_ENDPOINT, {
        params: {
            datasetVersionId: request.datasetVersionId,
            datasetProfileId: request.datasetProfileId,
            mlExperimentId: request.mlExperimentId,
            insightType: request.insightType,
            insightSourceType: request.insightSourceType,
            skipCount: (request.page - 1) * request.pageSize,
            maxResultCount: request.pageSize,
        },
    });

    return response.data.result;
};

export const getReports = async (
    request: GetReportsRequest,
): Promise<AbpPagedResult<ReportRecord>> => {
    const response = await axiosInstance.get<
        AbpApiResponse<AbpPagedResult<ReportRecord>>
    >(ANALYTICS_GET_REPORTS_ENDPOINT, {
        params: {
            datasetVersionId: request.datasetVersionId,
            datasetProfileId: request.datasetProfileId,
            mlExperimentId: request.mlExperimentId,
            reportFormat: request.reportFormat,
            reportSourceType: request.reportSourceType,
            skipCount: (request.page - 1) * request.pageSize,
            maxResultCount: request.pageSize,
        },
    });

    return response.data.result;
};

export const getExports = async (
    request: GetAnalyticsExportsRequest,
): Promise<AbpPagedResult<AnalyticsExport>> => {
    const response = await axiosInstance.get<
        AbpApiResponse<AbpPagedResult<AnalyticsExport>>
    >(ANALYTICS_GET_EXPORTS_ENDPOINT, {
        params: {
            datasetVersionId: request.datasetVersionId,
            mlExperimentId: request.mlExperimentId,
            insightRecordId: request.insightRecordId,
            reportRecordId: request.reportRecordId,
            exportType: request.exportType,
            skipCount: (request.page - 1) * request.pageSize,
            maxResultCount: request.pageSize,
        },
    });

    return response.data.result;
};

export const generateDatasetReport = async (
    datasetVersionId: number,
): Promise<GeneratedDatasetReportResult> => {
    const response = await axiosInstance.post<
        AbpApiResponse<GeneratedDatasetReportResult>
    >(ANALYTICS_GENERATE_REPORT_ENDPOINT, {
        datasetVersionId,
    });

    return response.data.result;
};

export const exportDatasetReportPdf = async ({
    datasetVersionId,
    reportRecordId,
}: {
    datasetVersionId: number;
    reportRecordId?: number;
}): Promise<GeneratedAnalyticsExportResult> => {
    const response = await axiosInstance.post<
        AbpApiResponse<GeneratedAnalyticsExportResult>
    >(ANALYTICS_EXPORT_REPORT_PDF_ENDPOINT, {
        datasetVersionId,
        reportRecordId,
    });

    return response.data.result;
};

export const exportDatasetInsightsCsv = async ({
    datasetVersionId,
    reportRecordId,
}: {
    datasetVersionId: number;
    reportRecordId?: number;
}): Promise<GeneratedAnalyticsExportResult> => {
    const response = await axiosInstance.post<
        AbpApiResponse<GeneratedAnalyticsExportResult>
    >(ANALYTICS_EXPORT_INSIGHTS_CSV_ENDPOINT, {
        datasetVersionId,
        reportRecordId,
    });

    return response.data.result;
};
