"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles((utils) => {
  const { css } = utils;

  return {
    card: css`
      &.ant-card {
        border-radius: 24px;
        border-color: rgba(255, 95, 95, 0.2);
        background: rgba(17, 26, 45, 0.94);
        box-shadow: none;
      }
    `,
    content: css`
      width: 100%;
    `,
  };
});
