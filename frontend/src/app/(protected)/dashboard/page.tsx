"use client";

import { useEffect, useEffectEvent } from "react";
import { Button } from "antd";
import { useRouter, useSearchParams } from "next/navigation";
import { AnalyticsDashboardWorkspace } from "@/components/dashboard/analyticsDashboardWorkspace";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import {
  AnalyticsDashboardProvider,
  useAnalyticsDashboardActions,
  useAnalyticsDashboardState,
} from "@/providers/analyticsDashboardProvider";
import { useStyles } from "./style";

const parseQueryNumber = (value: string | null | undefined): number | undefined => {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return undefined;
  }

  return parsedValue;
};

const buildDashboardHref = (
  datasetId?: number,
  datasetVersionId?: number,
): string => {
  const searchParams = new URLSearchParams();

  if (datasetId) {
    searchParams.set("datasetId", String(datasetId));
  }

  if (datasetVersionId) {
    searchParams.set("versionId", String(datasetVersionId));
  }

  const queryString = searchParams.toString();

  return queryString ? `/dashboard?${queryString}` : "/dashboard";
};

const DashboardPageContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const searchParams = useSearchParams();
  const {
    clearDashboardActionFeedback,
    exportInsightsCsv,
    exportReportPdf,
    generateReport,
    getRecentDatasets,
    loadDashboardContext,
    refreshDashboard,
  } = useAnalyticsDashboardActions();
  const {
    actionErrorMessage,
    analyticsSummary,
    contextErrorMessage,
    datasetErrorMessage,
    isExportingCsv,
    isExportingPdf,
    isGeneratingReport,
    isLoadingContext,
    isLoadingDatasets,
    lastGeneratedExport,
    lastGeneratedReport,
    recentDatasets,
    recentExports,
    recentInsights,
    recentReports,
    selectedDatasetDetails,
    selectedDatasetId,
    selectedDatasetVersionId,
  } = useAnalyticsDashboardState();

  const datasetId = parseQueryNumber(searchParams.get("datasetId"));
  const versionId = parseQueryNumber(searchParams.get("versionId"));

  const loadRecentDatasets = useEffectEvent(() => {
    void getRecentDatasets();
  });

  useEffect(() => {
    loadRecentDatasets();
  }, []);

  const loadDashboard = useEffectEvent(() => {
    if (!datasetId) {
      return;
    }

    void loadDashboardContext(datasetId, versionId);
  });

  useEffect(() => {
    loadDashboard();
  }, [datasetId, versionId]);

  useEffect(() => {
    if (datasetId || recentDatasets.length === 0) {
      return;
    }

    const defaultDataset =
      recentDatasets.find((dataset) => dataset.currentVersionId) ?? recentDatasets[0];

    router.replace(
      buildDashboardHref(
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
        router.replace(buildDashboardHref(datasetId), {
          scroll: false,
        });
      }

      return;
    }

    if (effectiveVersionId !== versionId) {
      router.replace(buildDashboardHref(datasetId, effectiveVersionId), {
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
      buildDashboardHref(
        nextDatasetId,
        currentDataset?.currentVersionId ?? undefined,
      ),
    );
  };

  const handleVersionChange = (nextVersionId?: number) => {
    router.push(buildDashboardHref(selectedDatasetId, nextVersionId));
  };

  return (
    <>
      <WorkspacePageHeader
        title="Analytics Dashboard"
        description="A polished executive summary of dataset quality, AI findings, transformation outcomes, ML highlights, and stakeholder-ready reporting actions."
        actions={
          <div className={styles.actionGroup}>
            <Button onClick={() => router.push("/datasets")}>Browse datasets</Button>
            <Button type="primary" onClick={() => router.push("/datasets/upload")}>
              Upload dataset
            </Button>
          </div>
        }
      />

      <AnalyticsDashboardWorkspace
        recentDatasets={recentDatasets}
        selectedDatasetId={selectedDatasetId ?? datasetId}
        selectedDatasetVersionId={selectedDatasetVersionId ?? versionId}
        selectedDatasetDetails={selectedDatasetDetails}
        analyticsSummary={analyticsSummary}
        recentInsights={recentInsights}
        recentReports={recentReports}
        recentExports={recentExports}
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
        onRefresh={() => void refreshDashboard()}
        onGenerateReport={() => void generateReport()}
        onExportPdf={(reportRecordId) => void exportReportPdf(reportRecordId)}
        onExportCsv={(reportRecordId) => void exportInsightsCsv(reportRecordId)}
        onClearFeedback={clearDashboardActionFeedback}
      />
    </>
  );
};

const DashboardPage = () => (
  <AnalyticsDashboardProvider>
    <DashboardPageContent />
  </AnalyticsDashboardProvider>
);

export default DashboardPage;
