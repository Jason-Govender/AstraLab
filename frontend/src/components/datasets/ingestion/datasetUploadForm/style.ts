"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "@/components/workspaceShell/workspaceSurfaceStyles";

export const useStyles = createStyles((utils) => {
  const { token, css } = utils;
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    card: surfaces.contextCard,

    form: css`
      display: flex;
      flex-direction: column;
      gap: 14px;
    `,

    introBlock: css`
      display: flex;
      flex-direction: column;
      gap: 6px;
    `,

    introTitle: css`
      margin: 0 !important;
      color: ${token.colorText} !important;
    `,

    helperText: css`
      color: ${token.colorTextSecondary};
      line-height: 1.7;
    `,

    dragger: css`
      .ant-upload {
        padding: 28px 20px;
      }

      .ant-upload.ant-upload-drag {
        border-radius: 20px;
        border-color: rgba(86, 120, 179, 0.34);
        background: rgba(12, 20, 34, 0.76);
      }

      .ant-upload.ant-upload-drag:hover {
        border-color: rgba(120, 152, 221, 0.65);
      }
    `,

    fileError: css`
      margin-top: 10px !important;
      color: ${token.colorError};
    `,

    selectedFileCard: css`
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      margin-top: 14px;
      padding: 16px 18px;
      border: 1px solid rgba(74, 120, 255, 0.32);
      border-radius: 18px;
      background: rgba(13, 22, 39, 0.82);

      @media (max-width: 640px) {
        flex-direction: column;
        align-items: flex-start;
      }
    `,

    selectedFileBody: css`
      display: flex;
      flex-direction: column;
      gap: 8px;
      min-width: 0;
    `,

    selectedFileName: css`
      color: ${token.colorText};
      font-size: 15px;
      font-weight: 600;
      word-break: break-word;
    `,

    selectedFileMeta: css`
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    `,

    submitRow: css`
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      flex-wrap: wrap;
      padding-top: 6px;
    `,

    submitHint: css`
      max-width: 460px;
      margin: 0 !important;
      color: ${token.colorTextSecondary} !important;
      line-height: 1.6;
    `,
  };
});
