"use client";

import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Button,
  Card,
  Descriptions,
  Empty,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Table,
  Tag,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { ML_ALGORITHM_OPTIONS } from "@/constants/ml";
import type { DatasetColumn } from "@/types/datasets";
import {
  MlExperimentStatus,
  MlTaskType,
  type CreateMlExperimentRequest,
  type MlExperiment,
  type MlModelFeatureImportance,
  type MlModelMetric,
} from "@/types/ml";
import {
  useMlExperimentsActions,
  useMlExperimentsState,
} from "@/providers/mlExperimentsProvider";
import { formatDateTime, formatNumber } from "@/utils/datasets";
import {
  formatMlMetricLabel,
  getMlAlgorithmLabel,
  getMlExperimentStatusColor,
  getMlExperimentStatusLabel,
  getMlTaskTypeLabel,
  parseMlJsonArray,
  parseMlJsonObject,
} from "@/utils/ml";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;
const { TextArea } = Input;

interface MlExperimentWorkspaceProps {
  datasetVersionId?: number;
  columns: DatasetColumn[];
  hasRawFile: boolean;
}

interface MlExperimentFormValues {
  taskType: MlTaskType;
  algorithmKey: string;
  featureDatasetColumnIds: number[];
  targetDatasetColumnId?: number;
  randomState?: number;
  testSize?: number;
  maxIterations?: number;
  numberOfEstimators?: number;
  clusterCount?: number;
  contamination?: number;
  advancedConfigurationJson?: string;
}

const metricColumns: ColumnsType<MlModelMetric> = [
  {
    title: "Metric",
    dataIndex: "metricName",
    key: "metricName",
    render: (value: string) => formatMlMetricLabel(value),
  },
  {
    title: "Value",
    dataIndex: "metricValue",
    key: "metricValue",
    align: "right",
    render: (value: number) => formatNumber(value, 4),
  },
];

const featureImportanceColumns: ColumnsType<MlModelFeatureImportance> = [
  {
    title: "Rank",
    dataIndex: "rank",
    key: "rank",
    width: 80,
  },
  {
    title: "Feature",
    dataIndex: "columnName",
    key: "columnName",
    render: (value: string | null | undefined, record) =>
      value || `Column ${record.datasetColumnId}`,
  },
  {
    title: "Importance",
    dataIndex: "importanceScore",
    key: "importanceScore",
    align: "right",
    render: (value: number) => formatNumber(value, 4),
  },
];

const isSupervisedTask = (taskType?: MlTaskType) =>
  taskType === MlTaskType.Classification || taskType === MlTaskType.Regression;

const buildTrainingConfigurationJson = (
  values: MlExperimentFormValues,
): string => {
  const advancedConfiguration = values.advancedConfigurationJson?.trim()
    ? (JSON.parse(values.advancedConfigurationJson) as Record<string, unknown>)
    : {};

  const configuration: Record<string, unknown> = {
    ...advancedConfiguration,
  };

  if (values.randomState !== undefined && values.randomState !== null) {
    configuration.randomState = values.randomState;
  }

  if (values.testSize !== undefined && values.testSize !== null) {
    configuration.testSize = values.testSize;
  }

  if (values.maxIterations !== undefined && values.maxIterations !== null) {
    configuration.maxIterations = values.maxIterations;
  }

  if (
    values.numberOfEstimators !== undefined &&
    values.numberOfEstimators !== null
  ) {
    configuration.numberOfEstimators = values.numberOfEstimators;
  }

  if (values.clusterCount !== undefined && values.clusterCount !== null) {
    configuration.clusterCount = values.clusterCount;
  }

  if (values.contamination !== undefined && values.contamination !== null) {
    configuration.contamination = values.contamination;
  }

  return JSON.stringify(configuration);
};

const getExperimentWarnings = (experiment?: MlExperiment) => {
  const warningSet = new Set<string>([
    ...parseMlJsonArray(experiment?.warningsJson),
    ...parseMlJsonArray(experiment?.model?.warningsJson),
  ]);

  return Array.from(warningSet);
};

export const MlExperimentWorkspace = ({
  datasetVersionId,
  columns,
  hasRawFile,
}: MlExperimentWorkspaceProps) => {
  const { styles, cx } = useStyles();
  const [form] = Form.useForm<MlExperimentFormValues>();
  const [formErrorMessage, setFormErrorMessage] = useState<string>();
  const {
    cancelExperiment,
    createExperiment,
    retryExperiment,
    selectExperiment,
  } = useMlExperimentsActions();
  const {
    errorMessage,
    experiments,
    isError,
    isLoadingExperiments,
    isMutatingExperiment,
    isSubmittingExperiment,
    selectedExperimentId,
  } = useMlExperimentsState();

  const currentTaskType = Form.useWatch("taskType", form);
  const currentAlgorithmKey = Form.useWatch("algorithmKey", form);
  const selectedExperiment = experiments.find(
    (experiment) => experiment.id === selectedExperimentId,
  );

  const orderedColumns = useMemo(
    () => [...columns].sort((leftColumn, rightColumn) => leftColumn.ordinal - rightColumn.ordinal),
    [columns],
  );

  const availableAlgorithmOptions = ML_ALGORITHM_OPTIONS.filter(
    (option) => option.taskType === currentTaskType,
  );

  useEffect(() => {
    if (availableAlgorithmOptions.length === 0) {
      return;
    }

    if (
      !currentAlgorithmKey ||
      !availableAlgorithmOptions.some(
        (option) => option.key === currentAlgorithmKey,
      )
    ) {
      form.setFieldValue("algorithmKey", availableAlgorithmOptions[0].key);
    }
  }, [availableAlgorithmOptions, currentAlgorithmKey, form]);

  useEffect(() => {
    if (isSupervisedTask(currentTaskType)) {
      return;
    }

    form.setFieldValue("targetDatasetColumnId", undefined);
  }, [currentTaskType, form]);

  const handleCreateExperiment = async (values: MlExperimentFormValues) => {
    if (!datasetVersionId) {
      return;
    }

    try {
      const request: CreateMlExperimentRequest = {
        datasetVersionId,
        taskType: values.taskType,
        algorithmKey: values.algorithmKey,
        featureDatasetColumnIds: values.featureDatasetColumnIds,
        targetDatasetColumnId: isSupervisedTask(values.taskType)
          ? values.targetDatasetColumnId
          : undefined,
        trainingConfigurationJson: buildTrainingConfigurationJson(values),
      };

      setFormErrorMessage(undefined);
      await createExperiment(request);
    } catch (error) {
      setFormErrorMessage(
        error instanceof Error
          ? error.message
          : "The ML configuration JSON is not valid.",
      );
    }
  };

  const experimentWarnings = getExperimentWarnings(selectedExperiment);
  const performanceSummary = parseMlJsonObject(
    selectedExperiment?.model?.performanceSummaryJson,
  );
  const hasRunnableDataset =
    Boolean(datasetVersionId) && hasRawFile && orderedColumns.length > 0;

  return (
    <Card className={styles.card} loading={isLoadingExperiments}>
      <Title level={3} className={styles.title}>
        Machine Learning Workspace
      </Title>
      <Paragraph className={styles.helperText}>
        Launch async ML experiments on the selected dataset version, track their
        lifecycle, and review structured metrics, feature importance, and
        artifacts as they complete.
      </Paragraph>

      {!datasetVersionId ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Select a dataset version to work with machine learning experiments."
        />
      ) : null}

      {datasetVersionId && !hasRawFile ? (
        <Alert
          type="warning"
          showIcon
          message="This dataset version does not have a stored raw file."
          description="ML execution requires a persisted dataset file that the Python executor can read."
        />
      ) : null}

      {datasetVersionId && hasRawFile && orderedColumns.length === 0 ? (
        <Alert
          type="warning"
          showIcon
          message="This dataset version has no persisted columns."
          description="ML execution requires persisted dataset columns so features and targets can be selected."
        />
      ) : null}

      {hasRunnableDataset ? (
        <div className={styles.workspaceGrid}>
          <div className={styles.panel}>
            <Card className={styles.sectionCard}>
              <Title level={4}>Create Experiment</Title>
              <Paragraph type="secondary">
                Start with a baseline sklearn workflow, then refine the
                configuration over retries.
              </Paragraph>

              {formErrorMessage ? (
                <Alert
                  type="error"
                  showIcon
                  message="Invalid ML configuration"
                  description={formErrorMessage}
                  style={{ marginBottom: 16 }}
                />
              ) : null}

              <Form<MlExperimentFormValues>
                form={form}
                layout="vertical"
                initialValues={{
                  taskType: MlTaskType.Classification,
                  randomState: 42,
                  testSize: 0.2,
                  maxIterations: 300,
                  numberOfEstimators: 200,
                  clusterCount: 3,
                  contamination: 0.05,
                  advancedConfigurationJson: "{}",
                }}
                onFinish={(values) => void handleCreateExperiment(values)}
              >
                <div className={styles.formGrid}>
                  <Form.Item
                    name="taskType"
                    label="Workflow"
                    rules={[{ required: true, message: "Select an ML workflow." }]}
                  >
                    <Select
                      size="large"
                      options={[
                        {
                          label: "Classification",
                          value: MlTaskType.Classification,
                        },
                        { label: "Regression", value: MlTaskType.Regression },
                        { label: "Clustering", value: MlTaskType.Clustering },
                        {
                          label: "Anomaly Detection",
                          value: MlTaskType.AnomalyDetection,
                        },
                      ]}
                    />
                  </Form.Item>

                  <Form.Item
                    name="algorithmKey"
                    label="Algorithm"
                    rules={[{ required: true, message: "Select an algorithm." }]}
                  >
                    <Select
                      size="large"
                      options={availableAlgorithmOptions.map((option) => ({
                        label: option.label,
                        value: option.key,
                      }))}
                    />
                  </Form.Item>

                  <Form.Item
                    name="featureDatasetColumnIds"
                    label="Feature Columns"
                    className={styles.fullWidth}
                    rules={[
                      {
                        required: true,
                        message: "Select at least one feature column.",
                      },
                    ]}
                  >
                    <Select
                      mode="multiple"
                      size="large"
                      placeholder="Select feature columns"
                      options={orderedColumns.map((column) => ({
                        label: `${column.ordinal}. ${column.name}${column.dataType ? ` (${column.dataType})` : ""}`,
                        value: column.id,
                      }))}
                    />
                  </Form.Item>

                  {isSupervisedTask(currentTaskType) ? (
                    <Form.Item
                      name="targetDatasetColumnId"
                      label="Target Column"
                      className={styles.fullWidth}
                      rules={[
                        {
                          required: true,
                          message: "Select the supervised target column.",
                        },
                      ]}
                    >
                      <Select
                        size="large"
                        placeholder="Select target column"
                        options={orderedColumns.map((column) => ({
                          label: `${column.ordinal}. ${column.name}${column.dataType ? ` (${column.dataType})` : ""}`,
                          value: column.id,
                        }))}
                      />
                    </Form.Item>
                  ) : null}

                  <Form.Item name="randomState" label="Random State">
                    <InputNumber className={styles.fullWidth} style={{ width: "100%" }} />
                  </Form.Item>

                  {isSupervisedTask(currentTaskType) ? (
                    <Form.Item name="testSize" label="Test Size">
                      <InputNumber
                        min={0.1}
                        max={0.4}
                        step={0.05}
                        style={{ width: "100%" }}
                      />
                    </Form.Item>
                  ) : null}

                  {currentAlgorithmKey === "logistic_regression" ||
                  currentAlgorithmKey === "kmeans" ? (
                    <Form.Item name="maxIterations" label="Max Iterations">
                      <InputNumber min={50} max={5000} style={{ width: "100%" }} />
                    </Form.Item>
                  ) : null}

                  {currentAlgorithmKey === "random_forest_classifier" ||
                  currentAlgorithmKey === "random_forest_regressor" ||
                  currentAlgorithmKey === "isolation_forest" ? (
                    <Form.Item name="numberOfEstimators" label="Number Of Estimators">
                      <InputNumber min={10} max={1000} style={{ width: "100%" }} />
                    </Form.Item>
                  ) : null}

                  {currentAlgorithmKey === "kmeans" ? (
                    <Form.Item name="clusterCount" label="Cluster Count">
                      <InputNumber min={2} max={20} style={{ width: "100%" }} />
                    </Form.Item>
                  ) : null}

                  {currentAlgorithmKey === "isolation_forest" ? (
                    <Form.Item name="contamination" label="Contamination">
                      <InputNumber
                        min={0.001}
                        max={0.5}
                        step={0.01}
                        style={{ width: "100%" }}
                      />
                    </Form.Item>
                  ) : null}

                  <Form.Item
                    name="advancedConfigurationJson"
                    label="Advanced Configuration JSON"
                    className={styles.fullWidth}
                  >
                    <TextArea
                      rows={5}
                      placeholder='{"numberOfEstimators": 300}'
                    />
                  </Form.Item>
                </div>

                <Button
                  type="primary"
                  htmlType="submit"
                  size="large"
                  loading={isSubmittingExperiment}
                >
                  Run experiment
                </Button>
              </Form>
            </Card>
          </div>

          <div className={styles.panel}>
            <Card className={styles.sectionCard}>
              <Title level={4}>Experiment History</Title>
              <Paragraph type="secondary">
                Select an experiment to inspect its metrics, failures, and artifact metadata.
              </Paragraph>

              {isError && errorMessage ? (
                <Alert
                  type="error"
                  showIcon
                  message="Unable to refresh ML experiments"
                  description={errorMessage}
                  style={{ marginBottom: 16 }}
                />
              ) : null}

              {experiments.length === 0 ? (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No ML experiments have been created for this version yet."
                />
              ) : (
                <div className={styles.experimentList}>
                  {experiments.map((experiment) => (
                    <div
                      key={experiment.id}
                      className={cx(
                        styles.experimentItem,
                        experiment.id === selectedExperimentId &&
                          styles.experimentItemSelected,
                      )}
                      onClick={() => selectExperiment(experiment.id)}
                    >
                      <Space wrap>
                        <Tag color="geekblue">
                          {getMlTaskTypeLabel(experiment.taskType)}
                        </Tag>
                        <Tag color={getMlExperimentStatusColor(experiment.status)}>
                          {getMlExperimentStatusLabel(experiment.status)}
                        </Tag>
                      </Space>

                      <Title level={5} style={{ marginTop: 12, marginBottom: 0 }}>
                        {getMlAlgorithmLabel(experiment.algorithmKey)}
                      </Title>
                      <Paragraph className={styles.experimentMeta}>
                        Requested {formatDateTime(experiment.creationTime)}
                      </Paragraph>
                    </div>
                  ))}
                </div>
              )}
            </Card>

            <Card className={styles.sectionCard}>
              <Title level={4}>Experiment Details</Title>

              {!selectedExperiment ? (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="Choose an experiment from the history list to inspect it."
                />
              ) : (
                <Space direction="vertical" size="large" style={{ width: "100%" }}>
                  <Descriptions
                    size="small"
                    column={1}
                    items={[
                      {
                        key: "workflow",
                        label: "Workflow",
                        children: getMlTaskTypeLabel(selectedExperiment.taskType),
                      },
                      {
                        key: "algorithm",
                        label: "Algorithm",
                        children: getMlAlgorithmLabel(selectedExperiment.algorithmKey),
                      },
                      {
                        key: "target",
                        label: "Target",
                        children: selectedExperiment.targetColumnName || "None",
                      },
                      {
                        key: "requested",
                        label: "Requested",
                        children: formatDateTime(selectedExperiment.creationTime),
                      },
                      {
                        key: "completed",
                        label: "Completed",
                        children: selectedExperiment.completedAtUtc
                          ? formatDateTime(selectedExperiment.completedAtUtc)
                          : "Not finished",
                      },
                    ]}
                  />

                  <div className={styles.tagWrap}>
                    {selectedExperiment.features.map((feature) => (
                      <Tag key={feature.datasetColumnId}>
                        {feature.name || `Column ${feature.datasetColumnId}`}
                      </Tag>
                    ))}
                  </div>

                  <div className={styles.detailActions}>
                    {selectedExperiment.status === MlExperimentStatus.Pending ? (
                      <Button
                        danger
                        onClick={() => void cancelExperiment(selectedExperiment.id)}
                        loading={isMutatingExperiment}
                      >
                        Cancel experiment
                      </Button>
                    ) : null}

                    {selectedExperiment.status === MlExperimentStatus.Failed ||
                    selectedExperiment.status === MlExperimentStatus.Cancelled ||
                    (selectedExperiment.status === MlExperimentStatus.Pending &&
                      !selectedExperiment.startedAtUtc) ? (
                      <Button
                        onClick={() => void retryExperiment(selectedExperiment.id)}
                        loading={isMutatingExperiment}
                      >
                        Retry experiment
                      </Button>
                    ) : null}
                  </div>

                  {selectedExperiment.dispatchErrorMessage ? (
                    <Alert
                      type="warning"
                      showIcon
                      message="Dispatch warning"
                      description={selectedExperiment.dispatchErrorMessage}
                    />
                  ) : null}

                  {selectedExperiment.failureMessage ? (
                    <Alert
                      type="error"
                      showIcon
                      message="Execution failed"
                      description={selectedExperiment.failureMessage}
                    />
                  ) : null}

                  {experimentWarnings.length > 0 ? (
                    <Alert
                      type="info"
                      showIcon
                      message="Executor warnings"
                      description={
                        <ul style={{ marginBottom: 0, paddingLeft: 18 }}>
                          {experimentWarnings.map((warning) => (
                            <li key={warning}>{warning}</li>
                          ))}
                        </ul>
                      }
                    />
                  ) : null}

                  {selectedExperiment.model ? (
                    <>
                      {performanceSummary ? (
                        <Descriptions
                          size="small"
                          column={1}
                          items={Object.entries(performanceSummary).map(
                            ([key, value]) => ({
                              key,
                              label: formatMlMetricLabel(key),
                              children:
                                typeof value === "number"
                                  ? formatNumber(value, 4)
                                  : String(value),
                            }),
                          )}
                        />
                      ) : null}

                      <Descriptions
                        size="small"
                        column={1}
                        items={[
                          {
                            key: "artifact-provider",
                            label: "Artifact Provider",
                            children:
                              selectedExperiment.model.artifactStorageProvider ||
                              "Unavailable",
                          },
                          {
                            key: "artifact-key",
                            label: "Artifact Key",
                            children:
                              selectedExperiment.model.artifactStorageKey ||
                              "Unavailable",
                          },
                        ]}
                      />

                      <Table
                        rowKey="metricName"
                        size="small"
                        pagination={false}
                        columns={metricColumns}
                        dataSource={selectedExperiment.model.metrics}
                      />

                      <Table
                        rowKey={(record) => `${record.datasetColumnId}-${record.rank}`}
                        size="small"
                        pagination={false}
                        columns={featureImportanceColumns}
                        dataSource={selectedExperiment.model.featureImportances}
                      />
                    </>
                  ) : (
                    <Paragraph type="secondary">
                      Structured results will appear here once the experiment completes.
                    </Paragraph>
                  )}

                  <div>
                    <Text strong>Training Configuration</Text>
                    <pre className={styles.codeBlock}>
                      {selectedExperiment.trainingConfigurationJson}
                    </pre>
                  </div>
                </Space>
              )}
            </Card>
          </div>
        </div>
      ) : null}
    </Card>
  );
};
