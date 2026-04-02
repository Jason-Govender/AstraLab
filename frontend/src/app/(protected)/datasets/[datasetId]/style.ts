"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    actionGroup: surfaces.actionGroup,
    pageGrid: css`
      display: grid;
      grid-template-columns: minmax(0, 1.65fr) minmax(300px, 0.98fr);
      gap: 24px;

      @media (max-width: 1080px) {
        grid-template-columns: minmax(0, 1fr);
      }
    `,
    mainColumn: css`
      display: flex;
      flex-direction: column;
      gap: 24px;
    `,
    sideColumn: css`
      display: flex;
      flex-direction: column;
      gap: 24px;
    `,
    loadingCard: surfaces.loadingCard,
  };
});
