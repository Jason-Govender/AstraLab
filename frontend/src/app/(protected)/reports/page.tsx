"use client";

import { useEffect, useEffectEvent } from "react";
import { Button } from "antd";
import { useRouter, useSearchParams } from "next/navigation";
import { ReportsWorkspace } from "@/components/reports/reportsWorkspace";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import {
  ReportsWorkspaceProvider,
  useReportsWorkspaceActions,
  useReportsWorkspaceState,
} from "@/providers/reportsWorkspaceProvider";
import { buildReportsHref } from "@/utils/reports";
import { useStyles } from "./style";

const parseQueryNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const ReportsPageContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const searchParams = useSearchParams();
  const {
    clearReportsActionFeedback,
    exportInsightsCsv,
    exportReportPdf,
    generateReport,
    getRecentDatasets,
    loadReportsWorkspace,
    refreshReportsWorkspace,
    selectReport,
  } = useReportsWorkspaceActions();
  const {
    actionErrorMessage,
    contextErrorMessage,
    datasetErrorMessage,
    exports,
    isExportingCsv,
    isExportingPdf,
    isGeneratingReport,
    isLoadingContext,
    isLoadingDatasets,
    lastGeneratedExport,
    lastGeneratedReport,
    recentDatasets,
    reports,
    selectedDatasetDetails,
    selectedDatasetId,
    selectedDatasetVersionId,
    selectedReportId,
  } = useReportsWorkspaceState();

  const datasetId = parseQueryNumber(searchParams.get("datasetId"));
  const versionId = parseQueryNumber(searchParams.get("versionId"));

  const loadRecentDatasets = useEffectEvent(() => {
    void getRecentDatasets();
  });

  useEffect(() => {
    loadRecentDatasets();
  }, []);

  const loadWorkspace = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void loadReportsWorkspace(datasetId, versionId);
  });

  useEffect(() => {
    loadWorkspace();
  }, [datasetId, versionId]);

  useEffect(() => {
    if (datasetId || recentDatasets.length === 0) {
      return;
    }

    const defaultDataset =
      recentDatasets.find((dataset) => dataset.currentVersionId) ?? recentDatasets[0];

    router.replace(
      buildReportsHref(
        defaultDataset.id,
        defaultDataset.currentVersionId ?? undefined,
      ),
      {
        scroll: false,
      },
    );
  }, [datasetId, recentDatasets, router]);

  useEffect(() => {
    if (!datasetId || !selectedDatasetDetails) {
      return;
    }

    const effectiveVersionId =
      selectedDatasetVersionId ?? selectedDatasetDetails.selectedVersion?.id;

    if (!effectiveVersionId) {
      if (versionId) {
        router.replace(buildReportsHref(datasetId), {
          scroll: false,
        });
      }

      return;
    }

    if (effectiveVersionId !== versionId) {
      router.replace(buildReportsHref(datasetId, effectiveVersionId), {
        scroll: false,
      });
    }
  }, [
    datasetId,
    router,
    selectedDatasetDetails,
    selectedDatasetVersionId,
    versionId,
  ]);

  const handleDatasetChange = (nextDatasetId?: number) => {
    const currentDataset = recentDatasets.find(
      (dataset) => dataset.id === nextDatasetId,
    );
    router.push(
      buildReportsHref(
        nextDatasetId,
        currentDataset?.currentVersionId ?? undefined,
      ),
    );
  };

  const handleVersionChange = (nextVersionId?: number) => {
    router.push(buildReportsHref(selectedDatasetId, nextVersionId));
  };

  return (
    <>
      <WorkspacePageHeader
        title="Reports"
        description="Generate stakeholder-ready reports, export structured outputs, and reopen previously saved PDF and CSV files from one dedicated workspace."
        actions={
          <div className={styles.actionGroup}>
            <Button onClick={() => router.push("/dashboard")}>Back to dashboard</Button>
            <Button onClick={() => router.push("/datasets")}>Browse datasets</Button>
            <Button type="primary" onClick={() => router.push("/datasets/upload")}>
              Upload dataset
            </Button>
          </div>
        }
      />

      <ReportsWorkspace
        recentDatasets={recentDatasets}
        selectedDatasetId={selectedDatasetId ?? datasetId}
        selectedDatasetVersionId={selectedDatasetVersionId ?? versionId}
        selectedDatasetDetails={selectedDatasetDetails}
        reports={reports}
        exports={exports}
        selectedReportId={selectedReportId}
        isLoadingDatasets={isLoadingDatasets}
        isLoadingContext={isLoadingContext}
        isGeneratingReport={isGeneratingReport}
        isExportingPdf={isExportingPdf}
        isExportingCsv={isExportingCsv}
        datasetErrorMessage={datasetErrorMessage}
        contextErrorMessage={contextErrorMessage}
        actionErrorMessage={actionErrorMessage}
        lastGeneratedReport={lastGeneratedReport}
        lastGeneratedExport={lastGeneratedExport}
        onSelectDataset={handleDatasetChange}
        onSelectVersion={handleVersionChange}
        onSelectReport={selectReport}
        onRefresh={() => {
          void getRecentDatasets();
          void refreshReportsWorkspace();
        }}
        onGenerateReport={() => void generateReport()}
        onExportPdf={(reportRecordId) => void exportReportPdf(reportRecordId)}
        onExportCsv={(reportRecordId) => void exportInsightsCsv(reportRecordId)}
        onClearFeedback={clearReportsActionFeedback}
      />
    </>
  );
};

const ReportsPage = () => (
  <ReportsWorkspaceProvider>
    <ReportsPageContent />
  </ReportsWorkspaceProvider>
);

export default ReportsPage;
