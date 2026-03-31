"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 18px;
      border-color: rgba(40, 58, 92, 0.82);
      background: rgba(11, 19, 36, 0.7);
      box-shadow: none;
    }
  `,

  stack: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,

  field: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  label: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextHeading};
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  fullWidth: css`
    width: 100%;
  `,
}));
