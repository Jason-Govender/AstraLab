"use client";

import type { ReactNode } from "react";
import { Card, Empty } from "antd";
import { useStyles } from "./style";

interface DatasetEmptyStateProps {
  title: string;
  description: string;
  action?: ReactNode;
}

export const DatasetEmptyState = ({
  title,
  description,
  action,
}: DatasetEmptyStateProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Empty
        className={styles.empty}
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description={
          <>
            <strong>{title}</strong>
            <br />
            {description}
          </>
        }
      >
        {action ?? null}
      </Empty>
    </Card>
  );
};
