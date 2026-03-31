"use client";

import { Button, Card, Empty, Timeline, Typography } from "antd";
import { useRouter } from "next/navigation";
import {
  DatasetVersionType,
  type DatasetTransformationHistory,
} from "@/types/datasets";
import { DatasetTransformationTypeTag } from "@/components/datasets/shared/datasetTransformationTypeTag";
import {
  formatDateTime,
  formatRelativeTime,
  getDatasetVersionTypeLabel,
} from "@/utils/datasets";
import { getDatasetTransformationSummaryLines } from "@/utils/datasetTransformations";
import { useStyles } from "./style";

const { Paragraph, Text, Title } = Typography;

interface DatasetTransformationHistoryCardProps {
  datasetId: number;
  history?: DatasetTransformationHistory;
  isLoading?: boolean;
}

export const DatasetTransformationHistoryCard = ({
  datasetId,
  history,
  isLoading = false,
}: DatasetTransformationHistoryCardProps) => {
  const { styles } = useStyles();
  const router = useRouter();

  return (
    <Card loading={isLoading} className={styles.card}>
      <Title level={4} className={styles.title}>
        Transformation History
      </Title>
      <Paragraph className={styles.helperText}>
        Review the processed versions that were created from this dataset and inspect the transformation pipeline outputs.
      </Paragraph>

      {!history || history.items.length === 0 ? (
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="No transformations have been run for this dataset yet."
        />
      ) : (
        <Timeline
          items={history.items.map((item) => ({
            children: (
              <div>
                <DatasetTransformationTypeTag
                  transformationType={item.transformation.transformationType}
                />

                <Paragraph>
                  <Text strong>
                    Executed {formatRelativeTime(item.transformation.executedAt)}
                  </Text>
                  <br />
                  <Text type="secondary">
                    {formatDateTime(item.transformation.executedAt)}
                  </Text>
                </Paragraph>

                <div className={styles.versionRow}>
                  <Button
                    size="small"
                    onClick={() => {
                      if (
                        item.sourceVersion.versionType ===
                        DatasetVersionType.Processed
                      ) {
                        router.push(`/datasets/versions/${item.sourceVersion.id}`);
                        return;
                      }

                      router.push(`/datasets/${datasetId}?versionId=${item.sourceVersion.id}`);
                    }}
                  >
                    Source {`v${item.sourceVersion.versionNumber}`} ·{" "}
                    {getDatasetVersionTypeLabel(item.sourceVersion.versionType)}
                  </Button>

                  {item.resultVersion ? (
                    <Button
                      size="small"
                      type={
                        history.currentVersionId === item.resultVersion.id
                          ? "primary"
                          : "default"
                      }
                      onClick={() =>
                        router.push(`/datasets/versions/${item.resultVersion?.id}`)
                      }
                    >
                      Result {`v${item.resultVersion.versionNumber}`} ·{" "}
                      {getDatasetVersionTypeLabel(item.resultVersion.versionType)}
                    </Button>
                  ) : null}
                </div>

                {getDatasetTransformationSummaryLines(item.transformation).length > 0 ? (
                  <ul className={styles.summaryList}>
                    {getDatasetTransformationSummaryLines(item.transformation).map(
                      (summaryLine) => (
                        <li key={summaryLine}>{summaryLine}</li>
                      ),
                    )}
                  </ul>
                ) : null}
              </div>
            ),
          }))}
        />
      )}
    </Card>
  );
};
