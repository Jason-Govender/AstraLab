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
  const { resetUpload, uploadRawDataset } = useDatasetIngestionActions();
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
            onInteraction={resetUpload}
            onSubmit={handleSubmit}
          />

          {uploadResult ? (
            <DatasetUploadSuccessState uploadResult={uploadResult} />
          ) : null}
        </div>

        <div className={styles.sideColumn}>
          <Card className={styles.helperCard}>
            <Title level={4} className={styles.helperTitle}>
              Before you upload
            </Title>
            <Paragraph className={styles.helperText}>
              Use a clean CSV or JSON file with stable headers if you want profiling, exploration, and ML setup to feel predictable on the first pass.
            </Paragraph>
            <Paragraph className={styles.helperText}>
              The upload button is the moment the file is actually sent. Selecting a file only prepares it locally so you can review what will be submitted first.
            </Paragraph>
            <ul className={styles.helperList}>
              <li>Every successful upload creates the dataset, the first raw version, extracted schema records, and a profiling snapshot.</li>
              <li>Validation and profiling come from the backend, so this screen stays aligned with the platform’s canonical ingestion rules.</li>
              <li>You’ll land on dataset details automatically once the new version is ready.</li>
            </ul>
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
