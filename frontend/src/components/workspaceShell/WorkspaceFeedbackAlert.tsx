"use client";

import type { ReactNode } from "react";
import { Alert, Button } from "antd";
import type { AlertProps } from "antd";
import { useWorkspaceFeedbackStyles } from "./workspaceFeedbackStyle";

interface WorkspaceFeedbackAlertProps {
  type: NonNullable<AlertProps["type"]>;
  title: string;
  description?: ReactNode;
  className?: string;
  closable?: boolean;
  onClose?: () => void;
  action?: ReactNode;
  actionLabel?: string;
  onAction?: () => void;
}

export const WorkspaceFeedbackAlert = ({
  type,
  title,
  description,
  className,
  closable = false,
  onClose,
  action,
  actionLabel,
  onAction,
}: WorkspaceFeedbackAlertProps) => {
  const { styles, cx } = useWorkspaceFeedbackStyles();

  const resolvedAction =
    action ||
    (actionLabel && onAction ? (
      <Button
        size="small"
        type={type === "success" || type === "info" ? "primary" : "default"}
        onClick={onAction}
      >
        {actionLabel}
      </Button>
    ) : undefined);

  return (
    <Alert
      type={type}
      showIcon
      message={title}
      description={description}
      closable={closable}
      onClose={onClose}
      action={resolvedAction}
      className={cx(styles.feedbackAlert, className)}
    />
  );
};
