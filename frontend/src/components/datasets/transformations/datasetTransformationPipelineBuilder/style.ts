"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 28px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 20px;
    margin-bottom: 24px;
    flex-wrap: wrap;
  `,

  title: css`
    margin-bottom: 6px !important;
    color: ${token.colorTextHeading} !important;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
    max-width: 720px;
  `,

  headerActions: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  typeSelect: css`
    width: 220px;
  `,

  alert: css`
    margin-bottom: 20px;
  `,

  steps: css`
    display: flex;
    flex-direction: column;
    gap: 20px;
  `,
}));
