"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  pageGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1.7fr) minmax(280px, 0.9fr);
    gap: 24px;

    @media (max-width: 1080px) {
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

  helperCard: css`
    &.ant-card {
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  helperTitle: css`
    color: ${token.colorText} !important;
  `,

  helperText: css`
    color: ${token.colorTextSecondary} !important;
  `,
}));
