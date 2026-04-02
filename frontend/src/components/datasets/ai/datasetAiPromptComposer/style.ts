"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { css, token } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCard,

    helperText: css`
      margin-bottom: 16px !important;
      color: ${token.colorTextSecondary};
      line-height: 1.7;
    `,

    actions: css`
      display: flex;
      justify-content: flex-end;
      margin-top: 16px;
    `,
  };
});
