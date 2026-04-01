export enum MlTaskType {
  Unknown = 0,
  Classification = 1,
  Regression = 2,
  Clustering = 3,
  AnomalyDetection = 4,
}

export enum MlExperimentStatus {
  Pending = 1,
  Running = 2,
  Completed = 3,
  Failed = 4,
  Cancelled = 5,
}

export interface MlExperimentFeature {
  datasetColumnId: number;
  name?: string | null;
  ordinal: number;
}

export interface MlModelMetric {
  metricName: string;
  metricValue: number;
}

export interface MlModelFeatureImportance {
  datasetColumnId: number;
  columnName?: string | null;
  importanceScore: number;
  rank: number;
}

export interface MlModel {
  modelType: string;
  artifactStorageProvider?: string | null;
  artifactStorageKey?: string | null;
  performanceSummaryJson?: string | null;
  warningsJson?: string | null;
  metrics: MlModelMetric[];
  featureImportances: MlModelFeatureImportance[];
}

export interface MlExperiment {
  id: number;
  datasetVersionId: number;
  taskType: MlTaskType;
  algorithmKey: string;
  targetDatasetColumnId?: number | null;
  targetColumnName?: string | null;
  status: MlExperimentStatus;
  trainingConfigurationJson: string;
  executedAt: string;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
  failureMessage?: string | null;
  dispatchErrorMessage?: string | null;
  warningsJson?: string | null;
  creationTime: string;
  features: MlExperimentFeature[];
  model?: MlModel | null;
}

export interface CreateMlExperimentRequest {
  datasetVersionId: number;
  taskType: MlTaskType;
  algorithmKey: string;
  featureDatasetColumnIds: number[];
  targetDatasetColumnId?: number;
  trainingConfigurationJson: string;
}

export interface MlAlgorithmOption {
  key: string;
  label: string;
  taskType: MlTaskType;
  description: string;
}
