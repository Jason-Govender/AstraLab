"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  title: css`
    color: ${token.colorTextHeading} !important;
  `,

  helperText: css`
    color: ${token.colorTextSecondary};
  `,

  stack: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,

  table: css`
    .ant-table {
      background: transparent;
    }
  `,
}));
