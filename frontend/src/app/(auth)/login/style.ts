"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    padding: 28px;
    background:
      radial-gradient(circle at top left, rgba(47, 140, 255, 0.16), transparent 28%),
      radial-gradient(circle at bottom right, rgba(255, 122, 26, 0.12), transparent 26%),
      linear-gradient(180deg, #09111f 0%, #070d19 100%);

    @media (max-width: 768px) {
      padding: 16px;
    }
  `,

  shell: css`
    width: 100%;
    max-width: 1240px;
    margin: 0 auto;
    padding: 42px 48px;
    border: 1px solid rgba(42, 58, 93, 0.72);
    border-radius: 32px;
    background: linear-gradient(180deg, rgba(13, 21, 38, 0.98), rgba(10, 16, 30, 0.98));
    box-shadow: 0 32px 96px rgba(0, 0, 0, 0.34);

    @media (max-width: 992px) {
      padding: 28px;
    }

    @media (max-width: 576px) {
      padding: 22px 18px;
      border-radius: 24px;
    }
  `,

  hero: css`
    max-width: 560px;
  `,

  brand: css`
    display: inline-block;
    margin-bottom: 28px;
    color: ${token.colorText};
    font-size: 26px;
    font-weight: 700;
    letter-spacing: -0.03em;
  `,

  heroTitle: css`
    margin: 0 0 20px !important;
    color: ${token.colorText} !important;
    font-size: clamp(40px, 5vw, 58px) !important;
    font-weight: 700 !important;
    line-height: 1.08 !important;
    letter-spacing: -0.05em !important;

    .ant-typography {
      color: inherit;
    }
  `,

  heroAccent: css`
    display: inline;
    color: ${token.colorPrimary};
  `,

  heroDescription: css`
    max-width: 520px;
    margin-bottom: 40px !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 20px;
    line-height: 1.7 !important;
  `,

  metrics: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 18px;
    margin-bottom: 22px;

    @media (max-width: 576px) {
      grid-template-columns: 1fr;
    }
  `,

  metricCard: css`
    &.ant-card {
      min-height: 126px;
      border-radius: 20px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(16, 25, 43, 0.9);
      box-shadow: none;
    }

    .ant-card-body {
      display: flex;
      height: 100%;
      flex-direction: column;
      justify-content: space-between;
      padding: 22px 20px;
    }
  `,

  metricLabel: css`
    color: ${token.colorTextSecondary};
    font-size: 15px;
  `,

  metricValue: css`
    color: ${token.colorText};
    font-size: 46px;
    font-weight: 700;
    line-height: 1;
    letter-spacing: -0.04em;
  `,

  summaryCard: css`
    &.ant-card {
      border-radius: 22px;
      border-color: rgba(40, 58, 92, 0.92);
      background: rgba(16, 25, 43, 0.88);
      box-shadow: none;
    }

    .ant-card-body {
      padding: 26px 24px;
    }
  `,

  summaryTitle: css`
    margin-bottom: 10px !important;
    color: ${token.colorText} !important;
    font-size: 28px !important;
    line-height: 1.3 !important;
    letter-spacing: -0.03em !important;
  `,

  summaryDescription: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 16px;
  `,

  formColumn: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
  `,

  formCard: css`
    &.ant-card {
      width: min(100%, 420px);
      border-radius: 28px;
      border-color: rgba(46, 67, 104, 0.84);
      background: rgba(16, 25, 43, 0.94);
      box-shadow: 0 24px 72px rgba(0, 0, 0, 0.26);
    }

    .ant-card-body {
      padding: 34px 32px 28px;
    }

    @media (max-width: 576px) {
      &.ant-card .ant-card-body {
        padding: 28px 20px 22px;
      }
    }
  `,

  formHeader: css`
    margin-bottom: 28px;
  `,

  formTitle: css`
    margin: 0 0 8px !important;
    color: ${token.colorText} !important;
    font-size: 26px !important;
    line-height: 1.15 !important;
    letter-spacing: -0.04em !important;
  `,

  formSubtitle: css`
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 16px;
  `,

  form: css`
    width: 100%;
  `,

  alert: css`
    margin-bottom: 20px;
    border-color: rgba(255, 95, 95, 0.32) !important;
    background: rgba(79, 20, 20, 0.34) !important;
  `,

  fieldRow: css`
    margin-bottom: 20px;

    .ant-form-item-label > label {
      color: ${token.colorText} !important;
      font-size: 14px;
      font-weight: 600;
    }

    .ant-form-item-explain-error {
      margin-top: 8px;
    }
  `,

  helperText: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,

  input: css`
    &.ant-input,
    &.ant-input-affix-wrapper {
      background: rgba(11, 19, 33, 0.92);
    }

    &.ant-input-affix-wrapper .ant-input {
      background: transparent;
    }
  `,

  actionsRow: css`
    display: flex;
    justify-content: flex-end;
    margin: -4px 0 18px;
  `,

  linkButton: css`
    &.ant-btn-link {
      padding: 0;
      height: auto;
      color: ${token.colorPrimary};
      font-size: 14px;
      font-weight: 600;
    }

    &.ant-btn-link:hover,
    &.ant-btn-link:focus {
      color: #ff984d !important;
    }
  `,

  submitButton: css`
    &.ant-btn {
      margin-top: 4px;
      height: 52px;
      box-shadow: 0 14px 28px rgba(255, 122, 26, 0.18);
    }
  `,

  divider: css`
    margin: 18px 0 14px !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 12px !important;
  `,

  registerRow: css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
    flex-wrap: wrap;
    text-align: center;
  `,

  registerText: css`
    color: ${token.colorTextSecondary};
    font-size: 14px;
  `,

  registerButton: css`
    &.ant-btn-link {
      padding: 0;
      height: auto;
      color: ${token.colorPrimary};
      font-size: 14px;
      font-weight: 700;
    }

    &.ant-btn-link:hover,
    &.ant-btn-link:focus {
      color: #ff984d !important;
    }
  `,

  footnote: css`
    color: rgba(146, 160, 184, 0.72);
    font-size: 12px;
    text-align: center;
  `,
}));
