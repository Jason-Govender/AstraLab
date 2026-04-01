"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css, token }) => ({
  card: css`
    overflow: hidden;
    border: 1px solid rgba(86, 120, 179, 0.28);
    border-radius: 28px;
    background:
      radial-gradient(circle at top left, rgba(78, 115, 255, 0.16), transparent 40%),
      radial-gradient(circle at bottom right, rgba(14, 193, 165, 0.12), transparent 35%),
      rgba(11, 18, 34, 0.94);
    box-shadow: none;
  `,

  title: css`
    margin-bottom: 6px !important;
    color: ${token.colorTextHeading};
  `,

  helperText: css`
    margin-bottom: 20px !important;
    color: ${token.colorTextSecondary};
  `,

  workspaceGrid: css`
    display: grid;
    grid-template-columns: minmax(0, 1.2fr) minmax(320px, 0.9fr);
    gap: 20px;

    @media (max-width: 1100px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  panel: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,

  sectionCard: css`
    border: 1px solid rgba(118, 145, 191, 0.18);
    border-radius: 22px;
    background: rgba(17, 27, 48, 0.82);
    box-shadow: none;
  `,

  formGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 16px;

    @media (max-width: 768px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  fullWidth: css`
    grid-column: 1 / -1;
  `,

  experimentList: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  experimentItem: css`
    padding: 14px 16px;
    border: 1px solid rgba(118, 145, 191, 0.18);
    border-radius: 18px;
    background: rgba(10, 17, 30, 0.64);
    transition:
      border-color 0.2s ease,
      transform 0.2s ease;
    cursor: pointer;

    &:hover {
      transform: translateY(-1px);
      border-color: rgba(103, 149, 255, 0.4);
    }
  `,

  experimentItemSelected: css`
    border-color: rgba(52, 211, 153, 0.42);
    background: rgba(13, 35, 34, 0.78);
  `,

  experimentMeta: css`
    margin-top: 8px;
    color: ${token.colorTextSecondary};
  `,

  tagWrap: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  `,

  detailActions: css`
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
  `,

  codeBlock: css`
    margin: 0;
    padding: 12px 14px;
    border-radius: 16px;
    background: rgba(4, 10, 20, 0.88);
    color: rgba(216, 228, 255, 0.92);
    overflow: auto;
    font-size: 12px;
    line-height: 1.6;
  `,
}));
