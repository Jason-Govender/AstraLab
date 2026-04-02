"use client";

import { Button } from "antd";
import type { ButtonProps } from "antd";
import { useRouter } from "next/navigation";

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

  const searchParams = new URLSearchParams();

  if (versionId) {
    searchParams.set("versionId", String(versionId));
  }

  if (experimentId) {
    searchParams.set("experimentId", String(experimentId));
  }

  const href = searchParams.toString()
    ? `/datasets/${datasetId}/assistant?${searchParams.toString()}`
    : `/datasets/${datasetId}/assistant`;

  return (
    <Button
      {...buttonProps}
      onClick={() => router.push(href)}
    >
      {children}
    </Button>
  );
};
