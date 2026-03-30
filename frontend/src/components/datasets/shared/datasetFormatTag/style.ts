"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  tag: css`
    &.ant-tag {
      margin-inline-end: 0;
      border-radius: 999px;
      color: ${token.colorText};
      font-weight: 600;
      background: rgba(47, 140, 255, 0.12);
      border-color: rgba(47, 140, 255, 0.28);
    }
  `,
}));
