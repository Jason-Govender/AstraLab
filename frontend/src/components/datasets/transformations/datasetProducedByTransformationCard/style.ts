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
    margin-bottom: 18px !important;
  `,

  summaryList: css`
    margin: 14px 0 0;
    padding-left: 18px;
    color: ${token.colorTextSecondary};
  `,
}));
