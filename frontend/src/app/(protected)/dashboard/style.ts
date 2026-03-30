"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  uploadCard: css`
    &.ant-card {
      margin-bottom: 22px;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }

    .ant-card-body {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 20px;
      padding: 28px 26px;

      @media (max-width: 768px) {
        flex-direction: column;
        align-items: flex-start;
      }
    }
  `,

  uploadTitle: css`
    margin: 0 0 6px !important;
    color: ${token.colorText} !important;
  `,

  uploadDescription: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 16px;
  `,

  metricCard: css`
    &.ant-card {
      height: 100%;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 22px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }

    .ant-card-body {
      padding: 20px 22px;
    }
  `,

  metricLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 14px;
  `,

  metricValue: css`
    margin: 10px 0 0 !important;
    color: ${token.colorText} !important;
    font-size: 34px !important;
    line-height: 1.1 !important;
  `,

  tableCard: css`
    &.ant-card {
      margin-top: 24px;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }

    .ant-card-body {
      padding: 22px 22px 16px;
    }
  `,

  tableTitle: css`
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

  healthGood: css`
    color: ${token.colorSuccess};
    font-weight: 700;
  `,

  healthWarning: css`
    color: ${token.colorWarning};
    font-weight: 700;
  `,

  healthError: css`
    color: ${token.colorError};
    font-weight: 700;
  `,
}));
