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

  aggregationList: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,

  aggregationCard: css`
    border: 1px solid rgba(40, 58, 92, 0.82);
    border-radius: 16px;
    padding: 16px;
    background: rgba(7, 12, 23, 0.58);
  `,

  aggregationHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 16px;
    flex-wrap: wrap;
  `,

  aggregationTitle: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextHeading};
    font-weight: 600;
  `,

  aggregationGrid: css`
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16px;

    @media (max-width: 980px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  actionsRow: css`
    display: flex;
    justify-content: flex-start;
  `,
}));
