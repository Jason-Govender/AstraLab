"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  actionGroup: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  contentStack: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  selectionCard: css`
    border: 1px solid rgba(86, 120, 179, 0.28);
    border-radius: 24px;
    background:
      radial-gradient(circle at top left, rgba(78, 115, 255, 0.16), transparent 40%),
      radial-gradient(circle at bottom right, rgba(14, 193, 165, 0.12), transparent 35%),
      rgba(11, 18, 34, 0.94);
    box-shadow: none;
  `,

  selectionGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;

    @media (max-width: 900px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  selectionMeta: css`
    margin-top: 16px;
    color: ${token.colorTextSecondary};
  `,

  loadingCard: css`
    &.ant-card {
      min-height: 280px;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,
}));
