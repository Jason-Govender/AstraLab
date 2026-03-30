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
    margin-bottom: 14px !important;
    color: ${token.colorText} !important;
  `,

  select: css`
    width: 100%;
  `,

  list: css`
    display: flex;
    flex-direction: column;
    gap: 14px;
    width: 100%;
    margin-top: 18px;
  `,

  listItem: css`
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 12px 14px;
    border: 1px solid rgba(31, 45, 72, 0.9);
    border-radius: 16px;
    background: rgba(11, 19, 35, 0.7);
  `,

  meta: css`
    color: ${token.colorTextSecondary};
  `,
}));
