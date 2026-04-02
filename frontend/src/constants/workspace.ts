import type { WorkspaceNavItem } from "@/types/workspace";
import { DASHBOARD_ROUTE } from "./auth";

export const WORKSPACE_NAVIGATION: WorkspaceNavItem[] = [
  {
    key: "dashboard",
    label: "Dashboard",
    href: DASHBOARD_ROUTE,
    icon: "dashboard",
    isAvailable: true,
  },
  {
    key: "datasets",
    label: "Datasets",
    href: "/datasets",
    icon: "datasets",
    isAvailable: true,
  },
  {
    key: "assistant",
    label: "AI Assistant",
    href: "/ai-assistant",
    icon: "assistant",
    isAvailable: false,
  },
  {
    key: "ml-workspace",
    label: "ML Workspace",
    href: "/ml-workspace",
    icon: "mlWorkspace",
    isAvailable: true,
  },
  {
    key: "reports",
    label: "Reports",
    href: "/reports",
    icon: "reports",
    isAvailable: true,
  },
  {
    key: "settings",
    label: "Settings",
    href: "/settings",
    icon: "settings",
    isAvailable: false,
  },
];
