import type {
  AnalyticsExport,
  DatasetAnalyticsSummary,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  InsightRecord,
  ReportRecord,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";
import type { IAnalyticsDashboardStateContext } from "./context";

type AnalyticsDashboardStatePatch = Partial<IAnalyticsDashboardStateContext>;

export enum AnalyticsDashboardActionEnums {
  getRecentDatasetsPending = "ANALYTICS_DASHBOARD_GET_RECENT_DATASETS_PENDING",
  getRecentDatasetsSuccess = "ANALYTICS_DASHBOARD_GET_RECENT_DATASETS_SUCCESS",
  getRecentDatasetsError = "ANALYTICS_DASHBOARD_GET_RECENT_DATASETS_ERROR",
  loadContextPending = "ANALYTICS_DASHBOARD_LOAD_CONTEXT_PENDING",
  loadContextSuccess = "ANALYTICS_DASHBOARD_LOAD_CONTEXT_SUCCESS",
  loadContextError = "ANALYTICS_DASHBOARD_LOAD_CONTEXT_ERROR",
  generateReportPending = "ANALYTICS_DASHBOARD_GENERATE_REPORT_PENDING",
  generateReportSuccess = "ANALYTICS_DASHBOARD_GENERATE_REPORT_SUCCESS",
  generateReportError = "ANALYTICS_DASHBOARD_GENERATE_REPORT_ERROR",
  exportPdfPending = "ANALYTICS_DASHBOARD_EXPORT_PDF_PENDING",
  exportPdfSuccess = "ANALYTICS_DASHBOARD_EXPORT_PDF_SUCCESS",
  exportPdfError = "ANALYTICS_DASHBOARD_EXPORT_PDF_ERROR",
  exportCsvPending = "ANALYTICS_DASHBOARD_EXPORT_CSV_PENDING",
  exportCsvSuccess = "ANALYTICS_DASHBOARD_EXPORT_CSV_SUCCESS",
  exportCsvError = "ANALYTICS_DASHBOARD_EXPORT_CSV_ERROR",
  clearActionFeedback = "ANALYTICS_DASHBOARD_CLEAR_ACTION_FEEDBACK",
}

interface AnalyticsDashboardAction {
  type: AnalyticsDashboardActionEnums;
  payload: AnalyticsDashboardStatePatch;
}

export const getRecentDatasetsPending = (): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.getRecentDatasetsPending,
  payload: {
    isLoadingDatasets: true,
    datasetErrorMessage: undefined,
  },
});

export const getRecentDatasetsSuccess = (
  recentDatasets: DatasetListItem[],
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.getRecentDatasetsSuccess,
  payload: {
    isLoadingDatasets: false,
    datasetErrorMessage: undefined,
    recentDatasets,
  },
});

export const getRecentDatasetsError = (
  errorMessage: string,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.getRecentDatasetsError,
  payload: {
    isLoadingDatasets: false,
    datasetErrorMessage: errorMessage,
    recentDatasets: [],
  },
});

export const loadContextPending = ({
  datasetId,
  datasetVersionId,
}: {
  datasetId: number;
  datasetVersionId?: number;
}): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.loadContextPending,
  payload: {
    selectedDatasetId: datasetId,
    selectedDatasetVersionId: datasetVersionId,
    isLoadingContext: true,
    isLoadingArtifacts: true,
    contextErrorMessage: undefined,
    artifactsErrorMessage: undefined,
  },
});

export const loadContextSuccess = ({
  datasetDetails,
  analyticsSummary,
  recentInsights,
  recentReports,
  recentExports,
  selectedDatasetId,
  selectedDatasetVersionId,
}: {
  datasetDetails: DatasetDetails;
  analyticsSummary: DatasetAnalyticsSummary | null;
  recentInsights: InsightRecord[];
  recentReports: ReportRecord[];
  recentExports: AnalyticsExport[];
  selectedDatasetId: number;
  selectedDatasetVersionId?: number;
}): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.loadContextSuccess,
  payload: {
    selectedDatasetId,
    selectedDatasetVersionId,
    selectedDatasetDetails: datasetDetails,
    analyticsSummary,
    recentInsights,
    recentReports,
    recentExports,
    isLoadingContext: false,
    isLoadingArtifacts: false,
    contextErrorMessage: undefined,
    artifactsErrorMessage: undefined,
  },
});

export const loadContextError = ({
  datasetId,
  datasetVersionId,
  errorMessage,
}: {
  datasetId: number;
  datasetVersionId?: number;
  errorMessage: string;
}): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.loadContextError,
  payload: {
    selectedDatasetId: datasetId,
    selectedDatasetVersionId: datasetVersionId,
    selectedDatasetDetails: null,
    analyticsSummary: null,
    recentInsights: [],
    recentReports: [],
    recentExports: [],
    isLoadingContext: false,
    isLoadingArtifacts: false,
    contextErrorMessage: errorMessage,
    artifactsErrorMessage: undefined,
  },
});

export const generateReportPending = (): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.generateReportPending,
  payload: {
    isGeneratingReport: true,
    actionErrorMessage: undefined,
    lastGeneratedReport: null,
  },
});

export const generateReportSuccess = (
  report: GeneratedDatasetReportResult,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.generateReportSuccess,
  payload: {
    isGeneratingReport: false,
    actionErrorMessage: undefined,
    lastGeneratedReport: report,
  },
});

export const generateReportError = (
  errorMessage: string,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.generateReportError,
  payload: {
    isGeneratingReport: false,
    actionErrorMessage: errorMessage,
  },
});

export const exportPdfPending = (): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportPdfPending,
  payload: {
    isExportingPdf: true,
    actionErrorMessage: undefined,
    lastGeneratedExport: null,
  },
});

export const exportPdfSuccess = (
  exportResult: GeneratedAnalyticsExportResult,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportPdfSuccess,
  payload: {
    isExportingPdf: false,
    actionErrorMessage: undefined,
    lastGeneratedExport: exportResult,
  },
});

export const exportPdfError = (
  errorMessage: string,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportPdfError,
  payload: {
    isExportingPdf: false,
    actionErrorMessage: errorMessage,
  },
});

export const exportCsvPending = (): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportCsvPending,
  payload: {
    isExportingCsv: true,
    actionErrorMessage: undefined,
    lastGeneratedExport: null,
  },
});

export const exportCsvSuccess = (
  exportResult: GeneratedAnalyticsExportResult,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportCsvSuccess,
  payload: {
    isExportingCsv: false,
    actionErrorMessage: undefined,
    lastGeneratedExport: exportResult,
  },
});

export const exportCsvError = (
  errorMessage: string,
): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.exportCsvError,
  payload: {
    isExportingCsv: false,
    actionErrorMessage: errorMessage,
  },
});

export const clearActionFeedback = (): AnalyticsDashboardAction => ({
  type: AnalyticsDashboardActionEnums.clearActionFeedback,
  payload: {
    actionErrorMessage: undefined,
    lastGeneratedReport: null,
    lastGeneratedExport: null,
  },
});

export type AnalyticsDashboardReducerAction = AnalyticsDashboardAction;
