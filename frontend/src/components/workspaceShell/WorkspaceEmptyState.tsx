"use client";

import type { ReactNode } from "react";
import { Card, Empty, Typography } from "antd";
import { useWorkspaceFeedbackStyles } from "./workspaceFeedbackStyle";

const { Paragraph, Text } = Typography;

interface WorkspaceEmptyStateProps {
  title?: string;
  description: string;
  action?: ReactNode;
  className?: string;
  isContained?: boolean;
}

export const WorkspaceEmptyState = ({
  title = "Nothing here yet",
  description,
  action,
  className,
  isContained = true,
}: WorkspaceEmptyStateProps) => {
  const { styles, cx } = useWorkspaceFeedbackStyles();

  const emptyContent = (
    <>
      <Empty
        className={styles.compactEmpty}
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description={
          <>
            <Text strong>{title}</Text>
            <Paragraph>{description}</Paragraph>
          </>
        }
      />
      {action ? <div className={styles.actionRow}>{action}</div> : null}
    </>
  );

  if (!isContained) {
    return <div className={className}>{emptyContent}</div>;
  }

  return <Card className={cx(styles.emptyCard, className)}>{emptyContent}</Card>;
};
