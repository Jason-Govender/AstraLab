"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      margin-bottom: 24px;
      border-color: rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  form: css`
    display: grid;
    grid-template-columns: minmax(0, 2fr) minmax(220px, 1fr) minmax(180px, 1fr) auto auto;
    gap: 16px;

    @media (max-width: 1080px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (max-width: 720px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  field: css`
    margin-bottom: 0 !important;
  `,

  actions: css`
    display: flex;
    align-items: flex-end;
    gap: 12px;

    @media (max-width: 720px) {
      align-items: stretch;
      flex-direction: column;
    }
  `,

  button: css`
    &.ant-btn {
      min-width: 120px;
    }
  `,

  helperText: css`
    margin-top: 14px;
    color: ${token.colorTextSecondary};
  `,
}));
