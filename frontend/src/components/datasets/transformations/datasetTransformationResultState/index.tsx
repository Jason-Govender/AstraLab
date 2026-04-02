"use client";

import type { TransformDatasetVersionResult } from "@/types/datasets";
import { DatasetProfileSummaryCard } from "@/components/datasets/shared/datasetProfileSummaryCard";
import { WorkspaceFeedbackAlert } from "@/components/workspaceShell/WorkspaceFeedbackAlert";
import type { DatasetProfileOverview } from "@/utils/datasets";
import { useStyles } from "./style";

interface DatasetTransformationResultStateProps {
  transformResult: TransformDatasetVersionResult;
}

export const DatasetTransformationResultState = ({
  transformResult,
}: DatasetTransformationResultStateProps) => {
  const { styles } = useStyles();
  const profileOverview: DatasetProfileOverview = {
    rowCount: transformResult.finalProfile.rowCount,
    duplicateRowCount: transformResult.finalProfile.duplicateRowCount,
    dataHealthScore: transformResult.finalProfile.dataHealthScore,
    totalNullCount: transformResult.finalProfile.totalNullCount,
    overallNullPercentage: transformResult.finalProfile.overallNullPercentage,
    totalAnomalyCount: transformResult.finalProfile.totalAnomalyCount,
    overallAnomalyPercentage:
      transformResult.finalProfile.overallAnomalyPercentage,
    creationTime: transformResult.finalProfile.creationTime,
  };

  return (
    <div className={styles.stack}>
      <WorkspaceFeedbackAlert
        type="success"
        title="Action completed"
        description="The backend created a processed version and finished profiling it. Redirecting to the processed version details page now."
      />

      <DatasetProfileSummaryCard
        profile={profileOverview}
        title="Final Processed Version Profile"
        helperText="This summary is returned by the backend after the transformation pipeline finishes and the final processed version is profiled."
      />
    </div>
  );
};
