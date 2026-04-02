"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    actionGroup: surfaces.actionGroup,
    stackedContent: surfaces.stack,
    loadingCard: surfaces.loadingCard,
  };
});
