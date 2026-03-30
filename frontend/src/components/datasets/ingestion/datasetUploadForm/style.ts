"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  form: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  helperText: css`
    color: ${token.colorTextSecondary};
  `,

  fileError: css`
    color: ${token.colorError};
  `,

  submitRow: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,
}));
