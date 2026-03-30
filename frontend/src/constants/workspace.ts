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
    isAvailable: false,
  },
  {
    key: "analysis",
    label: "Analysis",
    href: "/analysis",
    icon: "analysis",
    isAvailable: false,
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
    isAvailable: false,
  },
  {
    key: "reports",
    label: "Reports",
    href: "/reports",
    icon: "reports",
    isAvailable: false,
  },
  {
    key: "settings",
    label: "Settings",
    href: "/settings",
    icon: "settings",
    isAvailable: false,
  },
];
