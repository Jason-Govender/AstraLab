"use client";

import { Button, Card, Typography } from "antd";
import { useRouter } from "next/navigation";
import { WorkspacePageHeader } from "@/components/workspaceShell/WorkspacePageHeader";
import { DatasetUploadForm } from "@/components/datasets/ingestion/datasetUploadForm";
import {
  DatasetIngestionProvider,
  useDatasetIngestionActions,
  useDatasetIngestionState,
} from "@/providers/datasetIngestionProvider";
import type { UploadRawDatasetFormValues } from "@/types/datasets";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

const DatasetUploadContent = () => {
  const { styles } = useStyles();
  const router = useRouter();
  const { uploadRawDataset } = useDatasetIngestionActions();
  const { isUploading, errorMessage } = useDatasetIngestionState();

  const handleSubmit = async (values: UploadRawDatasetFormValues) => {
    try {
      const result = await uploadRawDataset(values);
      router.push(`/datasets/${result.dataset.id}?versionId=${result.datasetVersionId}`);
    } catch {
      return;
    }
  };

  return (
    <>
      <WorkspacePageHeader
        title="Upload Dataset"
        description="Submit a raw CSV or JSON file and let the backend validate, ingest, and derive the initial schema for you."
        actions={
          <div className={styles.actionGroup}>
            <Button size="large" onClick={() => router.push("/datasets")}>
              Back to datasets
            </Button>
          </div>
        }
      />

      <div className={styles.pageGrid}>
        <DatasetUploadForm
          isSubmitting={isUploading}
          errorMessage={errorMessage}
          onSubmit={handleSubmit}
        />

        <Card className={styles.helperCard}>
          <Title level={4} className={styles.helperTitle}>
            Ingestion Notes
          </Title>
          <Paragraph className={styles.helperText}>
            The backend accepts CSV and JSON uploads only.
          </Paragraph>
          <Paragraph className={styles.helperText}>
            Every successful upload creates a dataset, an initial raw version, stored raw file metadata, and extracted column/schema records.
          </Paragraph>
          <Paragraph className={styles.helperText}>
            Validation errors come directly from the backend ingestion boundary so the UI stays aligned with the canonical rules.
          </Paragraph>
        </Card>
      </div>
    </>
  );
};

const DatasetUploadPage = () => (
  <DatasetIngestionProvider>
    <DatasetUploadContent />
  </DatasetIngestionProvider>
);

export default DatasetUploadPage;
