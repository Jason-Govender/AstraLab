"use client";

import { useEffect, useEffectEvent, useState } from "react";
import { Button, Empty, InputNumber, Select, Tabs, Typography } from "antd";
import {
  DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
  DEFAULT_DATASET_EXPLORATION_ROW_PAGE_SIZE,
  DEFAULT_DATASET_EXPLORATION_SCATTER_POINT_COUNT,
  DEFAULT_DATASET_EXPLORATION_SORT_DIRECTION,
  DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
} from "@/constants/datasetExploration";
import { DatasetErrorState } from "@/components/datasets/shared/datasetErrorState";
import { useDatasetExplorationActions, useDatasetExplorationState } from "@/providers/datasetExplorationProvider";
import {
  useDatasetExplorationAnalysisActions,
  useDatasetExplorationAnalysisState,
} from "@/providers/datasetExplorationAnalysisProvider";
import type {
  DatasetExplorationColumns,
  DatasetExplorationWorkspaceTab,
} from "@/types/datasets";
import {
  buildDefaultBarChartRequest,
  buildDefaultCorrelationRequest,
  buildDefaultDistributionRequest,
  buildDefaultHistogramRequest,
  buildDefaultScatterPlotRequest,
  getBarChartEligibleColumns,
  getDistributionEligibleColumns,
  getNumericExplorationColumns,
} from "@/utils/datasetExploration";
import { DatasetBarChartCard } from "../datasetBarChartCard";
import { DatasetCorrelationMatrixCard } from "../datasetCorrelationMatrixCard";
import { DatasetDistributionSummaryCard } from "../datasetDistributionSummaryCard";
import { DatasetExplorationRowsTable } from "../datasetExplorationRowsTable";
import { DatasetHistogramChartCard } from "../datasetHistogramChartCard";
import { DatasetScatterPlotCard } from "../datasetScatterPlotCard";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface DatasetExplorationWorkspaceProps {
  datasetVersionId: number;
  explorationColumns?: DatasetExplorationColumns;
}

export const DatasetExplorationWorkspace = ({
  datasetVersionId,
  explorationColumns,
}: DatasetExplorationWorkspaceProps) => {
  const { styles } = useStyles();
  const { getRows, setRowQuery, refreshRows } = useDatasetExplorationActions();
  const {
    isLoadingRows,
    isRowsError,
    rowQuery,
    rows,
    rowsErrorMessage,
    totalRowCount,
  } = useDatasetExplorationState();
  const {
    activeTab,
    barChart,
    correlation,
    distribution,
    errorMessage,
    histogram,
    isError,
    isLoadingAnalysis,
    scatterPlot,
  } = useDatasetExplorationAnalysisState();
  const {
    clearAnalysisResult,
    getBarChart,
    getCorrelation,
    getDistribution,
    getHistogram,
    getScatterPlot,
    setActiveTab,
  } = useDatasetExplorationAnalysisActions();
  const [histogramRequest, setHistogramRequest] = useState(
    buildDefaultHistogramRequest(datasetVersionId, explorationColumns?.columns || []),
  );
  const [barChartRequest, setBarChartRequest] = useState(
    buildDefaultBarChartRequest(datasetVersionId, explorationColumns?.columns || []),
  );
  const [scatterPlotRequest, setScatterPlotRequest] = useState(
    buildDefaultScatterPlotRequest(datasetVersionId, explorationColumns?.columns || []),
  );
  const [distributionRequest, setDistributionRequest] = useState(
    buildDefaultDistributionRequest(datasetVersionId, explorationColumns?.columns || []),
  );
  const [correlationRequest, setCorrelationRequest] = useState(
    buildDefaultCorrelationRequest(datasetVersionId, explorationColumns?.columns || []),
  );

  const resetAnalysisState = useEffectEvent(() => {
    setHistogramRequest(
      buildDefaultHistogramRequest(datasetVersionId, explorationColumns?.columns || []),
    );
    setBarChartRequest(
      buildDefaultBarChartRequest(datasetVersionId, explorationColumns?.columns || []),
    );
    setScatterPlotRequest(
      buildDefaultScatterPlotRequest(datasetVersionId, explorationColumns?.columns || []),
    );
    setDistributionRequest(
      buildDefaultDistributionRequest(datasetVersionId, explorationColumns?.columns || []),
    );
    setCorrelationRequest(
      buildDefaultCorrelationRequest(datasetVersionId, explorationColumns?.columns || []),
    );
    clearAnalysisResult();
  });

  useEffect(() => {
    resetAnalysisState();
  }, [datasetVersionId, explorationColumns]);

  const effectiveRowQuery = rowQuery || {
    datasetVersionId,
    page: 1,
    pageSize: DEFAULT_DATASET_EXPLORATION_ROW_PAGE_SIZE,
    sortDirection: DEFAULT_DATASET_EXPLORATION_SORT_DIRECTION,
  };
  const numericColumns = getNumericExplorationColumns(explorationColumns?.columns || []);
  const barChartColumns = getBarChartEligibleColumns(explorationColumns?.columns || []);
  const distributionColumns = getDistributionEligibleColumns(
    explorationColumns?.columns || [],
  );

  const handleRowsQueryChange = (nextQuery: typeof effectiveRowQuery) => {
    setRowQuery(nextQuery);
    void getRows(nextQuery);
  };

  const renderAnalysisError = (retryAction: () => void) =>
    isError ? (
      <DatasetErrorState
        title="Unable to load exploration analysis"
        message={errorMessage || "Please try running this analysis again."}
        action={
          <Button type="primary" onClick={retryAction}>
            Retry analysis
          </Button>
        }
      />
    ) : null;

  return (
    <div className={styles.stack}>
      <div className={styles.header}>
        <div>
          <Title level={3} className={styles.title}>
            Explore Dataset Version
          </Title>
          <Paragraph className={styles.helperText}>
            Inspect the selected dataset version as rows, charts, and backend-generated analysis views.
          </Paragraph>
        </div>
      </div>

      <Tabs
        activeKey={activeTab}
        onChange={(tab) => setActiveTab(tab as DatasetExplorationWorkspaceTab)}
        items={[
          {
            key: "rows",
            label: "Rows",
            children: isRowsError ? (
              <DatasetErrorState
                title="Unable to load dataset rows"
                message={rowsErrorMessage || "Please try loading the rows again."}
                action={
                  <Button type="primary" onClick={() => void refreshRows()}>
                    Retry rows
                  </Button>
                }
              />
            ) : (
              <DatasetExplorationRowsTable
                columns={explorationColumns?.columns || []}
                rows={rows}
                totalCount={totalRowCount}
                query={effectiveRowQuery}
                isLoading={isLoadingRows}
                onQueryChange={handleRowsQueryChange}
              />
            ),
          },
          {
            key: "distribution",
            label: "Distribution",
            children: (
              <div className={styles.tabStack}>
                <div className={styles.controlsRow}>
                  <Select
                    size="large"
                    value={distributionRequest?.datasetColumnId}
                    className={styles.select}
                    placeholder="Select a column"
                    options={distributionColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(datasetColumnId) =>
                      setDistributionRequest((currentRequest) => ({
                        datasetVersionId,
                        datasetColumnId,
                        bucketCount:
                          currentRequest?.bucketCount ||
                          DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
                        topCategoryCount:
                          currentRequest?.topCategoryCount ||
                          DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
                      }))
                    }
                  />
                  <InputNumber
                    min={1}
                    max={50}
                    value={distributionRequest?.bucketCount}
                    className={styles.numberInput}
                    addonBefore="Buckets"
                    onChange={(bucketCount) =>
                      setDistributionRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              bucketCount:
                                typeof bucketCount === "number"
                                  ? bucketCount
                                  : DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <InputNumber
                    min={1}
                    max={25}
                    value={distributionRequest?.topCategoryCount}
                    className={styles.numberInput}
                    addonBefore="Top Categories"
                    onChange={(topCategoryCount) =>
                      setDistributionRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              topCategoryCount:
                                typeof topCategoryCount === "number"
                                  ? topCategoryCount
                                  : DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <Button
                    type="primary"
                    loading={isLoadingAnalysis && activeTab === "distribution"}
                    disabled={!distributionRequest}
                    onClick={() =>
                      distributionRequest
                        ? void getDistribution(distributionRequest)
                        : undefined
                    }
                  >
                    Run analysis
                  </Button>
                </div>
                {renderAnalysisError(() => {
                  if (distributionRequest) {
                    void getDistribution(distributionRequest);
                  }
                })}
                <DatasetDistributionSummaryCard
                  distribution={distribution}
                  isLoading={isLoadingAnalysis && activeTab === "distribution"}
                />
              </div>
            ),
          },
          {
            key: "histogram",
            label: "Histogram",
            children: (
              <div className={styles.tabStack}>
                <div className={styles.controlsRow}>
                  <Select
                    size="large"
                    value={histogramRequest?.datasetColumnId}
                    className={styles.select}
                    placeholder="Select a numeric column"
                    options={numericColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(datasetColumnId) =>
                      setHistogramRequest((currentRequest) => ({
                        datasetVersionId,
                        datasetColumnId,
                        bucketCount:
                          currentRequest?.bucketCount ||
                          DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
                      }))
                    }
                  />
                  <InputNumber
                    min={1}
                    max={50}
                    value={histogramRequest?.bucketCount}
                    className={styles.numberInput}
                    addonBefore="Buckets"
                    onChange={(bucketCount) =>
                      setHistogramRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              bucketCount:
                                typeof bucketCount === "number"
                                  ? bucketCount
                                  : DEFAULT_DATASET_EXPLORATION_HISTOGRAM_BUCKET_COUNT,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <Button
                    type="primary"
                    loading={isLoadingAnalysis && activeTab === "histogram"}
                    disabled={!histogramRequest}
                    onClick={() =>
                      histogramRequest ? void getHistogram(histogramRequest) : undefined
                    }
                  >
                    Run analysis
                  </Button>
                </div>
                {renderAnalysisError(() => {
                  if (histogramRequest) {
                    void getHistogram(histogramRequest);
                  }
                })}
                <DatasetHistogramChartCard
                  histogram={histogram}
                  isLoading={isLoadingAnalysis && activeTab === "histogram"}
                />
              </div>
            ),
          },
          {
            key: "barChart",
            label: "Bar Chart",
            children: (
              <div className={styles.tabStack}>
                <div className={styles.controlsRow}>
                  <Select
                    size="large"
                    value={barChartRequest?.datasetColumnId}
                    className={styles.select}
                    placeholder="Select a categorical column"
                    options={barChartColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(datasetColumnId) =>
                      setBarChartRequest((currentRequest) => ({
                        datasetVersionId,
                        datasetColumnId,
                        topCategoryCount:
                          currentRequest?.topCategoryCount ||
                          DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
                      }))
                    }
                  />
                  <InputNumber
                    min={1}
                    max={25}
                    value={barChartRequest?.topCategoryCount}
                    className={styles.numberInput}
                    addonBefore="Top Categories"
                    onChange={(topCategoryCount) =>
                      setBarChartRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              topCategoryCount:
                                typeof topCategoryCount === "number"
                                  ? topCategoryCount
                                  : DEFAULT_DATASET_EXPLORATION_TOP_CATEGORY_COUNT,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <Button
                    type="primary"
                    loading={isLoadingAnalysis && activeTab === "barChart"}
                    disabled={!barChartRequest}
                    onClick={() =>
                      barChartRequest ? void getBarChart(barChartRequest) : undefined
                    }
                  >
                    Run analysis
                  </Button>
                </div>
                {renderAnalysisError(() => {
                  if (barChartRequest) {
                    void getBarChart(barChartRequest);
                  }
                })}
                <DatasetBarChartCard
                  barChart={barChart}
                  isLoading={isLoadingAnalysis && activeTab === "barChart"}
                />
              </div>
            ),
          },
          {
            key: "scatterPlot",
            label: "Scatter Plot",
            children: (
              <div className={styles.tabStack}>
                <div className={styles.controlsRow}>
                  <Select
                    size="large"
                    value={scatterPlotRequest?.xDatasetColumnId}
                    className={styles.select}
                    placeholder="Select X column"
                    options={numericColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(xDatasetColumnId) =>
                      setScatterPlotRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              datasetVersionId,
                              xDatasetColumnId,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <Select
                    size="large"
                    value={scatterPlotRequest?.yDatasetColumnId}
                    className={styles.select}
                    placeholder="Select Y column"
                    options={numericColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(yDatasetColumnId) =>
                      setScatterPlotRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              datasetVersionId,
                              yDatasetColumnId,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <InputNumber
                    min={10}
                    max={5000}
                    value={scatterPlotRequest?.maxPointCount}
                    className={styles.numberInput}
                    addonBefore="Max Points"
                    onChange={(maxPointCount) =>
                      setScatterPlotRequest((currentRequest) =>
                        currentRequest
                          ? {
                              ...currentRequest,
                              maxPointCount:
                                typeof maxPointCount === "number"
                                  ? maxPointCount
                                  : DEFAULT_DATASET_EXPLORATION_SCATTER_POINT_COUNT,
                            }
                          : currentRequest
                      )
                    }
                  />
                  <Button
                    type="primary"
                    loading={isLoadingAnalysis && activeTab === "scatterPlot"}
                    disabled={
                      !scatterPlotRequest ||
                      scatterPlotRequest.xDatasetColumnId ===
                        scatterPlotRequest.yDatasetColumnId
                    }
                    onClick={() =>
                      scatterPlotRequest
                        ? void getScatterPlot(scatterPlotRequest)
                        : undefined
                    }
                  >
                    Run analysis
                  </Button>
                </div>
                {renderAnalysisError(() => {
                  if (scatterPlotRequest) {
                    void getScatterPlot(scatterPlotRequest);
                  }
                })}
                <DatasetScatterPlotCard
                  scatterPlot={scatterPlot}
                  isLoading={isLoadingAnalysis && activeTab === "scatterPlot"}
                />
              </div>
            ),
          },
          {
            key: "correlation",
            label: "Correlation",
            children: (
              <div className={styles.tabStack}>
                <div className={styles.controlsRow}>
                  <Select
                    mode="multiple"
                    size="large"
                    value={correlationRequest?.datasetColumnIds}
                    className={styles.multiSelect}
                    placeholder="Select numeric columns"
                    options={numericColumns.map((column) => ({
                      label: column.name,
                      value: column.datasetColumnId,
                    }))}
                    onChange={(datasetColumnIds) =>
                      setCorrelationRequest({
                        datasetVersionId,
                        datasetColumnIds: datasetColumnIds.map(Number),
                      })
                    }
                  />
                  <Button
                    type="primary"
                    loading={isLoadingAnalysis && activeTab === "correlation"}
                    disabled={
                      !correlationRequest ||
                      correlationRequest.datasetColumnIds.length < 2
                    }
                    onClick={() =>
                      correlationRequest
                        ? void getCorrelation(correlationRequest)
                        : undefined
                    }
                  >
                    Run analysis
                  </Button>
                </div>
                {renderAnalysisError(() => {
                  if (correlationRequest) {
                    void getCorrelation(correlationRequest);
                  }
                })}
                <DatasetCorrelationMatrixCard
                  correlation={correlation}
                  isLoading={isLoadingAnalysis && activeTab === "correlation"}
                />
              </div>
            ),
          },
        ]}
      />

      {!explorationColumns || explorationColumns.columns.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Exploration columns will appear here when the selected dataset version is eligible for exploration."
        />
      ) : null}
    </div>
  );
};
