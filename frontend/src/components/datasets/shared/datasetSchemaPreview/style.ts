"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      height: 100%;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  title: css`
    margin-bottom: 18px !important;
    color: ${token.colorText} !important;
  `,

  helperText: css`
    color: ${token.colorTextSecondary};
  `,

  list: css`
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-top: 18px;
  `,

  listItem: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 12px 14px;
    border: 1px solid rgba(31, 45, 72, 0.9);
    border-radius: 16px;
    background: rgba(11, 19, 35, 0.7);
  `,

  listItemName: css`
    color: ${token.colorText};
    font-weight: 600;
  `,

  listItemMeta: css`
    color: ${token.colorTextSecondary};
    font-size: 13px;
  `,
}));
