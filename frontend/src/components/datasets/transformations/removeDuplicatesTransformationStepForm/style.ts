"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 18px;
      border-color: rgba(40, 58, 92, 0.82);
      background: rgba(11, 19, 36, 0.7);
      box-shadow: none;
    }
  `,

  helperText: css`
    color: ${token.colorTextSecondary};
    margin-bottom: 16px !important;
  `,

  group: css`
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    gap: 12px;
  `,
}));
