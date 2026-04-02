"use client";

import { Button } from "antd";
import type { ButtonProps } from "antd";
import { useRouter } from "next/navigation";

interface DatasetAiAssistantLauncherButtonProps extends ButtonProps {
  datasetId: number;
  versionId?: number;
}

export const DatasetAiAssistantLauncherButton = ({
  datasetId,
  versionId,
  children = "AI assistant",
  ...buttonProps
}: DatasetAiAssistantLauncherButtonProps) => {
  const router = useRouter();

  return (
    <Button
      {...buttonProps}
      onClick={() =>
        router.push(
          versionId
            ? `/datasets/${datasetId}/assistant?versionId=${versionId}`
            : `/datasets/${datasetId}/assistant`,
        )
      }
    >
      {children}
    </Button>
  );
};
