"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  layout: css`
    display: grid;
    grid-template-columns: minmax(300px, 0.95fr) minmax(0, 1.35fr);
    gap: 24px;

    @media (max-width: 1180px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  sideColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  mainColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,
}));
