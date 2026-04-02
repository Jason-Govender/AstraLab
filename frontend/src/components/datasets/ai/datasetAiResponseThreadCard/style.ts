"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    &.ant-card {
      border-radius: 24px;
      border-color: rgba(40, 58, 92, 0.18);
      box-shadow: 0 18px 44px rgba(15, 23, 42, 0.08);
    }
  `,

  thread: css`
    display: flex;
    flex-direction: column;
    gap: 18px;
  `,

  turn: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  bubble: css`
    padding: 16px 18px;
    border-radius: 18px;
    border: 1px solid rgba(40, 58, 92, 0.14);
  `,

  userBubble: css`
    background: rgba(15, 23, 42, 0.04);
  `,

  assistantBubble: css`
    background: ${token.colorPrimaryBg};
  `,

  bubbleHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 10px;
  `,

  bubbleText: css`
    margin-bottom: 0 !important;
    white-space: pre-wrap;
  `,

  helperText: css`
    margin-bottom: 0 !important;
    color: ${token.colorTextSecondary};
  `,
}));
