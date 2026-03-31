"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  aggregationList: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  aggregationCard: css`
    padding: 14px;
    border: 1px solid ${token.colorBorderSecondary};
    border-radius: 16px;
    background: ${token.colorFillAlter};
  `,

  aggregationGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1fr) minmax(0, 1fr) minmax(0, 1fr) auto;
    gap: 12px;
    align-items: center;

    @media (max-width: 980px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,
}));
