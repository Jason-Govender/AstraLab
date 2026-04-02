"use client";

import { Button } from "antd";
import type { ButtonProps } from "antd";
import { useRouter } from "next/navigation";
import { buildAiAssistantHref } from "@/utils/aiAssistant";

interface DatasetAiAssistantLauncherButtonProps extends ButtonProps {
  datasetId: number;
  versionId?: number;
  experimentId?: number;
}

export const DatasetAiAssistantLauncherButton = ({
  datasetId,
  versionId,
  experimentId,
  children = "AI assistant",
  ...buttonProps
}: DatasetAiAssistantLauncherButtonProps) => {
  const router = useRouter();

  const href = buildAiAssistantHref({
    datasetId,
    versionId,
    experimentId,
  });

  return (
    <Button
      {...buttonProps}
      onClick={() => router.push(href)}
    >
      {children}
    </Button>
  );
};
