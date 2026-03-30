import type { ReactNode } from "react";

export type WorkspaceIconKey =
  | "dashboard"
  | "datasets"
  | "analysis"
  | "assistant"
  | "mlWorkspace"
  | "reports"
  | "settings";

export interface WorkspaceNavItem {
  key: string;
  label: string;
  href: string;
  icon: WorkspaceIconKey;
  isAvailable: boolean;
}

export interface WorkspaceShellProps {
  children: ReactNode;
}

export interface WorkspacePageHeaderProps {
  title: string;
  description: string;
  actions?: ReactNode;
}
