"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  exportDatasetInsightsCsv as exportDatasetInsightsCsvRequest,
  exportDatasetReportPdf as exportDatasetReportPdfRequest,
  generateDatasetReport as generateDatasetReportRequest,
  getExports,
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
  loadWorkspaceError,
  loadWorkspacePending,
  loadWorkspaceSuccess,
  selectReport as selectReportAction,
} from "./actions";
import {
  INITIAL_STATE,
  ReportsWorkspaceActionContext,
  ReportsWorkspaceStateContext,
} from "./context";
import { ReportsWorkspaceReducer } from "./reducer";

const RECENT_DATASET_PAGE_SIZE = 12;
const REPORTS_PAGE_SIZE = 20;
const EXPORTS_PAGE_SIZE = 20;

const sortDatasetsByMostRecent = <T extends { creationTime: string }>(
  items: T[],
): T[] =>
  [...items].sort(
    (left, right) =>
      new Date(right.creationTime).getTime() - new Date(left.creationTime).getTime(),
  );

const sortByNewest = <T extends { creationTime: string }>(items: T[]): T[] =>
  [...items].sort(
    (left, right) =>
      new Date(right.creationTime).getTime() - new Date(left.creationTime).getTime(),
  );

export const ReportsWorkspaceProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(ReportsWorkspaceReducer, INITIAL_STATE);

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
          getApiErrorMessage(error, "Unable to load reporting datasets right now."),
        ),
      );
    }
  };

  const loadReportsWorkspace = async (
    datasetId: number,
    datasetVersionId?: number,
    preferredReportId?: number,
  ) => {
    dispatch(loadWorkspacePending({ datasetId, datasetVersionId }));

    try {
      const datasetDetails = await getDatasetDetailsRequest(datasetId, datasetVersionId);
      const effectiveDatasetVersionId =
        datasetVersionId ?? datasetDetails.selectedVersion?.id;

      if (!effectiveDatasetVersionId) {
        dispatch(
          loadWorkspaceSuccess({
            datasetDetails,
            reports: [],
            exports: [],
            selectedDatasetId: datasetId,
            selectedDatasetVersionId: undefined,
            selectedReportId: undefined,
          }),
        );
        return;
      }

      const [reportsResult, exportsResult] = await Promise.all([
        getReports({
          datasetVersionId: effectiveDatasetVersionId,
          page: 1,
          pageSize: REPORTS_PAGE_SIZE,
        }),
        getExports({
          datasetVersionId: effectiveDatasetVersionId,
          page: 1,
          pageSize: EXPORTS_PAGE_SIZE,
        }),
      ]);

      const reports = sortByNewest(reportsResult.items);
      const exports = sortByNewest(exportsResult.items);
      const selectedReportId = reports.some(
        (report) => report.id === preferredReportId,
      )
        ? preferredReportId
        : reports.some((report) => report.id === state.selectedReportId)
          ? state.selectedReportId
          : reports[0]?.id;

      dispatch(
        loadWorkspaceSuccess({
          datasetDetails,
          reports,
          exports,
          selectedDatasetId: datasetId,
          selectedDatasetVersionId: effectiveDatasetVersionId,
          selectedReportId,
        }),
      );
    } catch (error) {
      dispatch(
        loadWorkspaceError({
          datasetId,
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the reports workspace for this dataset version right now.",
          ),
        }),
      );
    }
  };

  const refreshReportsWorkspace = async () => {
    if (!state.selectedDatasetId) {
      return;
    }

    await loadReportsWorkspace(
      state.selectedDatasetId,
      state.selectedDatasetVersionId,
    );
  };

  const selectReport = (reportId?: number) => {
    dispatch(selectReportAction(reportId));
  };

  const generateReport = async () => {
    if (!state.selectedDatasetId || !state.selectedDatasetVersionId) {
      return;
    }

    dispatch(generateReportPending());

    try {
      const reportResult = await generateDatasetReportRequest(
        state.selectedDatasetVersionId,
      );
      dispatch(generateReportSuccess(reportResult));
      await loadReportsWorkspace(
        state.selectedDatasetId,
        state.selectedDatasetVersionId,
        reportResult.report.id,
      );
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
      await refreshReportsWorkspace();
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
      await refreshReportsWorkspace();
    } catch (error) {
      dispatch(
        exportCsvError(
          getApiErrorMessage(error, "Unable to export the insights CSV right now."),
        ),
      );
    }
  };

  const clearReportsActionFeedback = () => {
    dispatch(clearActionFeedback());
  };

  return (
    <ReportsWorkspaceStateContext.Provider value={state}>
      <ReportsWorkspaceActionContext.Provider
        value={{
          getRecentDatasets,
          loadReportsWorkspace,
          refreshReportsWorkspace,
          selectReport,
          generateReport,
          exportReportPdf,
          exportInsightsCsv,
          clearReportsActionFeedback,
        }}
      >
        {children}
      </ReportsWorkspaceActionContext.Provider>
    </ReportsWorkspaceStateContext.Provider>
  );
};

export const useReportsWorkspaceState = () => {
  const context = useContext(ReportsWorkspaceStateContext);

  if (!context) {
    throw new Error(
      "useReportsWorkspaceState must be used within a ReportsWorkspaceProvider",
    );
  }

  return context;
};

export const useReportsWorkspaceActions = () => {
  const context = useContext(ReportsWorkspaceActionContext);

  if (!context) {
    throw new Error(
      "useReportsWorkspaceActions must be used within a ReportsWorkspaceProvider",
    );
  }

  return context;
};
