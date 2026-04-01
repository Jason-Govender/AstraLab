"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  card: css`
    border: 1px solid rgba(86, 120, 179, 0.28);
    border-radius: 24px;
    background:
      radial-gradient(circle at top left, rgba(78, 115, 255, 0.16), transparent 40%),
      radial-gradient(circle at bottom right, rgba(14, 193, 165, 0.12), transparent 35%),
      rgba(11, 18, 34, 0.94);
    box-shadow: none;
  `,

  title: css`
    margin-bottom: 8px !important;
    color: ${token.colorTextHeading};
  `,

  description: css`
    margin-bottom: 18px !important;
    color: ${token.colorTextSecondary};
  `,
}));
