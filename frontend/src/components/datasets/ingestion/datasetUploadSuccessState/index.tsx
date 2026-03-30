"use client";

import { Alert } from "antd";
import { DatasetProfileColumnsTable } from "@/components/datasets/shared/datasetProfileColumnsTable";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import type { UploadedRawDatasetResult } from "@/types/datasets";
import {
  buildDatasetColumnInsights,
  getUploadProfileOverview,
} from "@/utils/datasets";
import { useStyles } from "./style";

interface DatasetUploadSuccessStateProps {
  uploadResult: UploadedRawDatasetResult;
}

export const DatasetUploadSuccessState = ({
  uploadResult,
}: DatasetUploadSuccessStateProps) => {
  const { styles } = useStyles();
  const uploadProfileOverview = getUploadProfileOverview(uploadResult);
  const initialColumnInsights = buildDatasetColumnInsights(
    uploadResult.columns,
    uploadResult.columnProfiles,
  );

  return (
    <div className={styles.stack}>
      <Alert
        type="success"
        showIcon
        message="Upload complete"
        description="The dataset has been profiled successfully. Redirecting to the dataset details page now."
      />

      <DatasetProfileSummaryCard
        profile={uploadProfileOverview}
        title="Immediate Quality Snapshot"
        helperText="This summary comes directly from the upload response returned by the backend profiling pipeline."
      />

      <DatasetProfileColumnsTable
        columns={initialColumnInsights}
        title="Initial Column Insights"
      />
    </div>
  );
};
