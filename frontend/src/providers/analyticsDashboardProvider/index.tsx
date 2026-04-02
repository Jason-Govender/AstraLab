"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  exportDatasetInsightsCsv as exportDatasetInsightsCsvRequest,
  exportDatasetReportPdf as exportDatasetReportPdfRequest,
  generateDatasetReport as generateDatasetReportRequest,
  getDatasetAnalyticsSummary,
  getExports,
  getInsights,
  getReports,
} from "@/services/analyticsService";
import {
  getDatasetDetails as getDatasetDetailsRequest,
  getDatasets as getDatasetsRequest,
} from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearActionFeedback,
  exportCsvError,
  exportCsvPending,
  exportCsvSuccess,
  exportPdfError,
  exportPdfPending,
  exportPdfSuccess,
  generateReportError,
  generateReportPending,
  generateReportSuccess,
  getRecentDatasetsError,
  getRecentDatasetsPending,
  getRecentDatasetsSuccess,
  loadContextError,
  loadContextPending,
  loadContextSuccess,
} from "./actions";
import {
  AnalyticsDashboardActionContext,
  AnalyticsDashboardStateContext,
  INITIAL_STATE,
} from "./context";
import { AnalyticsDashboardReducer } from "./reducer";

const RECENT_DATASET_PAGE_SIZE = 12;
const RECENT_INSIGHTS_PAGE_SIZE = 5;
const RECENT_REPORTS_PAGE_SIZE = 5;
const RECENT_EXPORTS_PAGE_SIZE = 6;

const sortDatasetsByMostRecent = <T extends { creationTime: string }>(
  items: T[],
): T[] =>
  [...items].sort(
    (left, right) =>
      new Date(right.creationTime).getTime() - new Date(left.creationTime).getTime(),
  );

export const AnalyticsDashboardProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(AnalyticsDashboardReducer, INITIAL_STATE);

  const getRecentDatasets = async () => {
    dispatch(getRecentDatasetsPending());

    try {
      const result = await getDatasetsRequest({
        keyword: "",
        page: 1,
        pageSize: RECENT_DATASET_PAGE_SIZE,
      });

      dispatch(getRecentDatasetsSuccess(sortDatasetsByMostRecent(result.items)));
    } catch (error) {
      dispatch(
        getRecentDatasetsError(
          getApiErrorMessage(error, "Unable to load dashboard datasets right now."),
        ),
      );
    }
  };

  const loadDashboardContext = async (
    datasetId: number,
    datasetVersionId?: number,
  ) => {
    dispatch(loadContextPending({ datasetId, datasetVersionId }));

    try {
      const datasetDetails = await getDatasetDetailsRequest(datasetId, datasetVersionId);
      const effectiveDatasetVersionId =
        datasetVersionId ?? datasetDetails.selectedVersion?.id;

      if (!effectiveDatasetVersionId) {
        dispatch(
          loadContextSuccess({
            datasetDetails,
            analyticsSummary: null,
            recentInsights: [],
            recentReports: [],
            recentExports: [],
            selectedDatasetId: datasetId,
            selectedDatasetVersionId: undefined,
          }),
        );
        return;
      }

      const [analyticsSummary, insightsResult, reportsResult, exportsResult] =
        await Promise.all([
          getDatasetAnalyticsSummary(effectiveDatasetVersionId),
          getInsights({
            datasetVersionId: effectiveDatasetVersionId,
            page: 1,
            pageSize: RECENT_INSIGHTS_PAGE_SIZE,
          }),
          getReports({
            datasetVersionId: effectiveDatasetVersionId,
            page: 1,
            pageSize: RECENT_REPORTS_PAGE_SIZE,
          }),
          getExports({
            datasetVersionId: effectiveDatasetVersionId,
            page: 1,
            pageSize: RECENT_EXPORTS_PAGE_SIZE,
          }),
        ]);

      dispatch(
        loadContextSuccess({
          datasetDetails,
          analyticsSummary,
          recentInsights: insightsResult.items,
          recentReports: reportsResult.items,
          recentExports: exportsResult.items,
          selectedDatasetId: datasetId,
          selectedDatasetVersionId: effectiveDatasetVersionId,
        }),
      );
    } catch (error) {
      dispatch(
        loadContextError({
          datasetId,
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the analytics dashboard for this dataset version right now.",
          ),
        }),
      );
    }
  };

  const refreshDashboard = async () => {
    if (!state.selectedDatasetId) {
      return;
    }

    await loadDashboardContext(
      state.selectedDatasetId,
      state.selectedDatasetVersionId,
    );
  };

  const generateReport = async () => {
    if (!state.selectedDatasetVersionId) {
      return;
    }

    dispatch(generateReportPending());

    try {
      const reportResult = await generateDatasetReportRequest(
        state.selectedDatasetVersionId,
      );
      dispatch(generateReportSuccess(reportResult));
      await refreshDashboard();
    } catch (error) {
      dispatch(
        generateReportError(
          getApiErrorMessage(
            error,
            "Unable to generate the stakeholder report right now.",
          ),
        ),
      );
    }
  };

  const exportReportPdf = async (reportRecordId?: number) => {
    if (!state.selectedDatasetVersionId) {
      return;
    }

    dispatch(exportPdfPending());

    try {
      const exportResult = await exportDatasetReportPdfRequest({
        datasetVersionId: state.selectedDatasetVersionId,
        reportRecordId,
      });
      dispatch(exportPdfSuccess(exportResult));
      await refreshDashboard();
    } catch (error) {
      dispatch(
        exportPdfError(
          getApiErrorMessage(error, "Unable to export the PDF report right now."),
        ),
      );
    }
  };

  const exportInsightsCsv = async (reportRecordId?: number) => {
    if (!state.selectedDatasetVersionId) {
      return;
    }

    dispatch(exportCsvPending());

    try {
      const exportResult = await exportDatasetInsightsCsvRequest({
        datasetVersionId: state.selectedDatasetVersionId,
        reportRecordId,
      });
      dispatch(exportCsvSuccess(exportResult));
      await refreshDashboard();
    } catch (error) {
      dispatch(
        exportCsvError(
          getApiErrorMessage(error, "Unable to export the insights CSV right now."),
        ),
      );
    }
  };

  const clearDashboardActionFeedback = () => {
    dispatch(clearActionFeedback());
  };

  return (
    <AnalyticsDashboardStateContext.Provider value={state}>
      <AnalyticsDashboardActionContext.Provider
        value={{
          getRecentDatasets,
          loadDashboardContext,
          refreshDashboard,
          generateReport,
          exportReportPdf,
          exportInsightsCsv,
          clearDashboardActionFeedback,
        }}
      >
        {children}
      </AnalyticsDashboardActionContext.Provider>
    </AnalyticsDashboardStateContext.Provider>
  );
};

export const useAnalyticsDashboardState = () => {
  const context = useContext(AnalyticsDashboardStateContext);

  if (!context) {
    throw new Error(
      "useAnalyticsDashboardState must be used within an AnalyticsDashboardProvider",
    );
  }

  return context;
};

export const useAnalyticsDashboardActions = () => {
  const context = useContext(AnalyticsDashboardActionContext);

  if (!context) {
    throw new Error(
      "useAnalyticsDashboardActions must be used within an AnalyticsDashboardProvider",
    );
  }

  return context;
};
