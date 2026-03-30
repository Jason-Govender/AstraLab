"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  actionGroup: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  stackedContent: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  loadingCard: css`
    &.ant-card {
      min-height: 240px;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,
}));
