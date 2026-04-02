"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { css, token } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    actionGroup: surfaces.actionGroup,
    contentStack: surfaces.stack,
    selectionCard: surfaces.selectionCard,

    selectionGrid: css`
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 18px;

      @media (max-width: 900px) {
        grid-template-columns: minmax(0, 1fr);
      }
    `,

    selectorField: surfaces.selectorField,
    selectionMeta: css`
      margin-top: 16px;
      color: ${token.colorTextSecondary};
    `,

    selectInput: surfaces.selectInput,

    loadingCard: surfaces.loadingCard,
  };
});
