"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCard,

    titleHelper: css`
      margin: 0 0 4px !important;
      color: ${token.colorTextSecondary} !important;
    `,

    thread: css`
      display: flex;
      flex-direction: column;
      gap: 18px;
    `,

    turn: css`
      display: flex;
      flex-direction: column;
      gap: 12px;
    `,

    bubble: css`
      padding: 16px 18px;
      border-radius: 18px;
      border: 1px solid rgba(43, 62, 99, 0.9);
    `,

    userBubble: css`
      background: rgba(12, 20, 34, 0.84);
    `,

    assistantBubble: css`
      background:
        radial-gradient(circle at top right, rgba(74, 120, 255, 0.12), transparent 38%),
        rgba(14, 24, 41, 0.95);
      border-color: rgba(74, 120, 255, 0.32);
    `,

    bubbleHeader: css`
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 10px;
    `,

    bubbleText: css`
      margin-bottom: 0 !important;
      line-height: 1.75;
      white-space: pre-wrap;
    `,

    helperText: css`
      margin-bottom: 0 !important;
      color: ${token.colorTextSecondary};
    `,
  };
});
