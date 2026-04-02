"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  card: css`
    border-radius: 24px;
    border: 1px solid rgba(118, 145, 191, 0.18);
    background: rgba(17, 27, 48, 0.82);
    box-shadow: none;
  `,

  helperText: css`
    margin-bottom: 16px !important;
    color: ${token.colorTextSecondary};
  `,

  tagWrap: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  `,

  metricList: css`
    display: flex;
    flex-direction: column;
    gap: 10px;
  `,

  metricRow: css`
    display: flex;
    justify-content: space-between;
    gap: 12px;
  `,
}));
