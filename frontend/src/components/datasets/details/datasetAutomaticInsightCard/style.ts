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

  header: css`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 20px;

    @media (max-width: 820px) {
      flex-direction: column;
    }
  `,

  title: css`
    margin-bottom: 6px !important;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  body: css`
    display: flex;
    flex-direction: column;
    gap: 18px;
  `,

  section: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  sectionTitle: css`
    margin-bottom: 0 !important;
  `,

  sectionBody: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
    white-space: pre-wrap;
  `,

  fullText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
    white-space: pre-wrap;
  `,
}));
