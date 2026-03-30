"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      height: 100%;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  title: css`
    margin-bottom: 18px !important;
    color: ${token.colorText} !important;
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

    .ant-table-tbody > tr > td {
      background: rgba(13, 22, 39, 0.96);
      color: ${token.colorText};
      border-bottom-color: rgba(23, 34, 55, 0.9);
    }

    .ant-table-tbody > tr:last-child > td {
      border-bottom: none;
    }
  `,

  subtleValue: css`
    color: ${token.colorTextSecondary};
  `,
}));
