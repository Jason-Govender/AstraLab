"use strict";

import type {
    DatasetFormat,
    DatasetStatus,
    DatasetTransformationType,
    DatasetVersionStatus,
    DatasetVersionType,
} from "@/types/datasets";
import type { MlExperimentStatus, MlTaskType } from "@/types/ml";

export enum InsightType {
    Summary = 1,
    DataQuality = 2,
    Pattern = 3,
    Recommendation = 4,
    Experiment = 5,
}

export enum InsightSourceType {
    AiGenerated = 1,
    SystemGenerated = 2,
}

export enum ReportFormat {
    PlainText = 1,
    Markdown = 2,
    Html = 3,
    Json = 4,
}

export enum ReportSourceType {
    AiGenerated = 1,
    SystemGenerated = 2,
}

export enum AnalyticsExportType {
    Document = 1,
    Spreadsheet = 2,
    Presentation = 3,
    DataPackage = 4,
}

export enum AnalyticsNarrativeStatus {
    Generated = 1,
    Unavailable = 2,
    Failed = 3,
}

export interface DatasetQualityColumnHighlight {
    datasetColumnId: number;
    name: string;
    dataType?: string | null;
    nullCount: number;
    nullPercentage: number;
    distinctCount?: number | null;
    hasAnomalies: boolean;
    anomalyCount: number;
    anomalyPercentage: number;
    mean?: number | null;
    min?: number | null;
    max?: number | null;
}

export interface DatasetQualityHighlights {
    hasProfile: boolean;
    datasetProfileId?: number | null;
    rowCount?: number | null;
    duplicateRowCount?: number | null;
    dataHealthScore?: number | null;
    totalNullCount?: number | null;
    overallNullPercentage?: number | null;
    totalAnomalyCount?: number | null;
    overallAnomalyPercentage?: number | null;
    highRiskColumns: DatasetQualityColumnHighlight[];
}

export interface TransformationOutcomeSummary {
    datasetTransformationId: number;
    transformationType: DatasetTransformationType;
    sourceDatasetVersionId: number;
    resultDatasetVersionId?: number | null;
    executionOrder: number;
    executedAt: string;
    summaryPreview?: string | null;
}

export interface AiFindingPreview {
    source: string;
    title: string;
    contentPreview: string;
    creationTime: string;
}

export interface AiFindingsSummary {
    storedAiResponseCount: number;
    storedInsightRecordCount: number;
    hasAutomaticInsight: boolean;
    latestAutomaticInsightPreview?: string | null;
    latestManualInsightPreview?: string | null;
    latestRecommendationPreview?: string | null;
    latestFindingTime?: string | null;
    recentFindings: AiFindingPreview[];
}

export interface AnalyticsMlMetric {
    metricName: string;
    metricValue: number;
}

export interface AnalyticsMlFeatureImportance {
    datasetColumnId: number;
    columnName?: string | null;
    importanceScore: number;
    rank: number;
}

export interface MlExperimentHighlights {
    hasCompletedExperiment: boolean;
    mlExperimentId?: number | null;
    status?: MlExperimentStatus | null;
    taskType?: MlTaskType | null;
    algorithmKey?: string | null;
    modelType?: string | null;
    targetColumnName?: string | null;
    featureCount: number;
    featureNames: string[];
    metrics: AnalyticsMlMetric[];
    primaryMetricName?: string | null;
    primaryMetricValue?: number | null;
    topFeatureImportances: AnalyticsMlFeatureImportance[];
    warnings: string[];
    executedAt?: string | null;
    hasModelOutput: boolean;
}

export interface AnalyticsDashboardSummary {
    datasetVersionId: number;
    dataHealthScore?: number | null;
    rowCount?: number | null;
    columnCount?: number | null;
    highRiskColumnCount: number;
    recentTransformationCount: number;
    storedInsightCount: number;
    storedAiResponseCount: number;
    hasAutomaticAiInsight: boolean;
    hasCompletedMlExperiment: boolean;
    primaryMetricName?: string | null;
    primaryMetricValue?: number | null;
    mlWarningCount: number;
}

export interface AnalyticsNarrative {
    status: AnalyticsNarrativeStatus;
    content?: string | null;
    failureMessage?: string | null;
}

export interface DatasetAnalyticsSummary {
    datasetId: number;
    datasetName: string;
    sourceFormat: DatasetFormat;
    datasetStatus: DatasetStatus;
    datasetVersionId: number;
    versionNumber: number;
    versionType: DatasetVersionType;
    versionStatus: DatasetVersionStatus;
    columnCount?: number | null;
    sizeBytes: number;
    qualityHighlights: DatasetQualityHighlights;
    transformationOutcomes: TransformationOutcomeSummary[];
    aiFindings: AiFindingsSummary;
    mlExperimentHighlights: MlExperimentHighlights;
    dashboardSummary: AnalyticsDashboardSummary;
    narrative: AnalyticsNarrative;
}

export interface InsightRecord {
    id: number;
    datasetVersionId: number;
    datasetProfileId?: number | null;
    mlExperimentId?: number | null;
    aiResponseId?: number | null;
    title: string;
    content: string;
    insightType: InsightType;
    insightSourceType: InsightSourceType;
    metadataJson?: string | null;
    creationTime: string;
}

export interface ReportRecord {
    id: number;
    datasetVersionId: number;
    datasetProfileId?: number | null;
    mlExperimentId?: number | null;
    aiResponseId?: number | null;
    title: string;
    summary?: string | null;
    content: string;
    reportFormat: ReportFormat;
    reportSourceType: ReportSourceType;
    metadataJson?: string | null;
    creationTime: string;
}

export interface AnalyticsExport {
    id: number;
    datasetVersionId: number;
    mlExperimentId?: number | null;
    insightRecordId?: number | null;
    reportRecordId?: number | null;
    exportType: AnalyticsExportType;
    displayName: string;
    storageProvider: string;
    storageKey: string;
    contentType?: string | null;
    sizeBytes?: number | null;
    checksumSha256?: string | null;
    metadataJson?: string | null;
    creationTime: string;
    downloadUrl?: string | null;
}

export interface GetInsightsRequest {
    datasetVersionId: number;
    datasetProfileId?: number;
    mlExperimentId?: number;
    insightType?: InsightType;
    insightSourceType?: InsightSourceType;
    page: number;
    pageSize: number;
}

export interface GetReportsRequest {
    datasetVersionId: number;
    datasetProfileId?: number;
    mlExperimentId?: number;
    reportFormat?: ReportFormat;
    reportSourceType?: ReportSourceType;
    page: number;
    pageSize: number;
}

export interface GetAnalyticsExportsRequest {
    datasetVersionId: number;
    mlExperimentId?: number;
    insightRecordId?: number;
    reportRecordId?: number;
    exportType?: AnalyticsExportType;
    page: number;
    pageSize: number;
}

export interface GeneratedDatasetReportResult {
    datasetVersionId: number;
    mlExperimentId?: number | null;
    report: ReportRecord;
}

export interface GeneratedAnalyticsExportResult {
    datasetVersionId: number;
    mlExperimentId?: number | null;
    report?: ReportRecord | null;
    export: AnalyticsExport;
}
