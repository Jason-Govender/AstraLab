"use client";

import { Card } from "antd";
import { useWorkspaceFeedbackStyles } from "./workspaceFeedbackStyle";

interface WorkspaceLoadingCardProps {
  className?: string;
  minHeight?: number;
}

export const WorkspaceLoadingCard = ({
  className,
  minHeight,
}: WorkspaceLoadingCardProps) => {
  const { styles, cx } = useWorkspaceFeedbackStyles();

  return (
    <Card
      loading
      className={cx(styles.loadingCard, className)}
      style={minHeight ? { minHeight } : undefined}
    />
  );
};
