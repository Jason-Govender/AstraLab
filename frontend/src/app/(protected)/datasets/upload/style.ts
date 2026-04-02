"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    actionGroup: surfaces.actionGroup,
    pageGrid: css`
      display: grid;
      grid-template-columns: minmax(0, 1.7fr) minmax(300px, 0.92fr);
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

    helperCard: surfaces.sectionCard,

    helperTitle: css`
      margin: 0 !important;
      color: ${token.colorText} !important;
    `,

    helperText: css`
      color: ${token.colorTextSecondary} !important;
      line-height: 1.7;
    `,

    helperList: css`
      margin: 0;
      padding-left: 18px;
      color: ${token.colorTextSecondary};
      display: flex;
      flex-direction: column;
      gap: 10px;
    `,
  };
});
