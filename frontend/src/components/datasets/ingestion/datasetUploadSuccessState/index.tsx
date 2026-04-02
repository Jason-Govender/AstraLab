"use client";

import { DatasetProfileColumnsTable } from "@/components/datasets/shared/datasetProfileColumnsTable";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { WorkspaceFeedbackAlert } from "@/components/workspaceShell/WorkspaceFeedbackAlert";
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
      <WorkspaceFeedbackAlert
        type="success"
        title="Action completed"
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
