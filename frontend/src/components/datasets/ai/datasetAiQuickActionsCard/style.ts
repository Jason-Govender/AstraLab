"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCard,

    helperText: css`
      margin-bottom: 20px !important;
      color: ${token.colorTextSecondary};
      line-height: 1.7;
    `,

    actions: css`
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    `,
  };
});
