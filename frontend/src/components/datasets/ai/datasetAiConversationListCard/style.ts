"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.18);
      box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
    }
  `,

  list: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  item: css`
    appearance: none;
    display: flex;
    flex-direction: column;
    gap: 8px;
    width: 100%;
    padding: 16px;
    border: 1px solid rgba(40, 58, 92, 0.16);
    border-radius: 18px;
    background: rgba(15, 23, 42, 0.03);
    cursor: pointer;
    text-align: left;
    transition:
      border-color 0.2s ease,
      background 0.2s ease;

    &:hover {
      border-color: ${token.colorPrimaryBorder};
      background: rgba(15, 23, 42, 0.05);
    }
  `,

  activeItem: css`
    border-color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
  `,

  itemHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  `,

  preview: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
    white-space: pre-wrap;
  `,

  metaText: css`
    color: ${token.colorTextSecondary};
  `,
}));
