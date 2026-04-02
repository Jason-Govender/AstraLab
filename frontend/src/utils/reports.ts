export const buildReportsHref = (
  datasetId?: number,
  datasetVersionId?: number,
): string => {
  const searchParams = new URLSearchParams();

  if (datasetId) {
    searchParams.set("datasetId", String(datasetId));
  }

  if (datasetVersionId) {
    searchParams.set("versionId", String(datasetVersionId));
  }

  const queryString = searchParams.toString();

  return queryString ? `/reports?${queryString}` : "/reports";
};
