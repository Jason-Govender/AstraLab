"use client";

import { Button } from "antd";
import type { ButtonProps } from "antd";
import { useRouter } from "next/navigation";

interface DatasetExplorationLauncherButtonProps extends ButtonProps {
  datasetId: number;
  versionId?: number;
}

export const DatasetExplorationLauncherButton = ({
  datasetId,
  versionId,
  children = "Explore data",
  ...buttonProps
}: DatasetExplorationLauncherButtonProps) => {
  const router = useRouter();

  return (
    <Button
      {...buttonProps}
      onClick={() =>
        router.push(
          versionId
            ? `/datasets/${datasetId}/explore?versionId=${versionId}`
            : `/datasets/${datasetId}/explore`,
        )
      }
    >
      {children}
    </Button>
  );
};
