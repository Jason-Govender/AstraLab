"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.18);
      background: ${token.colorBgContainer};
      box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
    }
  `,

  header: css`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 20px;

    @media (max-width: 820px) {
      flex-direction: column;
    }
  `,

  title: css`
    margin-bottom: 6px !important;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  scoreSection: css`
    display: grid;
    grid-template-columns: minmax(180px, 220px) minmax(0, 1fr);
    gap: 20px;
    align-items: center;
    margin-bottom: 24px;

    @media (max-width: 880px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  progressWrap: css`
    display: flex;
    justify-content: center;
  `,

  scoreMeta: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  scoreValue: css`
    margin-bottom: 0 !important;
  `,

  timestamp: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,

  metricsGrid: css`
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;

    @media (max-width: 1080px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (max-width: 640px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  metricCard: css`
    border: 1px solid ${token.colorBorderSecondary};
    border-radius: 18px;
    padding: 14px 16px;
    background: ${token.colorFillAlter};
  `,

  metricLabel: css`
    display: block;
    margin-bottom: 8px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
    letter-spacing: 0.04em;
    text-transform: uppercase;
  `,

  metricValue: css`
    display: block;
    color: ${token.colorText};
    font-size: 22px;
    font-weight: 700;
    line-height: 1.2;
  `,
}));
