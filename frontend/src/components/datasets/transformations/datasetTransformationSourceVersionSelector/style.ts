"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.18);
      background: ${token.colorBgContainer};
      box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
    }
  `,

  title: css`
    margin-bottom: 8px !important;
  `,

  helperText: css`
    margin-bottom: 16px !important;
    color: ${token.colorTextSecondary};
  `,

  select: css`
    width: 100%;
  `,

  list: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-top: 18px;
  `,

  listItem: css`
    padding: 12px 14px;
    border: 1px solid ${token.colorBorderSecondary};
    border-radius: 16px;
    background: ${token.colorFillAlter};
  `,

  meta: css`
    margin-top: 6px !important;
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,
}));
