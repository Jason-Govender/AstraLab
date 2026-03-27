"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    padding: 28px;
    background:
      radial-gradient(circle at top left, rgba(47, 140, 255, 0.16), transparent 24%),
      linear-gradient(180deg, #09111f 0%, #070d19 100%);
  `,

  shell: css`
    width: 100%;
    max-width: 1240px;
    margin: 0 auto;
    padding: 36px;
    border: 1px solid rgba(42, 58, 93, 0.72);
    border-radius: 32px;
    background: linear-gradient(180deg, rgba(13, 21, 38, 0.98), rgba(10, 16, 30, 0.98));
    box-shadow: 0 32px 96px rgba(0, 0, 0, 0.34);
  `,

  loadingState: css`
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(180deg, #09111f 0%, #070d19 100%);
  `,

  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 24px;
    margin-bottom: 28px;

    @media (max-width: 768px) {
      flex-direction: column;
    }
  `,

  kicker: css`
    color: ${token.colorPrimary};
    font-size: 14px;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  `,

  title: css`
    margin: 10px 0 8px !important;
    color: ${token.colorText} !important;
    font-size: clamp(34px, 4vw, 48px) !important;
    letter-spacing: -0.04em !important;
  `,

  description: css`
    max-width: 720px;
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 18px;
  `,

  logoutButton: css`
    &.ant-btn {
      min-width: 140px;
    }
  `,

  infoCard: css`
    &.ant-card {
      height: 100%;
      border-radius: 22px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(16, 25, 43, 0.9);
      box-shadow: none;
    }

    .ant-card-body {
      padding: 24px 22px;
    }
  `,

  cardLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 14px;
  `,

  cardValue: css`
    margin: 12px 0 0 !important;
    color: ${token.colorText} !important;
    font-size: 26px !important;
    line-height: 1.2 !important;
  `,

  summaryCard: css`
    &.ant-card {
      margin-top: 24px;
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(16, 25, 43, 0.88);
      box-shadow: none;
    }
  `,

  summaryTitle: css`
    color: ${token.colorText} !important;
    margin-bottom: 8px !important;
  `,

  summaryText: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 16px;
  `,
}));
