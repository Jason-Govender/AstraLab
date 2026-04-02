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
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 18px;

      @media (max-width: 1080px) {
        grid-template-columns: minmax(0, 1fr);
      }
    `,

    selectorField: surfaces.selectorField,
    selectInput: surfaces.selectInput,

    selectionMeta: css`
      margin-top: 16px;
      color: ${token.colorTextSecondary};
    `,

    pageGrid: css`
      display: grid;
      grid-template-columns: minmax(0, 1.55fr) minmax(300px, 0.95fr);
      gap: 24px;

      @media (max-width: 1180px) {
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
