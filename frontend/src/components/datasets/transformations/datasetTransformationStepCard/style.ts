"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
    margin-bottom: 20px;
    flex-wrap: wrap;
  `,

  headingBlock: css`
    display: flex;
    flex-direction: column;
    gap: 6px;
    min-width: 0;
  `,

  stepTag: css`
    width: fit-content;
    border-radius: 999px;
    padding-inline: 10px;
  `,

  title: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextHeading} !important;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  controls: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  typeSelect: css`
    width: 220px;
  `,
}));
