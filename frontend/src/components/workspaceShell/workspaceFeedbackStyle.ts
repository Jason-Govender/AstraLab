"use client";

import { createStyles } from "antd-style";
import { buildWorkspaceSurfaceStyles } from "./workspaceSurfaceStyles";

export const useWorkspaceFeedbackStyles = createStyles((utils) => {
  const surfaces = buildWorkspaceSurfaceStyles(utils);

  return {
    feedbackAlert: surfaces.feedbackAlert,
    compactEmpty: surfaces.compactEmpty,
    emptyCard: surfaces.sectionCardMuted,
    loadingCard: surfaces.loadingCard,
    actionRow: surfaces.actionGroup,
  };
});
