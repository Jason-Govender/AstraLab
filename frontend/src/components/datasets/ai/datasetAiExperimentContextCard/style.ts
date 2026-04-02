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

    tagWrap: css`
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    `,

    metricList: css`
      display: flex;
      flex-direction: column;
      gap: 10px;
    `,

    metricRow: css`
      display: flex;
      justify-content: space-between;
      gap: 12px;
    `,
  };
});
