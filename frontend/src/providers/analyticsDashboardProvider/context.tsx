import { createContext } from "react";
import type {
  AnalyticsExport,
  DatasetAnalyticsSummary,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  InsightRecord,
  ReportRecord,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";

export interface IAnalyticsDashboardStateContext {
  recentDatasets: DatasetListItem[];
  selectedDatasetId?: number;
  selectedDatasetVersionId?: number;
  selectedDatasetDetails?: DatasetDetails | null;
  analyticsSummary?: DatasetAnalyticsSummary | null;
  recentInsights: InsightRecord[];
  recentReports: ReportRecord[];
  recentExports: AnalyticsExport[];
  isLoadingDatasets: boolean;
  isLoadingContext: boolean;
  isLoadingArtifacts: boolean;
  isGeneratingReport: boolean;
  isExportingPdf: boolean;
  isExportingCsv: boolean;
  datasetErrorMessage?: string;
  contextErrorMessage?: string;
  artifactsErrorMessage?: string;
  actionErrorMessage?: string;
  lastGeneratedReport?: GeneratedDatasetReportResult | null;
  lastGeneratedExport?: GeneratedAnalyticsExportResult | null;
}

export interface IAnalyticsDashboardActionContext {
  getRecentDatasets: () => Promise<void>;
  loadDashboardContext: (
    datasetId: number,
    datasetVersionId?: number,
  ) => Promise<void>;
  refreshDashboard: () => Promise<void>;
  generateReport: () => Promise<void>;
  exportReportPdf: (reportRecordId?: number) => Promise<void>;
  exportInsightsCsv: (reportRecordId?: number) => Promise<void>;
  clearDashboardActionFeedback: () => void;
}

export const INITIAL_STATE: IAnalyticsDashboardStateContext = {
  recentDatasets: [],
  selectedDatasetDetails: null,
  analyticsSummary: null,
  recentInsights: [],
  recentReports: [],
  recentExports: [],
  isLoadingDatasets: false,
  isLoadingContext: false,
  isLoadingArtifacts: false,
  isGeneratingReport: false,
  isExportingPdf: false,
  isExportingCsv: false,
  lastGeneratedReport: null,
  lastGeneratedExport: null,
};

export const AnalyticsDashboardStateContext =
  createContext<IAnalyticsDashboardStateContext | undefined>(undefined);

export const AnalyticsDashboardActionContext =
  createContext<IAnalyticsDashboardActionContext | undefined>(undefined);
