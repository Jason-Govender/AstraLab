"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCard,

    helperText: css`
      margin: 0 0 4px !important;
      color: ${token.colorTextSecondary} !important;
    `,

    list: css`
      display: flex;
      flex-direction: column;
      gap: 12px;
    `,

    item: css`
      appearance: none;
      display: flex;
      flex-direction: column;
      gap: 8px;
      width: 100%;
      padding: 16px;
      border: 1px solid rgba(43, 62, 99, 0.9);
      border-radius: 18px;
      background: rgba(12, 20, 34, 0.84);
      cursor: pointer;
      text-align: left;
      transition:
        border-color 0.2s ease,
        background 0.2s ease,
        transform 0.2s ease;

      &:hover {
        transform: translateY(-1px);
        border-color: rgba(120, 152, 221, 0.7);
        background: rgba(15, 25, 42, 0.92);
      }
    `,

    activeItem: css`
      border-color: rgba(74, 120, 255, 0.92);
      background:
        radial-gradient(circle at top right, rgba(74, 120, 255, 0.14), transparent 35%),
        rgba(14, 24, 41, 0.96);
      box-shadow: inset 0 0 0 1px rgba(74, 120, 255, 0.28);
    `,

    itemHeader: css`
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    `,

    preview: css`
      margin-bottom: 0 !important;
      color: ${token.colorTextSecondary};
      line-height: 1.65;
      white-space: pre-wrap;
    `,

    metaText: css`
      color: ${token.colorTextSecondary};
    `,
  };
});
