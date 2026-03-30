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
  rawFile?: DatasetFileSummary | null;
}

export interface DatasetDetails {
  dataset: Dataset;
  versions: DatasetVersionSummary[];
  selectedVersion?: DatasetVersionDetails | null;
  columns: DatasetColumn[];
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
