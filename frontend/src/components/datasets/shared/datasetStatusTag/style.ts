"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  tag: css`
    &.ant-tag {
      margin-inline-end: 0;
      border-radius: 999px;
      font-weight: 600;
    }
  `,
}));
