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
    margin-bottom: 6px !important;
    color: ${token.colorText} !important;
  `,

  description: css`
    color: ${token.colorTextSecondary} !important;
  `,

  metaRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-bottom: 18px;
  `,
}));
