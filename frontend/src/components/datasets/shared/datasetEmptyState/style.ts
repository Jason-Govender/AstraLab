"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCardMuted,
    empty: surfaces.compactEmpty,
  };
});
