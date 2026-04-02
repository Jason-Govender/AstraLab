export enum DatasetFormat {
  Csv = 1,
  Json = 2,
}

export enum DatasetStatus {
  Uploaded = 1,
  Profiling = 2,
  Ready = 3,
  Failed = 4,
  Archived = 5,
}

export enum DatasetVersionStatus {
  Draft = 1,
  Active = 2,
  Superseded = 3,
  Failed = 4,
}

export enum DatasetVersionType {
  Raw = 1,
  Processed = 2,
}

export enum DatasetRowSortDirection {
  Ascending = 1,
  Descending = 2,
}

export enum DatasetTransformationType {
  RemoveDuplicates = 1,
  HandleMissingValues = 2,
  ConvertDataType = 3,
  FilterRows = 4,
  Aggregate = 5,
}

export enum MissingValueStrategy {
  DropRows = "dropRows",
  FillConstant = "fillConstant",
  FillMean = "fillMean",
  FillMode = "fillMode",
}

export enum FilterMatchMode {
  All = "all",
  Any = "any",
}

export enum FilterOperator {
  Equals = "equals",
  NotEquals = "notEquals",
  GreaterThan = "greaterThan",
  GreaterThanOrEqual = "greaterThanOrEqual",
  LessThan = "lessThan",
  LessThanOrEqual = "lessThanOrEqual",
  Contains = "contains",
  StartsWith = "startsWith",
  EndsWith = "endsWith",
  IsNull = "isNull",
  IsNotNull = "isNotNull",
}

export enum AggregationFunction {
  Count = "count",
  Sum = "sum",
  Average = "average",
  Min = "min",
  Max = "max",
}

export enum DatasetTransformationDataType {
  Integer = "integer",
  Decimal = "decimal",
  Boolean = "boolean",
  DateTime = "datetime",
  String = "string",
}

export enum AIResponseType {
  Summary = 1,
  Recommendation = 2,
  Explanation = 3,
  Insight = 4,
  QuestionAnswer = 5,
}

export interface Dataset {
  id: number;
  name: string;
  description?: string | null;
  sourceFormat: DatasetFormat;
  status: DatasetStatus;
  ownerUserId: number;
  originalFileName: string;
  creationTime: string;
}

export interface AIResponse {
  id: number;
  aiConversationId: number;
  datasetVersionId: number;
  userQuery?: string | null;
  responseContent: string;
  responseType: AIResponseType;
  datasetTransformationId?: number | null;
  metadataJson?: string | null;
  creationTime: string;
}

export interface AIConversation {
  id: number;
  datasetId: number;
  ownerUserId: number;
  lastInteractionTime: string;
  creationTime: string;
  responseCount: number;
  latestDatasetVersionId?: number | null;
  latestResponseType?: AIResponseType | null;
  latestUserQuery?: string | null;
  latestResponsePreview?: string | null;
}

export interface AskDatasetAiQuestionRequest {
  datasetVersionId: number;
  question: string;
  conversationId?: number;
}

export interface GenerateDatasetAiResponseResult {
  conversationId: number;
  response: AIResponse;
}

export interface GetDatasetAiConversationsRequest {
  datasetId: number;
  datasetVersionId?: number;
  page: number;
  pageSize: number;
}

export interface GetDatasetAiResponsesRequest {
  conversationId: number;
  page: number;
  pageSize: number;
  isChronological?: boolean;
}

export interface DatasetColumn {
  id: number;
  datasetVersionId: number;
  name: string;
  dataType?: string | null;
  isDataTypeInferred: boolean;
  ordinal: number;
  nullCount?: number | null;
  distinctCount?: number | null;
  creationTime: string;
}

export interface DatasetProfileSummaryPayload {
  totalNullCount: number;
  overallNullPercentage: number;
  totalAnomalyCount: number;
  overallAnomalyPercentage: number;
}

export interface DatasetColumnStatistics {
  nullPercentage: number;
  mean?: number | null;
  min?: number | null;
  max?: number | null;
  anomalyCount: number;
  anomalyPercentage: number;
  hasAnomalies: boolean;
}

export interface DatasetColumnProfile {
  id: number;
  datasetProfileId: number;
  datasetColumnId: number;
  inferredDataType?: string | null;
  nullCount: number;
  nullPercentage: number;
  distinctCount?: number | null;
  statisticsJson?: string | null;
  creationTime: string;
}

export interface DatasetProfile {
  id: number;
  datasetVersionId: number;
  rowCount: number;
  duplicateRowCount: number;
  dataHealthScore: number;
  summaryJson?: string | null;
  columnProfiles: DatasetColumnProfile[];
  creationTime: string;
}

export interface DatasetProfileSummary {
  datasetVersionId: number;
  profileId: number;
  rowCount: number;
  duplicateRowCount: number;
  dataHealthScore: number;
  totalNullCount: number;
  overallNullPercentage: number;
  totalAnomalyCount: number;
  overallAnomalyPercentage: number;
  creationTime: string;
}

export interface DatasetTransformation {
  id: number;
  sourceDatasetVersionId: number;
  resultDatasetVersionId?: number | null;
  transformationType: DatasetTransformationType;
  configurationJson: string;
  executionOrder: number;
  executedAt: string;
  summaryJson?: string | null;
  creationTime: string;
}

export interface DatasetTransformationHistoryItem {
  transformation: DatasetTransformation;
  sourceVersion: DatasetVersionSummary;
  resultVersion?: DatasetVersionSummary | null;
}

export interface DatasetTransformationHistory {
  datasetId: number;
  currentVersionId?: number | null;
  items: DatasetTransformationHistoryItem[];
}

export interface DatasetColumnInsight {
  datasetColumnId: number;
  columnProfileId: number;
  name: string;
  ordinal: number;
  inferredDataType?: string | null;
  nullCount: number;
  nullPercentage: number;
  distinctCount?: number | null;
  mean?: number | null;
  min?: number | null;
  max?: number | null;
  anomalyCount: number;
  anomalyPercentage: number;
  hasAnomalies: boolean;
  creationTime: string;
}

export interface DatasetFileSummary {
  originalFileName: string;
  contentType?: string | null;
  sizeBytes: number;
  creationTime: string;
}

export interface DatasetVersion {
  id: number;
  datasetId: number;
  versionNumber: number;
  versionType: DatasetVersionType;
  status: DatasetVersionStatus;
  parentVersionId?: number | null;
  rowCount?: number | null;
  columnCount?: number | null;
  schemaJson?: string | null;
  sizeBytes: number;
  creationTime: string;
}

export interface DatasetVersionSummary {
  id: number;
  versionNumber: number;
  versionType: DatasetVersionType;
  status: DatasetVersionStatus;
  creationTime: string;
  columnCount?: number | null;
}

export interface DatasetVersionDetails extends DatasetVersion {
  profile?: DatasetProfile | null;
  rawFile?: DatasetFileSummary | null;
}

export interface DatasetDetails {
  dataset: Dataset;
  versions: DatasetVersionSummary[];
  selectedVersion?: DatasetVersionDetails | null;
  columns: DatasetColumn[];
}

export interface ProcessedDatasetVersion {
  version: DatasetVersionDetails;
  columns: DatasetColumn[];
  producedByTransformation?: DatasetTransformation | null;
}

export interface DatasetListItem {
  id: number;
  name: string;
  description?: string | null;
  sourceFormat: DatasetFormat;
  status: DatasetStatus;
  originalFileName: string;
  creationTime: string;
  currentVersionId?: number | null;
  currentVersionNumber?: number | null;
  currentVersionStatus?: DatasetVersionStatus | null;
  columnCount?: number | null;
}

export interface UploadRawDatasetFormValues {
  name: string;
  description?: string;
  file: File;
}

export interface UploadRawDatasetPayload {
  name: string;
  description?: string;
  file: File;
}

export interface UploadedRawDatasetResult {
  dataset: Dataset;
  datasetVersionId: number;
  storageProvider: string;
  storageKey: string;
  sizeBytes: number;
  checksumSha256: string;
  columnCount: number;
  schemaJson: string;
  columns: DatasetColumn[];
  rowCount: number;
  duplicateRowCount: number;
  dataHealthScore: number;
  columnProfiles: DatasetColumnProfile[];
}

export interface DatasetCatalogFilters {
  keyword: string;
  status?: DatasetStatus;
  page: number;
  pageSize: number;
}

export interface DatasetSchemaColumn {
  name: string;
  dataType?: string | null;
  isDataTypeInferred: boolean;
  ordinal: number;
}

export interface DatasetSchema {
  format: string;
  rootKind: string;
  columns: DatasetSchemaColumn[];
}

export interface DatasetProfileColumnsRequest {
  datasetVersionId: number;
  page: number;
  pageSize: number;
  hasAnomalies?: boolean;
  inferredDataType?: string;
}

export type DatasetExplorationWorkspaceTab =
  | "rows"
  | "distribution"
  | "histogram"
  | "barChart"
  | "scatterPlot"
  | "correlation";

export interface DatasetExplorationColumn {
  datasetColumnId: number;
  name: string;
  ordinal: number;
  dataType: string;
  isNumeric: boolean;
  isCategorical: boolean;
  isTemporal: boolean;
}

export interface DatasetExplorationColumns {
  datasetVersionId: number;
  columns: DatasetExplorationColumn[];
}

export interface DatasetRow {
  rowNumber: number;
  values: Array<string | null>;
}

export interface PagedDatasetRowsRequest {
  datasetVersionId: number;
  page: number;
  pageSize: number;
  sortDatasetColumnId?: number;
  sortDirection?: DatasetRowSortDirection;
}

export interface HistogramChartBucket {
  label: string;
  start: number;
  end: number;
  count: number;
}

export interface HistogramChart {
  datasetVersionId: number;
  datasetColumnId: number;
  columnName: string;
  bucketCount: number;
  valueCount: number;
  nullCount: number;
  min?: number | null;
  max?: number | null;
  buckets: HistogramChartBucket[];
}

export interface BarChartCategory {
  label: string;
  count: number;
}

export interface BarChart {
  datasetVersionId: number;
  datasetColumnId: number;
  columnName: string;
  distinctCategoryCount: number;
  nullCount: number;
  categories: BarChartCategory[];
}

export interface ScatterPlotPoint {
  rowNumber: number;
  x: number;
  y: number;
}

export interface ScatterPlot {
  datasetVersionId: number;
  xDatasetColumnId: number;
  yDatasetColumnId: number;
  xColumnName: string;
  yColumnName: string;
  pointCount: number;
  points: ScatterPlotPoint[];
}

export interface DistributionAnalysis {
  datasetVersionId: number;
  datasetColumnId: number;
  columnName: string;
  dataType: string;
  valueCount: number;
  nullCount: number;
  mean?: number | null;
  min?: number | null;
  max?: number | null;
  median?: number | null;
  distinctCount?: number | null;
  buckets: HistogramChartBucket[];
  categories: BarChartCategory[];
}

export interface CorrelationColumn {
  datasetColumnId: number;
  name: string;
}

export interface CorrelationPair {
  xDatasetColumnId: number;
  yDatasetColumnId: number;
  xColumnName: string;
  yColumnName: string;
  coefficient?: number | null;
  sampleSize: number;
}

export interface CorrelationAnalysis {
  datasetVersionId: number;
  columns: CorrelationColumn[];
  pairs: CorrelationPair[];
}

export interface DatasetHistogramRequest {
  datasetVersionId: number;
  datasetColumnId: number;
  bucketCount?: number;
}

export interface DatasetBarChartRequest {
  datasetVersionId: number;
  datasetColumnId: number;
  topCategoryCount?: number;
}

export interface DatasetScatterPlotRequest {
  datasetVersionId: number;
  xDatasetColumnId: number;
  yDatasetColumnId: number;
  maxPointCount?: number;
}

export interface DatasetDistributionRequest {
  datasetVersionId: number;
  datasetColumnId: number;
  bucketCount?: number;
  topCategoryCount?: number;
}

export interface DatasetCorrelationRequest {
  datasetVersionId: number;
  datasetColumnIds: number[];
}

export interface RemoveDuplicatesTransformationConfig {
  columns: string[];
}

export interface HandleMissingValuesTransformationConfig {
  columns: string[];
  strategy: MissingValueStrategy;
  constantValue?: string;
}

export interface ConvertDataTypeTransformationConfig {
  column?: string;
  targetType?: DatasetTransformationDataType;
}

export interface FilterRowsTransformationCondition {
  column?: string;
  operator: FilterOperator;
  value?: string;
}

export interface FilterRowsTransformationConfig {
  match: FilterMatchMode;
  conditions: FilterRowsTransformationCondition[];
}

export interface AggregateTransformationDefinition {
  function: AggregationFunction;
  column?: string;
  outputColumn: string;
}

export interface AggregateTransformationConfig {
  groupByColumns: string[];
  aggregations: AggregateTransformationDefinition[];
}

export type DatasetTransformationBuilderConfiguration =
  | RemoveDuplicatesTransformationConfig
  | HandleMissingValuesTransformationConfig
  | ConvertDataTypeTransformationConfig
  | FilterRowsTransformationConfig
  | AggregateTransformationConfig;

export interface DatasetTransformationBuilderStep {
  id: string;
  transformationType: DatasetTransformationType;
  configuration: DatasetTransformationBuilderConfiguration;
}

export interface DatasetTransformationStepRequest {
  transformationType: DatasetTransformationType;
  configurationJson: string;
}

export interface TransformDatasetVersionRequest {
  sourceDatasetVersionId: number;
  steps: DatasetTransformationStepRequest[];
}

export interface TransformDatasetVersionResult {
  sourceDatasetVersionId: number;
  finalDatasetVersionId: number;
  createdVersions: DatasetVersion[];
  transformations: DatasetTransformation[];
  finalProfile: DatasetProfileSummary;
}
