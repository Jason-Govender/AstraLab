"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  stack: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    flex-wrap: wrap;
  `,

  title: css`
    margin-bottom: 6px !important;
    color: ${token.colorTextHeading} !important;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  tabStack: css`
    display: flex;
    flex-direction: column;
    gap: 20px;
  `,

  controlsRow: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  select: css`
    min-width: 240px;
  `,

  multiSelect: css`
    min-width: 320px;
    max-width: 520px;
  `,

  numberInput: css`
    min-width: 150px;
  `,
}));
