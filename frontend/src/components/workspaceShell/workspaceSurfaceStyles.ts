"use client";

import type { CreateStylesUtils } from "antd-style/es/factories/createStyles/types";

/**
 * Shared protected-workspace surface styles used across dashboard, reports,
 * assistant, ML, dataset catalog, and upload screens so the product feels
 * visually consistent without duplicating the same card/layout treatment.
 */
export const buildWorkspaceSurfaceStyles = ({
  token,
  css,
}: CreateStylesUtils) => ({
  actionGroup: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  stack: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
  `,

  contextCard: css`
    &.ant-card {
      border: 1px solid rgba(86, 120, 179, 0.28);
      border-radius: 28px;
      background:
        radial-gradient(circle at top left, rgba(78, 115, 255, 0.16), transparent 40%),
        radial-gradient(circle at bottom right, rgba(14, 193, 165, 0.12), transparent 35%),
        rgba(11, 18, 34, 0.94);
      box-shadow: 0 24px 60px rgba(3, 9, 22, 0.26);
    }

    .ant-card-body {
      display: flex;
      flex-direction: column;
      gap: 22px;
      padding: 28px;
    }
  `,

  selectionCard: css`
    &.ant-card {
      border: 1px solid rgba(86, 120, 179, 0.28);
      border-radius: 24px;
      background:
        radial-gradient(circle at top left, rgba(78, 115, 255, 0.16), transparent 40%),
        radial-gradient(circle at bottom right, rgba(14, 193, 165, 0.12), transparent 35%),
        rgba(11, 18, 34, 0.94);
      box-shadow: none;
    }

    .ant-card-body {
      padding: 24px;
    }
  `,

  sectionCard: css`
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

  sectionCardMuted: css`
    &.ant-card {
      border: 1px solid rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,

  feedbackAlert: css`
    margin-top: 4px;
  `,

  selectorField: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  selectInput: css`
    width: 100%;
  `,

  subtleText: css`
    color: ${token.colorTextSecondary};
  `,

  compactEmpty: css`
    .ant-empty-description {
      color: ${token.colorTextSecondary};
      max-width: 520px;
      margin: 0 auto;
    }
  `,

  loadingCard: css`
    &.ant-card {
      min-height: 260px;
      border: 1px solid rgba(40, 58, 92, 0.92);
      border-radius: 24px;
      background: rgba(17, 26, 45, 0.94);
      box-shadow: none;
    }
  `,
});
