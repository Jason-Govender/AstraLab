"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.18);
      box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
    }
  `,

  helperText: css`
    margin-bottom: 20px !important;
    color: ${token.colorTextSecondary};
  `,

  actions: css`
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
  `,
}));
