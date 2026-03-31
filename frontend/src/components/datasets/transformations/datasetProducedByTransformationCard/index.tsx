"use client";

import { Button, Card, Descriptions, Empty, Typography } from "antd";
import { useRouter } from "next/navigation";
import type { DatasetTransformation } from "@/types/datasets";
import { DatasetTransformationTypeTag } from "@/components/datasets/shared/datasetTransformationTypeTag";
import { formatDateTime } from "@/utils/datasets";
import { getDatasetTransformationSummaryLines } from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Title } = Typography;

interface DatasetProducedByTransformationCardProps {
  datasetId: number;
  transformation?: DatasetTransformation | null;
}

export const DatasetProducedByTransformationCard = ({
  datasetId,
  transformation,
}: DatasetProducedByTransformationCardProps) => {
  const { styles } = useStyles();
  const router = useRouter();

  return (
    <Card className={styles.card}>
      <Title level={4} className={styles.title}>
        Producing Transformation
      </Title>

      {!transformation ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="Transformation metadata is not available for this processed version."
        />
      ) : (
        <>
          <Descriptions
            size="small"
            column={1}
            items={[
              {
                key: "type",
                label: "Type",
                children: (
                  <DatasetTransformationTypeTag
                    transformationType={transformation.transformationType}
                  />
                ),
              },
              {
                key: "execution-order",
                label: "Execution Order",
                children: transformation.executionOrder,
              },
              {
                key: "executed-at",
                label: "Executed",
                children: formatDateTime(transformation.executedAt),
              },
            ]}
          />

          <div>
            <Button
              size="small"
              onClick={() =>
                router.push(
                  `/datasets/${datasetId}?versionId=${transformation.sourceDatasetVersionId}`,
                )
              }
            >
              Open source version
            </Button>
          </div>

          {getDatasetTransformationSummaryLines(transformation).length > 0 ? (
            <ul className={styles.summaryList}>
              {getDatasetTransformationSummaryLines(transformation).map(
                (summaryLine) => (
                  <li key={summaryLine}>{summaryLine}</li>
                ),
              )}
            </ul>
          ) : null}
        </>
      )}
    </Card>
  );
};
