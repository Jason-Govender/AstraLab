"use client";

import { Button, Card, Empty, Select, Tag, Typography } from "antd";
import { useRouter } from "next/navigation";
import type {
  AnalyticsExport,
  GeneratedAnalyticsExportResult,
  GeneratedDatasetReportResult,
  ReportRecord,
  ReportSourceType,
} from "@/types/analytics";
import type { DatasetDetails, DatasetListItem } from "@/types/datasets";
import { DatasetVersionType } from "@/types/datasets";
import { WorkspaceEmptyState } from "@/components/workspaceShell/WorkspaceEmptyState";
import { WorkspaceFeedbackAlert } from "@/components/workspaceShell/WorkspaceFeedbackAlert";
import {
  formatBytes,
  formatDateTime,
  getDatasetFormatLabel,
  getDatasetStatusColor,
  getDatasetStatusLabel,
  getDatasetVersionStatusColor,
  getDatasetVersionStatusLabel,
  getDatasetVersionTypeLabel,
} from "@/utils/datasets";
import { buildMlWorkspaceHref } from "@/utils/ml";
import { DatasetAiAssistantLauncherButton } from "@/components/datasets/ai/datasetAiAssistantLauncherButton";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface ReportsWorkspaceProps {
  recentDatasets: DatasetListItem[];
  selectedDatasetId?: number;
  selectedDatasetVersionId?: number;
  selectedDatasetDetails?: DatasetDetails | null;
  reports: ReportRecord[];
  exports: AnalyticsExport[];
  selectedReportId?: number;
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
  onSelectReport: (reportId?: number) => void;
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

const getReportSourceLabel = (sourceType: ReportSourceType): string => {
  switch (sourceType) {
    case 1:
      return "AI generated";
    case 2:
      return "System generated";
    default:
      return "Generated";
  }
};

const getExportTypeLabel = (exportItem: AnalyticsExport): string => {
  if (exportItem.contentType?.includes("pdf")) {
    return "PDF report";
  }

  if (exportItem.contentType?.includes("csv")) {
    return "CSV highlights";
  }

  return exportItem.displayName;
};

export const ReportsWorkspace = ({
  recentDatasets,
  selectedDatasetId,
  selectedDatasetVersionId,
  selectedDatasetDetails,
  reports,
  exports,
  selectedReportId,
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
  onSelectReport,
  onRefresh,
  onGenerateReport,
  onExportPdf,
  onExportCsv,
  onClearFeedback,
}: ReportsWorkspaceProps) => {
  const { styles, cx } = useStyles();
  const router = useRouter();
  const selectedVersion = selectedDatasetDetails?.selectedVersion;
  const selectedReport =
    reports.find((report) => report.id === selectedReportId) ?? reports[0];

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
            <Text className={styles.eyebrow}>Stakeholder Outputs</Text>
            <Title level={2} className={styles.title}>
              {selectedDatasetDetails?.dataset.name || "Reports workspace"}
            </Title>
            <Paragraph className={styles.description}>
              Generate stakeholder-ready reports, export structured highlights, and reopen previously saved outputs for the selected dataset version.
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
            </div>
          </div>

          <div className={styles.actionPanel}>
            <div className={styles.actionCluster}>
              <Text className={styles.actionLabel}>Explore</Text>
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
              </div>
            </div>

            <div className={styles.actionCluster}>
              <Text className={styles.actionLabel}>Generate and export</Text>
              <div className={styles.actionGroup}>
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
                  onClick={() => onExportPdf(selectedReport?.id)}
                  disabled={!selectedDatasetVersionId}
                >
                  Export selected PDF
                </Button>
                <Button
                  loading={isExportingCsv}
                  onClick={() => onExportCsv(selectedReport?.id)}
                  disabled={!selectedDatasetVersionId}
                >
                  Export highlights CSV
                </Button>
              </div>
            </div>
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
          <WorkspaceFeedbackAlert
            type="error"
            title="Unable to load"
            description={datasetErrorMessage}
            actionLabel="Retry"
            onAction={onRefresh}
          />
        ) : null}

        {contextErrorMessage ? (
          <WorkspaceFeedbackAlert
            type="error"
            title="Unable to load"
            description={contextErrorMessage}
            actionLabel="Retry"
            onAction={onRefresh}
          />
        ) : null}

        {actionErrorMessage ? (
          <WorkspaceFeedbackAlert
            type="error"
            className={styles.feedbackAlert}
            title="Action failed"
            description={actionErrorMessage}
            closable
            onClose={onClearFeedback}
          />
        ) : null}

        {lastGeneratedReport ? (
          <WorkspaceFeedbackAlert
            type="success"
            className={styles.feedbackAlert}
            title="Action completed"
            description={`Saved "${lastGeneratedReport.report.title}" for this dataset version.`}
            closable
            onClose={onClearFeedback}
          />
        ) : null}

        {lastGeneratedExport ? (
          <WorkspaceFeedbackAlert
            type="success"
            className={styles.feedbackAlert}
            title="Action completed"
            description={`${lastGeneratedExport.export.displayName} is ready to download.`}
            closable
            onClose={onClearFeedback}
            action={
              lastGeneratedExport.export.downloadUrl ? (
                <Button
                  size="small"
                  type="primary"
                  onClick={() => openDownload(lastGeneratedExport.export.downloadUrl)}
                >
                  Download
                </Button>
              ) : undefined
            }
          />
        ) : null}
      </Card>

      {!selectedDatasetId && recentDatasets.length === 0 && !isLoadingDatasets ? (
        <WorkspaceEmptyState
          className={styles.card}
          description="No datasets are available yet. Upload a dataset to start generating reports and exports."
        />
      ) : null}

      {selectedDatasetId &&
      selectedDatasetDetails &&
      selectedDatasetDetails.versions.length === 0 &&
      !isLoadingContext ? (
        <WorkspaceEmptyState
          className={styles.card}
          description="This dataset does not have any versions yet, so there are no reports to show."
        />
      ) : null}

      <div className={styles.contentGrid}>
        <div className={styles.mainColumn}>
          <Card className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <Title level={4} className={styles.cardTitle}>
                  Report preview
                </Title>
                <Paragraph className={styles.cardHelper}>
                  Review the selected report before sharing or exporting it.
                </Paragraph>
              </div>
              {selectedReport ? (
                <div className={styles.tagRow}>
                  <Tag color="blue">{getReportSourceLabel(selectedReport.reportSourceType)}</Tag>
                  <Tag color="default">{formatDateTime(selectedReport.creationTime)}</Tag>
                </div>
              ) : null}
            </div>

            {selectedReport ? (
              <>
                <div className={styles.summaryBlock}>
                  <span className={styles.summaryLabel}>{selectedReport.title}</span>
                  <Paragraph className={styles.summaryValue}>
                    {selectedReport.summary || "This report is ready for stakeholder sharing and export."}
                  </Paragraph>
                </div>

                <div className={styles.buttonRow}>
                  <Button type="primary" loading={isExportingPdf} onClick={() => onExportPdf(selectedReport.id)}>
                    Export selected report as PDF
                  </Button>
                  <Button loading={isExportingCsv} onClick={() => onExportCsv(selectedReport.id)}>
                    Export highlights as CSV
                  </Button>
                </div>

                {selectedReport.content?.trim() ? (
                  <iframe
                    title={`Report preview ${selectedReport.title}`}
                    className={styles.previewFrame}
                    sandbox=""
                    srcDoc={selectedReport.content}
                  />
                ) : (
                  <Empty
                    className={styles.emptyCompact}
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                    description="This report does not have previewable content yet."
                  />
                )}
              </>
            ) : (
              <Empty
                className={styles.emptyCompact}
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="Generate a report to preview stakeholder-ready content here."
              />
            )}
          </Card>

          <Card className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <Title level={4} className={styles.cardTitle}>
                  Generated reports
                </Title>
                <Paragraph className={styles.cardHelper}>
                  Select a stored report to preview it or export it again.
                </Paragraph>
              </div>
              <Button type="primary" loading={isGeneratingReport} onClick={onGenerateReport}>
                Generate report
              </Button>
            </div>

            {reports.length > 0 ? (
              <ul className={styles.plainList}>
                {reports.map((report) => (
                  <li
                    key={report.id}
                    className={cx(
                      styles.listItem,
                      report.id === selectedReport?.id && styles.listItemActive,
                    )}
                  >
                    <button
                      type="button"
                      className={styles.listButton}
                      onClick={() => onSelectReport(report.id)}
                    >
                      <div>
                        <div className={styles.listTitle}>{report.title}</div>
                        <div className={styles.listMeta}>
                          {formatDateTime(report.creationTime)}
                        </div>
                      </div>
                      <div className={styles.tagRow}>
                        <Tag color="blue">{getReportSourceLabel(report.reportSourceType)}</Tag>
                      </div>
                    </button>
                    <Paragraph className={styles.listBody}>
                      {report.summary || "Stored stakeholder-ready report content."}
                    </Paragraph>
                    <div className={styles.buttonRow}>
                      <Button size="small" onClick={() => onSelectReport(report.id)}>
                        Preview
                      </Button>
                      <Button
                        size="small"
                        type="primary"
                        loading={isExportingPdf && report.id === selectedReport?.id}
                        onClick={() => onExportPdf(report.id)}
                      >
                        PDF
                      </Button>
                      <Button
                        size="small"
                        loading={isExportingCsv && report.id === selectedReport?.id}
                        onClick={() => onExportCsv(report.id)}
                      >
                        CSV
                      </Button>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                className={styles.emptyCompact}
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No reports have been generated for this dataset version yet."
              />
            )}
          </Card>
        </div>

        <div className={styles.sideColumn}>
          <Card className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <Title level={4} className={styles.cardTitle}>
                  Exported outputs
                </Title>
                <Paragraph className={styles.cardHelper}>
                  Reopen previously generated files or create fresh exports from the current report context.
                </Paragraph>
              </div>
            </div>

            {exports.length > 0 ? (
              <ul className={styles.plainList}>
                {exports.map((exportItem) => (
                  <li key={exportItem.id} className={styles.listItem}>
                    <div className={styles.listTitle}>{exportItem.displayName}</div>
                    <div className={styles.listMeta}>
                      {formatDateTime(exportItem.creationTime)}
                    </div>
                    <div className={styles.tagRow}>
                      <Tag color="geekblue">{getExportTypeLabel(exportItem)}</Tag>
                      {exportItem.sizeBytes ? (
                        <Tag color="default">{formatBytes(exportItem.sizeBytes)}</Tag>
                      ) : null}
                    </div>
                    <div className={styles.buttonRow}>
                      {exportItem.downloadUrl ? (
                        <Button
                          size="small"
                          type="primary"
                          onClick={() => openDownload(exportItem.downloadUrl)}
                        >
                          Download
                        </Button>
                      ) : null}
                      {exportItem.reportRecordId ? (
                        <Button
                          size="small"
                          onClick={() => onSelectReport(exportItem.reportRecordId || undefined)}
                        >
                          Open source report
                        </Button>
                      ) : null}
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty
                className={styles.emptyCompact}
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No PDF or CSV exports have been generated for this version yet."
              />
            )}
          </Card>

          <Card className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <Title level={4} className={styles.cardTitle}>
                  What gets exported
                </Title>
                <Paragraph className={styles.cardHelper}>
                  Keep stakeholder expectations clear when choosing an output format.
                </Paragraph>
              </div>
            </div>

            <ul className={styles.explainerList}>
              <li>PDF exports package the selected stakeholder report as a presentation-ready document.</li>
              <li>CSV exports capture structured analytical highlights rather than raw dataset rows.</li>
              <li>Reports stay scoped to the currently selected dataset version in this workspace.</li>
            </ul>
          </Card>
        </div>
      </div>
    </div>
  );
};
