"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  stack: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,
}));
