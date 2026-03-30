"use client";

import type { ReactNode } from "react";
import { Alert, Card, Space } from "antd";
import { useStyles } from "./style";

interface DatasetErrorStateProps {
  title: string;
  message: string;
  action?: ReactNode;
}

export const DatasetErrorState = ({
  title,
  message,
  action,
}: DatasetErrorStateProps) => {
  const { styles } = useStyles();

  return (
    <Card className={styles.card}>
      <Space direction="vertical" size="large" className={styles.content}>
        <Alert type="error" showIcon message={title} description={message} />
        {action ?? null}
      </Space>
    </Card>
  );
};
