"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  pageGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1.55fr) minmax(300px, 0.95fr);
    gap: 24px;

    @media (max-width: 1180px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  mainColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  sideColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  actionGroup: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  loadingCard: css`
    &.ant-card {
      min-height: 280px;
      border-radius: 24px;
    }
  `,
}));
