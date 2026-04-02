"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  pageStack: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  contextCard: css`
    &.ant-card {
      border: 1px solid rgba(74, 120, 255, 0.18);
      border-radius: 28px;
      background:
        radial-gradient(circle at top right, rgba(74, 120, 255, 0.16), transparent 35%),
        linear-gradient(180deg, rgba(14, 22, 38, 0.98), rgba(11, 17, 30, 0.96));
      box-shadow: 0 24px 60px rgba(3, 9, 22, 0.26);
    }

    .ant-card-body {
      display: flex;
      flex-direction: column;
      gap: 22px;
      padding: 28px;
    }
  `,

  contextTopRow: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 20px;

    @media (max-width: 1100px) {
      flex-direction: column;
    }
  `,

  contextHeadline: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  eyebrow: css`
    color: ${token.colorPrimary};
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
  `,

  title: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
  `,

  description: css`
    margin: 0 !important;
    max-width: 760px;
    color: ${token.colorTextSecondary} !important;
    font-size: 15px;
  `,

  metaRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  `,

  actionGroup: css`
    display: flex;
    flex-wrap: wrap;
    justify-content: flex-end;
    gap: 12px;
  `,

  selectorGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 18px;

    @media (max-width: 900px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  selectorField: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  selectInput: css`
    width: 100%;
  `,

  feedbackAlert: css`
    margin-top: 4px;
  `,

  contentGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1.7fr) minmax(320px, 1fr);
    gap: 24px;

    @media (max-width: 1180px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  mainColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  sideColumn: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  card: css`
    &.ant-card {
      border: 1px solid rgba(46, 67, 110, 0.88);
      border-radius: 24px;
      background: rgba(13, 20, 35, 0.96);
      box-shadow: 0 16px 44px rgba(4, 8, 18, 0.18);
    }

    .ant-card-body {
      display: flex;
      flex-direction: column;
      gap: 18px;
      padding: 22px;
    }
  `,

  cardHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 16px;
  `,

  cardTitle: css`
    margin: 0 !important;
    color: ${token.colorText} !important;
  `,

  cardHelper: css`
    margin: 6px 0 0 !important;
    color: ${token.colorTextSecondary} !important;
  `,

  summaryBlock: css`
    border: 1px solid rgba(42, 63, 99, 0.92);
    border-radius: 20px;
    background: rgba(17, 26, 44, 0.86);
    padding: 18px;
  `,

  summaryLabel: css`
    display: block;
    color: ${token.colorTextSecondary};
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.06em;
    text-transform: uppercase;
  `,

  summaryValue: css`
    margin: 10px 0 0 !important;
    color: ${token.colorText} !important;
    font-size: 15px;
    line-height: 1.75;
  `,

  previewFrame: css`
    width: 100%;
    min-height: 520px;
    border: 1px solid rgba(40, 58, 92, 0.92);
    border-radius: 20px;
    background: #ffffff;
  `,

  plainList: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin: 0;
    padding: 0;
    list-style: none;
  `,

  listItem: css`
    border: 1px solid rgba(37, 54, 88, 0.92);
    border-radius: 18px;
    background: rgba(15, 24, 39, 0.86);
    padding: 16px;
  `,

  listItemActive: css`
    border-color: rgba(74, 120, 255, 0.75);
    box-shadow: inset 0 0 0 1px rgba(74, 120, 255, 0.35);
  `,

  listButton: css`
    display: flex;
    width: 100%;
    align-items: flex-start;
    justify-content: space-between;
    gap: 14px;
    border: none;
    background: transparent;
    color: inherit;
    cursor: pointer;
    padding: 0;
    text-align: left;
  `,

  listTitle: css`
    color: ${token.colorText};
    font-size: 15px;
    font-weight: 600;
  `,

  listMeta: css`
    margin-top: 8px;
    color: ${token.colorTextSecondary};
    font-size: 13px;
  `,

  listBody: css`
    margin-top: 10px !important;
    color: ${token.colorTextSecondary} !important;
    white-space: pre-line;
  `,

  tagRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  `,

  buttonRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  `,

  explainerList: css`
    margin: 0;
    padding-left: 18px;
    color: ${token.colorTextSecondary};
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  emptyCompact: css`
    .ant-empty-description {
      color: ${token.colorTextSecondary};
    }
  `,
}));
