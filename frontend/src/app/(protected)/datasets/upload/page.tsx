"use client";

import { useEffect } from "react";
import { Button, Card, Typography } from "antd";
import { useRouter } from "next/navigation";
import { DatasetUploadSuccessState } from "@/components/datasets/ingestion/datasetUploadSuccessState";
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
  const { isUploading, errorMessage, uploadResult } = useDatasetIngestionState();

  useEffect(() => {
    if (!uploadResult) {
      return undefined;
    }

    const redirectTimeout = window.setTimeout(() => {
      router.push(
        `/datasets/${uploadResult.dataset.id}?versionId=${uploadResult.datasetVersionId}`,
      );
    }, 1200);

    return () => window.clearTimeout(redirectTimeout);
  }, [router, uploadResult]);

  const handleSubmit = async (values: UploadRawDatasetFormValues) => {
    try {
      await uploadRawDataset(values);
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
        <div className={styles.mainColumn}>
          <DatasetUploadForm
            isSubmitting={isUploading}
            errorMessage={errorMessage}
            onSubmit={handleSubmit}
          />

          {uploadResult ? (
            <DatasetUploadSuccessState uploadResult={uploadResult} />
          ) : null}
        </div>

        <div className={styles.sideColumn}>
          <Card className={styles.helperCard}>
            <Title level={4} className={styles.helperTitle}>
              Ingestion Notes
            </Title>
            <Paragraph className={styles.helperText}>
              The backend accepts CSV and JSON uploads only.
            </Paragraph>
            <Paragraph className={styles.helperText}>
              Every successful upload creates a dataset, an initial raw version, stored raw file metadata, extracted column/schema records, and a profiling snapshot.
            </Paragraph>
            <Paragraph className={styles.helperText}>
              Validation and profiling outcomes come directly from the backend so the UI stays aligned with the canonical rules.
            </Paragraph>
          </Card>
        </div>
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
