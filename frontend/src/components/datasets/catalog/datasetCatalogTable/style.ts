"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.sectionCardMuted,

    titleRow: css`
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      margin-bottom: 18px;
    `,

    title: css`
      margin: 0 !important;
      color: ${token.colorText} !important;
    `,

    helperText: css`
      color: ${token.colorTextSecondary};
    `,

    nameBlock: css`
      display: flex;
      flex-direction: column;
      gap: 4px;
    `,

    description: css`
      color: ${token.colorTextSecondary};
      line-height: 1.6;
    `,

    table: css`
      .ant-table {
        background: transparent;
      }

      .ant-table-container {
        border-radius: 18px;
        overflow: hidden;
        border: 1px solid rgba(31, 45, 72, 0.9);
      }

      .ant-table-thead > tr > th {
        background: rgba(11, 19, 35, 0.95);
        color: ${token.colorTextSecondary};
        border-bottom-color: rgba(31, 45, 72, 0.9);
        font-size: 12px;
        font-weight: 700;
        letter-spacing: 0.04em;
        text-transform: uppercase;
      }

      .ant-table-tbody > tr {
        cursor: pointer;
        transition: background 0.2s ease;
      }

      .ant-table-tbody > tr > td {
        background: rgba(13, 22, 39, 0.96);
        color: ${token.colorText};
        border-bottom-color: rgba(23, 34, 55, 0.9);
      }

      .ant-table-tbody > tr:hover > td {
        background: rgba(18, 29, 49, 0.98) !important;
      }
    `,
  };
});
