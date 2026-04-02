"use client";

interface BuildAiAssistantHrefOptions {
  datasetId?: number;
  versionId?: number;
  experimentId?: number;
}

export const buildAiAssistantHref = ({
  datasetId,
  versionId,
  experimentId,
}: BuildAiAssistantHrefOptions): string => {
  const searchParams = new URLSearchParams();

  if (datasetId) {
    searchParams.set("datasetId", String(datasetId));
  }

  if (versionId) {
    searchParams.set("versionId", String(versionId));
  }

  if (experimentId) {
    searchParams.set("experimentId", String(experimentId));
  }

  const queryString = searchParams.toString();

  return queryString ? `/ai-assistant?${queryString}` : "/ai-assistant";
};
