"use client";

import { Button, Card, Typography } from "antd";
import { useRouter } from "next/navigation";
import { buildMlWorkspaceHref } from "@/utils/ml";
import { useStyles } from "./style";

const { Paragraph, Title } = Typography;

interface MlWorkspaceLauncherCardProps {
  datasetId: number;
  datasetVersionId?: number;
}

export const MlWorkspaceLauncherCard = ({
  datasetId,
  datasetVersionId,
}: MlWorkspaceLauncherCardProps) => {
  const router = useRouter();
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Title level={3} className={styles.title}>
        Machine Learning Workspace
      </Title>

      <Paragraph className={styles.description}>
        Open the dedicated ML workspace to choose a dataset version, launch
        experiments, and review results without leaving the workspace flow.
      </Paragraph>

      <Button
        type="primary"
        size="large"
        onClick={() =>
          router.push(buildMlWorkspaceHref(datasetId, datasetVersionId))
        }
      >
        Open ML workspace
      </Button>
    </Card>
  );
};
