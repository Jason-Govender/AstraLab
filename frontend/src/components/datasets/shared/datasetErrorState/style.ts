"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  card: css`
    &.ant-card {
      border-color: rgba(255, 95, 95, 0.2);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  content: css`
    width: 100%;
  `,
}));
