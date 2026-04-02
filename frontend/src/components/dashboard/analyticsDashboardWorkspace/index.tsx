"use client";

import { Alert, Button, Card, Empty, Progress, Select, Tag, Typography } from "antd";
import { useRouter } from "next/navigation";
import type {
  AnalyticsExport,
  DatasetAnalyticsSummary,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  InsightRecord,
  ReportRecord,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";
import { DatasetAiAssistantLauncherButton } from "@/components/datasets/ai/datasetAiAssistantLauncherButton";
import { DatasetVersionType } from "@/types/datasets";
import {
  formatBytes,
  formatDateTime,
  formatNumber,
  formatPercentage,
  formatRelativeTime,
  getDataHealthScoreTone,
  getDatasetFormatLabel,
  getDatasetStatusColor,
  getDatasetStatusLabel,
  getDatasetVersionStatusColor,
  getDatasetVersionStatusLabel,
  getDatasetVersionTypeLabel,
} from "@/utils/datasets";
import {
  buildMlWorkspaceHref,
  getMlAlgorithmLabel,
  getMlExperimentStatusColor,
  getMlExperimentStatusLabel,
  getMlTaskTypeLabel,
} from "@/utils/ml";
import { getDatasetTransformationTypeLabel } from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface AnalyticsDashboardWorkspaceProps {
  recentDatasets: DatasetListItem[];
  selectedDatasetId?: number;
  selectedDatasetVersionId?: number;
  selectedDatasetDetails?: DatasetDetails | null;
  analyticsSummary?: DatasetAnalyticsSummary | null;
  recentInsights: InsightRecord[];
  recentReports: ReportRecord[];
  recentExports: AnalyticsExport[];
  isLoadingDatasets?: boolean;
  isLoadingContext?: boolean;
  isGeneratingReport?: boolean;
  isExportingPdf?: boolean;
  isExportingCsv?: boolean;
  datasetErrorMessage?: string;
  contextErrorMessage?: string;
  actionErrorMessage?: string;
  lastGeneratedReport?: GeneratedDatasetReportResult | null;
  lastGeneratedExport?: GeneratedAnalyticsExportResult | null;
  onSelectDataset: (datasetId?: number) => void;
  onSelectVersion: (datasetVersionId?: number) => void;
  onRefresh: () => void;
  onGenerateReport: () => void;
  onExportPdf: (reportRecordId?: number) => void;
  onExportCsv: (reportRecordId?: number) => void;
  onClearFeedback: () => void;
}

const buildDatasetDetailsHref = (
  datasetId?: number,
  datasetVersionId?: number,
): string => {
  if (!datasetId) {
    return "/datasets";
  }

  if (!datasetVersionId) {
    return `/datasets/${datasetId}`;
  }

  return `/datasets/${datasetId}?versionId=${datasetVersionId}`;
};

const openDownload = (downloadUrl?: string | null) => {
  if (!downloadUrl || typeof window === "undefined") {
    return;
  }

  window.open(downloadUrl, "_blank", "noopener,noreferrer");
};

const getExportTypeLabel = (exportItem: AnalyticsExport): string => {
  if (exportItem.contentType?.includes("pdf")) {
    return "PDF";
  }

  if (exportItem.contentType?.includes("csv")) {
    return "CSV";
  }

  return exportItem.displayName.split(".").pop()?.toUpperCase() || "Export";
};

const renderEmptyCard = (description: string, className: string) => (
  <Card className={className}>
    <Empty
      image={Empty.PRESENTED_IMAGE_SIMPLE}
      description={description}
    />
  </Card>
);

export const AnalyticsDashboardWorkspace = ({
  recentDatasets,
  selectedDatasetId,
  selectedDatasetVersionId,
  selectedDatasetDetails,
  analyticsSummary,
  recentInsights,
  recentReports,
  recentExports,
  isLoadingDatasets = false,
  isLoadingContext = false,
  isGeneratingReport = false,
  isExportingPdf = false,
  isExportingCsv = false,
  datasetErrorMessage,
  contextErrorMessage,
  actionErrorMessage,
  lastGeneratedReport,
  lastGeneratedExport,
  onSelectDataset,
  onSelectVersion,
  onRefresh,
  onGenerateReport,
  onExportPdf,
  onExportCsv,
  onClearFeedback,
}: AnalyticsDashboardWorkspaceProps) => {
  const { styles } = useStyles();
  const router = useRouter();
  const latestReport = lastGeneratedReport?.report ?? recentReports[0];
  const selectedVersion = selectedDatasetDetails?.selectedVersion;
  const dashboardSummary = analyticsSummary?.dashboardSummary;
  const qualityHighlights = analyticsSummary?.qualityHighlights;
  const aiFindings = analyticsSummary?.aiFindings;
  const mlHighlights = analyticsSummary?.mlExperimentHighlights;
  const transformationOutcomes = analyticsSummary?.transformationOutcomes ?? [];

  const datasetOptions = recentDatasets.map((dataset) => ({
    label: dataset.name,
    value: dataset.id,
  }));

  const versionOptions =
    selectedDatasetDetails?.versions.map((version) => ({
      label: `v${version.versionNumber} · ${getDatasetVersionTypeLabel(version.versionType)}`,
      value: version.id,
    })) ?? [];

  return (
    <div className={styles.pageStack}>
      <Card loading={isLoadingContext && !selectedDatasetDetails} className={styles.contextCard}>
        <div className={styles.contextTopRow}>
          <div className={styles.contextHeadline}>
            <Text className={styles.eyebrow}>Executive Summary</Text>
            <Title level={2} className={styles.title}>
              {selectedDatasetDetails?.dataset.name || "Analytics dashboard"}
            </Title>
            <Paragraph className={styles.description}>
              Surface the most important profiling, transformation, AI, and ML signals for one dataset version and move quickly into deeper analysis or stakeholder outputs.
            </Paragraph>
            <div className={styles.metaRow}>
              {selectedDatasetDetails?.dataset ? (
                <>
                  <Tag color={getDatasetStatusColor(selectedDatasetDetails.dataset.status)}>
                    {getDatasetStatusLabel(selectedDatasetDetails.dataset.status)}
                  </Tag>
                  <Tag color="blue">
                    {getDatasetFormatLabel(selectedDatasetDetails.dataset.sourceFormat)}
                  </Tag>
                </>
              ) : null}
              {selectedVersion ? (
                <>
                  <Tag color={getDatasetVersionStatusColor(selectedVersion.status)}>
                    {getDatasetVersionStatusLabel(selectedVersion.status)}
                  </Tag>
                  <Tag color={selectedVersion.versionType === DatasetVersionType.Processed ? "purple" : "default"}>
                    {`v${selectedVersion.versionNumber} · ${getDatasetVersionTypeLabel(selectedVersion.versionType)}`}
                  </Tag>
                </>
              ) : null}
              {analyticsSummary ? (
                <Tag color="cyan">{formatBytes(analyticsSummary.sizeBytes)}</Tag>
              ) : null}
            </div>
          </div>

          <div className={styles.actionGroup}>
            <Button onClick={onRefresh}>Refresh</Button>
            <Button
              onClick={() =>
                router.push(buildDatasetDetailsHref(selectedDatasetId, selectedDatasetVersionId))
              }
              disabled={!selectedDatasetId}
            >
              Open details
            </Button>
            <DatasetAiAssistantLauncherButton
              datasetId={selectedDatasetId || 0}
              versionId={selectedDatasetVersionId}
              disabled={!selectedDatasetId}
            >
              Open assistant
            </DatasetAiAssistantLauncherButton>
            <Button
              onClick={() =>
                router.push(buildMlWorkspaceHref(selectedDatasetId, selectedDatasetVersionId))
              }
              disabled={!selectedDatasetId}
            >
              Open ML workspace
            </Button>
            <Button
              type="primary"
              loading={isGeneratingReport}
              onClick={onGenerateReport}
              disabled={!selectedDatasetVersionId}
            >
              Generate report
            </Button>
            <Button
              loading={isExportingPdf}
              onClick={() => onExportPdf(latestReport?.id)}
              disabled={!selectedDatasetVersionId}
            >
              Export PDF
            </Button>
            <Button
              loading={isExportingCsv}
              onClick={() => onExportCsv(latestReport?.id)}
              disabled={!selectedDatasetVersionId}
            >
              Export CSV
            </Button>
          </div>
        </div>

        <div className={styles.selectorGrid}>
          <div className={styles.selectorField}>
            <Text strong>Dataset</Text>
            <Select
              allowClear
              showSearch
              size="large"
              value={selectedDatasetId}
              options={datasetOptions}
              loading={isLoadingDatasets}
              onChange={onSelectDataset}
              className={styles.selectInput}
              optionFilterProp="label"
              placeholder="Select a dataset"
              notFoundContent={isLoadingDatasets ? "Loading datasets..." : "No datasets available."}
            />
          </div>

          <div className={styles.selectorField}>
            <Text strong>Version</Text>
            <Select
              allowClear
              size="large"
              value={selectedDatasetVersionId}
              options={versionOptions}
              onChange={onSelectVersion}
              className={styles.selectInput}
              placeholder={selectedDatasetId ? "Select a dataset version" : "Choose a dataset first"}
              disabled={!selectedDatasetId || versionOptions.length === 0}
            />
          </div>
        </div>

        {datasetErrorMessage ? (
          <Alert
            type="error"
            showIcon
            message="Unable to load recent datasets"
            description={datasetErrorMessage}
          />
        ) : null}

        {contextErrorMessage ? (
          <Alert
            type="error"
            showIcon
            message="Unable to load dashboard summary"
            description={contextErrorMessage}
          />
        ) : null}

        {actionErrorMessage ? (
          <Alert
            type="error"
            showIcon
            closable
            onClose={onClearFeedback}
            className={styles.feedbackAlert}
            message="Dashboard action failed"
            description={actionErrorMessage}
          />
        ) : null}

        {lastGeneratedReport ? (
          <Alert
            type="success"
            showIcon
            closable
            onClose={onClearFeedback}
            className={styles.feedbackAlert}
            message="Stakeholder report generated"
            description={`Saved report "${lastGeneratedReport.report.title}" for this dataset version.`}
          />
        ) : null}

        {lastGeneratedExport ? (
          <Alert
            type="success"
            showIcon
            closable
            onClose={onClearFeedback}
            className={styles.feedbackAlert}
            message="Export ready"
            description={`${lastGeneratedExport.export.displayName} is ready for download.`}
            action={
              lastGeneratedExport.export.downloadUrl ? (
                <Button size="small" type="primary" onClick={() => openDownload(lastGeneratedExport.export.downloadUrl)}>
                  Download
                </Button>
              ) : undefined
            }
          />
        ) : null}
      </Card>

      {!selectedDatasetId && recentDatasets.length === 0 && !isLoadingDatasets
        ? renderEmptyCard(
            "No datasets are available yet. Upload a dataset to start building executive summaries and reports.",
            styles.card,
          )
        : null}

      {selectedDatasetId &&
      selectedDatasetDetails &&
      selectedDatasetDetails.versions.length === 0 &&
      !isLoadingContext
        ? renderEmptyCard(
            "This dataset does not have any versions yet, so there is no analytics summary to display.",
            styles.card,
          )
        : null}

      {analyticsSummary ? (
        <>
          <div className={styles.heroGrid}>
            <Card className={`${styles.card} ${styles.heroCard}`}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Executive narrative
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    A concise stakeholder-ready overview generated from the unified analytics summary.
                  </Paragraph>
                </div>
                <Tag color={analyticsSummary.narrative.content ? "blue" : "default"}>
                  {analyticsSummary.narrative.content ? "Narrative ready" : "Narrative unavailable"}
                </Tag>
              </div>

              <Paragraph className={styles.heroText}>
                {analyticsSummary.narrative.content ||
                  analyticsSummary.narrative.failureMessage ||
                  "The deterministic dashboard summary is available even though a narrative was not generated for this version."}
              </Paragraph>
            </Card>

            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Summary metrics
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    The main workspace signals to scan before going deeper.
                  </Paragraph>
                </div>
              </div>

              <div className={styles.metricGrid}>
                {[
                  {
                    key: "health",
                    label: "Health score",
                    value:
                      dashboardSummary?.dataHealthScore !== null &&
                      dashboardSummary?.dataHealthScore !== undefined
                        ? formatNumber(dashboardSummary.dataHealthScore)
                        : "N/A",
                    subtext: qualityHighlights?.hasProfile
                      ? "Calculated from profiling"
                      : "Profiling not available",
                  },
                  {
                    key: "rows",
                    label: "Rows",
                    value:
                      dashboardSummary?.rowCount !== null &&
                      dashboardSummary?.rowCount !== undefined
                        ? formatNumber(dashboardSummary.rowCount, 0)
                        : "N/A",
                    subtext: "Profiled data volume",
                  },
                  {
                    key: "risks",
                    label: "High-risk columns",
                    value: formatNumber(dashboardSummary?.highRiskColumnCount ?? 0, 0),
                    subtext: "Prioritized quality risks",
                  },
                  {
                    key: "insights",
                    label: "Stored findings",
                    value: formatNumber(
                      (dashboardSummary?.storedAiResponseCount ?? 0) +
                        (dashboardSummary?.storedInsightCount ?? 0),
                      0,
                    ),
                    subtext: "AI and analytics outputs",
                  },
                ].map((metric) => (
                  <div key={metric.key} className={styles.metricCard}>
                    <span className={styles.metricLabel}>{metric.label}</span>
                    <span className={styles.metricValue}>{metric.value}</span>
                    {metric.subtext ? (
                      <span className={styles.metricSubtext}>{metric.subtext}</span>
                    ) : null}
                  </div>
                ))}
              </div>
            </Card>
          </div>

          <div className={styles.sectionGrid}>
            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Dataset quality
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    Profile-level quality signals, duplicate pressure, and the riskiest columns to address first.
                  </Paragraph>
                </div>
                {qualityHighlights?.dataHealthScore !== null &&
                qualityHighlights?.dataHealthScore !== undefined ? (
                  <Tag color={getDataHealthScoreTone(qualityHighlights.dataHealthScore)}>
                    Health {formatNumber(qualityHighlights.dataHealthScore)}
                  </Tag>
                ) : null}
              </div>

              {!qualityHighlights?.hasProfile ? (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="Profiling has not been generated for this version yet."
                />
              ) : (
                <>
                  <div className={styles.scoreWrap}>
                    <Progress
                      type="dashboard"
                      percent={Math.max(
                        0,
                        Math.min(
                          100,
                          Number(qualityHighlights.dataHealthScore ?? 0),
                        ),
                      )}
                      status={
                        Number(qualityHighlights.dataHealthScore ?? 0) >= 85
                          ? "success"
                          : Number(qualityHighlights.dataHealthScore ?? 0) >= 60
                            ? "active"
                            : "exception"
                      }
                    />
                    <div className={styles.scoreMeta}>
                      <Title level={3} className={styles.scoreValue}>
                        {formatNumber(qualityHighlights.dataHealthScore)}
                      </Title>
                      <Text type="secondary">
                        Duplicate rows {formatNumber(qualityHighlights.duplicateRowCount, 0)}
                      </Text>
                      <Text type="secondary">
                        Null rate {formatPercentage(qualityHighlights.overallNullPercentage)}
                      </Text>
                    </div>
                  </div>

                  <div className={styles.statGrid}>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Profiled rows</span>
                      <span className={styles.statValue}>
                        {formatNumber(qualityHighlights.rowCount, 0)}
                      </span>
                    </div>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Detected anomalies</span>
                      <span className={styles.statValue}>
                        {formatNumber(qualityHighlights.totalAnomalyCount, 0)}
                      </span>
                    </div>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Overall null rate</span>
                      <span className={styles.statValue}>
                        {formatPercentage(qualityHighlights.overallNullPercentage)}
                      </span>
                    </div>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Version columns</span>
                      <span className={styles.statValue}>
                        {formatNumber(analyticsSummary.columnCount, 0)}
                      </span>
                    </div>
                  </div>

                  {qualityHighlights.highRiskColumns.length > 0 ? (
                    <ul className={styles.plainList}>
                      {qualityHighlights.highRiskColumns.slice(0, 4).map((column) => (
                        <li key={column.datasetColumnId} className={styles.listItem}>
                          <div className={styles.listTitleRow}>
                            <div>
                              <div className={styles.listTitle}>{column.name}</div>
                              <div className={styles.listMeta}>
                                {column.dataType || "Unknown type"}
                              </div>
                            </div>
                            <div className={styles.tagRow}>
                              <Tag color="gold">
                                Nulls {formatPercentage(column.nullPercentage)}
                              </Tag>
                              {column.hasAnomalies ? (
                                <Tag color="red">
                                  Anomalies {formatNumber(column.anomalyCount, 0)}
                                </Tag>
                              ) : null}
                            </div>
                          </div>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </>
              )}
            </Card>

            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    AI findings and recommendations
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    The latest stored AI observations and recommendation signals for this version.
                  </Paragraph>
                </div>
                {dashboardSummary?.hasAutomaticAiInsight ? (
                  <Tag color="blue">Automatic insight available</Tag>
                ) : null}
              </div>

              {aiFindings?.recentFindings.length ? (
                <>
                  <div className={styles.statGrid}>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Stored AI responses</span>
                      <span className={styles.statValue}>
                        {formatNumber(aiFindings.storedAiResponseCount, 0)}
                      </span>
                    </div>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Stored insight records</span>
                      <span className={styles.statValue}>
                        {formatNumber(aiFindings.storedInsightRecordCount, 0)}
                      </span>
                    </div>
                  </div>

                  <ul className={styles.plainList}>
                    {aiFindings.recentFindings.map((finding) => (
                      <li key={`${finding.source}-${finding.creationTime}-${finding.title}`} className={styles.listItem}>
                        <div className={styles.listTitleRow}>
                          <div>
                            <div className={styles.listTitle}>{finding.title}</div>
                            <div className={styles.listMeta}>
                              {finding.source} · {formatRelativeTime(finding.creationTime)}
                            </div>
                          </div>
                        </div>
                        <Paragraph className={styles.listBody}>
                          {finding.contentPreview}
                        </Paragraph>
                      </li>
                    ))}
                  </ul>
                </>
              ) : (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No stored AI findings or recommendations are available yet for this version."
                />
              )}
            </Card>
          </div>

          <div className={styles.sectionGrid}>
            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Recent transformations
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    The latest transformation outcomes that shaped this dataset lineage.
                  </Paragraph>
                </div>
                <Tag color="purple">
                  {formatNumber(transformationOutcomes.length, 0)} recent steps
                </Tag>
              </div>

              {transformationOutcomes.length > 0 ? (
                <ul className={styles.plainList}>
                  {transformationOutcomes.map((outcome) => (
                    <li key={outcome.datasetTransformationId} className={styles.listItem}>
                      <div className={styles.listTitleRow}>
                        <div>
                          <div className={styles.listTitle}>
                            {getDatasetTransformationTypeLabel(outcome.transformationType)}
                          </div>
                          <div className={styles.listMeta}>
                            Executed {formatRelativeTime(outcome.executedAt)} · {formatDateTime(outcome.executedAt)}
                          </div>
                        </div>
                        <Tag color="purple">
                          Step {formatNumber(outcome.executionOrder, 0)}
                        </Tag>
                      </div>
                      <Paragraph className={styles.listBody}>
                        {outcome.summaryPreview || "This transformation completed successfully without a stored stakeholder-facing summary."}
                      </Paragraph>
                    </li>
                  ))}
                </ul>
              ) : (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No transformations have been recorded for this dataset version yet."
                />
              )}
            </Card>

            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    ML highlights
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    The latest completed ML signal for this version, including top metrics and warnings.
                  </Paragraph>
                </div>
                {mlHighlights?.hasCompletedExperiment && mlHighlights.status ? (
                  <Tag color={getMlExperimentStatusColor(mlHighlights.status)}>
                    {getMlExperimentStatusLabel(mlHighlights.status)}
                  </Tag>
                ) : null}
              </div>

              {!mlHighlights?.hasCompletedExperiment ? (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No completed ML experiment is available yet for this dataset version."
                />
              ) : (
                <>
                  <div className={styles.statGrid}>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Task</span>
                      <span className={styles.statValue}>
                        {mlHighlights.taskType ? getMlTaskTypeLabel(mlHighlights.taskType) : "Unknown"}
                      </span>
                    </div>
                    <div className={styles.statCard}>
                      <span className={styles.statLabel}>Primary metric</span>
                      <span className={styles.statValue}>
                        {mlHighlights.primaryMetricName && mlHighlights.primaryMetricValue !== null && mlHighlights.primaryMetricValue !== undefined
                          ? `${mlHighlights.primaryMetricName} · ${formatNumber(mlHighlights.primaryMetricValue, 4)}`
                          : "Unavailable"}
                      </span>
                    </div>
                  </div>

                  <div className={styles.tagRow}>
                    {mlHighlights.algorithmKey ? (
                      <Tag color="cyan">{getMlAlgorithmLabel(mlHighlights.algorithmKey)}</Tag>
                    ) : null}
                    {mlHighlights.modelType ? (
                      <Tag color="geekblue">{mlHighlights.modelType}</Tag>
                    ) : null}
                    {mlHighlights.targetColumnName ? (
                      <Tag color="gold">{`Target ${mlHighlights.targetColumnName}`}</Tag>
                    ) : null}
                    {mlHighlights.executedAt ? (
                      <Tag color="default">{formatDateTime(mlHighlights.executedAt)}</Tag>
                    ) : null}
                  </div>

                  {mlHighlights.metrics.length > 0 ? (
                    <ul className={styles.plainList}>
                      {mlHighlights.metrics.slice(0, 3).map((metric) => (
                        <li key={metric.metricName} className={styles.listItem}>
                          <div className={styles.listTitleRow}>
                            <div className={styles.listTitle}>{metric.metricName}</div>
                            <Tag color="blue">{formatNumber(metric.metricValue, 4)}</Tag>
                          </div>
                        </li>
                      ))}
                    </ul>
                  ) : null}

                  {mlHighlights.warnings.length > 0 ? (
                    <div className={styles.tagRow}>
                      {mlHighlights.warnings.slice(0, 4).map((warning) => (
                        <Tag key={warning} color="orange">
                          {warning}
                        </Tag>
                      ))}
                    </div>
                  ) : null}
                </>
              )}
            </Card>
          </div>

          <div className={styles.sectionGrid}>
            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Reports and exports
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    Generate stakeholder outputs here and reopen recent files without leaving the dashboard.
                  </Paragraph>
                </div>
              </div>

              <div className={styles.buttonRow}>
                <Button type="primary" loading={isGeneratingReport} onClick={onGenerateReport}>
                  Generate report
                </Button>
                <Button loading={isExportingPdf} onClick={() => onExportPdf(latestReport?.id)}>
                  Export PDF
                </Button>
                <Button loading={isExportingCsv} onClick={() => onExportCsv(latestReport?.id)}>
                  Export CSV
                </Button>
              </div>

              {recentReports.length > 0 ? (
                <ul className={styles.plainList}>
                  {recentReports.map((report) => (
                    <li key={report.id} className={styles.listItem}>
                      <div className={styles.listTitleRow}>
                        <div>
                          <div className={styles.listTitle}>{report.title}</div>
                          <div className={styles.listMeta}>
                            {formatDateTime(report.creationTime)}
                          </div>
                        </div>
                        <Tag color={report.summary ? "blue" : "default"}>
                          Report
                        </Tag>
                      </div>
                      <Paragraph className={styles.listBody}>
                        {report.summary || "Stored report ready for PDF export and later stakeholder sharing."}
                      </Paragraph>
                    </li>
                  ))}
                </ul>
              ) : (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No stakeholder reports have been generated for this version yet."
                />
              )}

              {recentExports.length > 0 ? (
                <ul className={styles.plainList}>
                  {recentExports.map((exportItem) => (
                    <li key={exportItem.id} className={styles.listItem}>
                      <div className={styles.listTitleRow}>
                        <div>
                          <div className={styles.listTitle}>{exportItem.displayName}</div>
                          <div className={styles.listMeta}>
                            {formatDateTime(exportItem.creationTime)}
                          </div>
                        </div>
                        <div className={styles.tagRow}>
                          <Tag color="geekblue">{getExportTypeLabel(exportItem)}</Tag>
                          {exportItem.sizeBytes ? (
                            <Tag color="default">{formatBytes(exportItem.sizeBytes)}</Tag>
                          ) : null}
                        </div>
                      </div>
                      <div className={styles.buttonRow}>
                        {exportItem.downloadUrl ? (
                          <Button size="small" type="primary" onClick={() => openDownload(exportItem.downloadUrl)}>
                            Download
                          </Button>
                        ) : null}
                        {exportItem.reportRecordId ? (
                          <Button size="small" onClick={() => onExportPdf(exportItem.reportRecordId || undefined)}>
                            Refresh PDF
                          </Button>
                        ) : null}
                      </div>
                    </li>
                  ))}
                </ul>
              ) : null}
            </Card>

            <Card className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <Title level={4} className={styles.cardTitle}>
                    Recent datasets
                  </Title>
                  <Paragraph className={styles.cardHelper}>
                    Quickly switch the dashboard context to another recent dataset in the workspace.
                  </Paragraph>
                </div>
              </div>

              {recentDatasets.length > 0 ? (
                <ul className={styles.plainList}>
                  {recentDatasets.map((dataset) => (
                    <li key={dataset.id} className={styles.listItem}>
                      <button
                        type="button"
                        className={styles.datasetListButton}
                        onClick={() => onSelectDataset(dataset.id)}
                      >
                        <div>
                          <div className={styles.listTitle}>{dataset.name}</div>
                          <div className={styles.listMeta}>
                            Created {formatRelativeTime(dataset.creationTime)}
                          </div>
                        </div>
                        <div className={styles.datasetListMeta}>
                          <Tag color={getDatasetStatusColor(dataset.status)}>
                            {getDatasetStatusLabel(dataset.status)}
                          </Tag>
                          {dataset.currentVersionNumber ? (
                            <Tag color="blue">{`v${dataset.currentVersionNumber}`}</Tag>
                          ) : null}
                        </div>
                      </button>
                    </li>
                  ))}
                </ul>
              ) : (
                <Empty
                  className={styles.emptyCompact}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="Recent datasets will appear here once the workspace contains uploaded data."
                />
              )}

              {recentInsights.length > 0 ? (
                <>
                  <Title level={5} className={styles.cardTitle}>
                    Stored insight records
                  </Title>
                  <ul className={styles.plainList}>
                    {recentInsights.map((insight) => (
                      <li key={insight.id} className={styles.listItem}>
                        <div className={styles.listTitleRow}>
                          <div>
                            <div className={styles.listTitle}>{insight.title}</div>
                            <div className={styles.listMeta}>
                              {formatDateTime(insight.creationTime)}
                            </div>
                          </div>
                        </div>
                        <Paragraph className={styles.listBody}>
                          {insight.content}
                        </Paragraph>
                      </li>
                    ))}
                  </ul>
                </>
              ) : null}
            </Card>
          </div>
        </>
      ) : null}
    </div>
  );
};
