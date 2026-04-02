import type {
  AnalyticsExport,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  ReportRecord,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";
import type { IReportsWorkspaceStateContext } from "./context";

type ReportsWorkspaceStatePatch = Partial<IReportsWorkspaceStateContext>;

export enum ReportsWorkspaceActionEnums {
  getRecentDatasetsPending = "REPORTS_WORKSPACE_GET_RECENT_DATASETS_PENDING",
  getRecentDatasetsSuccess = "REPORTS_WORKSPACE_GET_RECENT_DATASETS_SUCCESS",
  getRecentDatasetsError = "REPORTS_WORKSPACE_GET_RECENT_DATASETS_ERROR",
  loadWorkspacePending = "REPORTS_WORKSPACE_LOAD_PENDING",
  loadWorkspaceSuccess = "REPORTS_WORKSPACE_LOAD_SUCCESS",
  loadWorkspaceError = "REPORTS_WORKSPACE_LOAD_ERROR",
  selectReport = "REPORTS_WORKSPACE_SELECT_REPORT",
  generateReportPending = "REPORTS_WORKSPACE_GENERATE_REPORT_PENDING",
  generateReportSuccess = "REPORTS_WORKSPACE_GENERATE_REPORT_SUCCESS",
  generateReportError = "REPORTS_WORKSPACE_GENERATE_REPORT_ERROR",
  exportPdfPending = "REPORTS_WORKSPACE_EXPORT_PDF_PENDING",
  exportPdfSuccess = "REPORTS_WORKSPACE_EXPORT_PDF_SUCCESS",
  exportPdfError = "REPORTS_WORKSPACE_EXPORT_PDF_ERROR",
  exportCsvPending = "REPORTS_WORKSPACE_EXPORT_CSV_PENDING",
  exportCsvSuccess = "REPORTS_WORKSPACE_EXPORT_CSV_SUCCESS",
  exportCsvError = "REPORTS_WORKSPACE_EXPORT_CSV_ERROR",
  clearActionFeedback = "REPORTS_WORKSPACE_CLEAR_ACTION_FEEDBACK",
}

interface ReportsWorkspaceReducerAction {
  type: ReportsWorkspaceActionEnums;
  payload: ReportsWorkspaceStatePatch;
}

export const getRecentDatasetsPending = (): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.getRecentDatasetsPending,
  payload: {
    isLoadingDatasets: true,
    datasetErrorMessage: undefined,
  },
});

export const getRecentDatasetsSuccess = (
  recentDatasets: DatasetListItem[],
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.getRecentDatasetsSuccess,
  payload: {
    isLoadingDatasets: false,
    datasetErrorMessage: undefined,
    recentDatasets,
  },
});

export const getRecentDatasetsError = (
  errorMessage: string,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.getRecentDatasetsError,
  payload: {
    isLoadingDatasets: false,
    datasetErrorMessage: errorMessage,
    recentDatasets: [],
  },
});

export const loadWorkspacePending = ({
  datasetId,
  datasetVersionId,
}: {
  datasetId: number;
  datasetVersionId?: number;
}): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.loadWorkspacePending,
  payload: {
    selectedDatasetId: datasetId,
    selectedDatasetVersionId: datasetVersionId,
    isLoadingContext: true,
    contextErrorMessage: undefined,
  },
});

export const loadWorkspaceSuccess = ({
  datasetDetails,
  reports,
  exports,
  selectedDatasetId,
  selectedDatasetVersionId,
  selectedReportId,
}: {
  datasetDetails: DatasetDetails;
  reports: ReportRecord[];
  exports: AnalyticsExport[];
  selectedDatasetId: number;
  selectedDatasetVersionId?: number;
  selectedReportId?: number;
}): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.loadWorkspaceSuccess,
  payload: {
    selectedDatasetId,
    selectedDatasetVersionId,
    selectedDatasetDetails: datasetDetails,
    reports,
    exports,
    selectedReportId,
    isLoadingContext: false,
    contextErrorMessage: undefined,
  },
});

export const loadWorkspaceError = ({
  datasetId,
  datasetVersionId,
  errorMessage,
}: {
  datasetId: number;
  datasetVersionId?: number;
  errorMessage: string;
}): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.loadWorkspaceError,
  payload: {
    selectedDatasetId: datasetId,
    selectedDatasetVersionId: datasetVersionId,
    selectedDatasetDetails: null,
    reports: [],
    exports: [],
    selectedReportId: undefined,
    isLoadingContext: false,
    contextErrorMessage: errorMessage,
  },
});

export const selectReport = (
  reportId?: number,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.selectReport,
  payload: {
    selectedReportId: reportId,
  },
});

export const generateReportPending = (): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.generateReportPending,
  payload: {
    isGeneratingReport: true,
    actionErrorMessage: undefined,
    lastGeneratedReport: null,
  },
});

export const generateReportSuccess = (
  reportResult: GeneratedDatasetReportResult,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.generateReportSuccess,
  payload: {
    isGeneratingReport: false,
    actionErrorMessage: undefined,
    lastGeneratedReport: reportResult,
    selectedReportId: reportResult.report.id,
  },
});

export const generateReportError = (
  errorMessage: string,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.generateReportError,
  payload: {
    isGeneratingReport: false,
    actionErrorMessage: errorMessage,
  },
});

export const exportPdfPending = (): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportPdfPending,
  payload: {
    isExportingPdf: true,
    actionErrorMessage: undefined,
    lastGeneratedExport: null,
  },
});

export const exportPdfSuccess = (
  exportResult: GeneratedAnalyticsExportResult,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportPdfSuccess,
  payload: {
    isExportingPdf: false,
    actionErrorMessage: undefined,
    lastGeneratedExport: exportResult,
  },
});

export const exportPdfError = (
  errorMessage: string,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportPdfError,
  payload: {
    isExportingPdf: false,
    actionErrorMessage: errorMessage,
  },
});

export const exportCsvPending = (): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportCsvPending,
  payload: {
    isExportingCsv: true,
    actionErrorMessage: undefined,
    lastGeneratedExport: null,
  },
});

export const exportCsvSuccess = (
  exportResult: GeneratedAnalyticsExportResult,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportCsvSuccess,
  payload: {
    isExportingCsv: false,
    actionErrorMessage: undefined,
    lastGeneratedExport: exportResult,
  },
});

export const exportCsvError = (
  errorMessage: string,
): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.exportCsvError,
  payload: {
    isExportingCsv: false,
    actionErrorMessage: errorMessage,
  },
});

export const clearActionFeedback = (): ReportsWorkspaceReducerAction => ({
  type: ReportsWorkspaceActionEnums.clearActionFeedback,
  payload: {
    actionErrorMessage: undefined,
    lastGeneratedReport: null,
    lastGeneratedExport: null,
  },
});

export type ReportsWorkspaceAction = ReportsWorkspaceReducerAction;
