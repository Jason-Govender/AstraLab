import { createContext } from "react";
import type {
  AnalyticsExport,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  ReportRecord,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";

export interface IReportsWorkspaceStateContext {
  recentDatasets: DatasetListItem[];
  selectedDatasetId?: number;
  selectedDatasetVersionId?: number;
  selectedDatasetDetails?: DatasetDetails | null;
  reports: ReportRecord[];
  exports: AnalyticsExport[];
  selectedReportId?: number;
  isLoadingDatasets: boolean;
  isLoadingContext: boolean;
  isGeneratingReport: boolean;
  isExportingPdf: boolean;
  isExportingCsv: boolean;
  datasetErrorMessage?: string;
  contextErrorMessage?: string;
  actionErrorMessage?: string;
  lastGeneratedReport?: GeneratedDatasetReportResult | null;
  lastGeneratedExport?: GeneratedAnalyticsExportResult | null;
}

export interface IReportsWorkspaceActionContext {
  getRecentDatasets: () => Promise<void>;
  loadReportsWorkspace: (
    datasetId: number,
    datasetVersionId?: number,
    preferredReportId?: number,
  ) => Promise<void>;
  refreshReportsWorkspace: () => Promise<void>;
  selectReport: (reportId?: number) => void;
  generateReport: () => Promise<void>;
  exportReportPdf: (reportRecordId?: number) => Promise<void>;
  exportInsightsCsv: (reportRecordId?: number) => Promise<void>;
  clearReportsActionFeedback: () => void;
}

export const INITIAL_STATE: IReportsWorkspaceStateContext = {
  recentDatasets: [],
  selectedDatasetDetails: null,
  reports: [],
  exports: [],
  isLoadingDatasets: false,
  isLoadingContext: false,
  isGeneratingReport: false,
  isExportingPdf: false,
  isExportingCsv: false,
  lastGeneratedReport: null,
  lastGeneratedExport: null,
};

export const ReportsWorkspaceStateContext =
  createContext<IReportsWorkspaceStateContext | undefined>(undefined);

export const ReportsWorkspaceActionContext =
  createContext<IReportsWorkspaceActionContext | undefined>(undefined);
