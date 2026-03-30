using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Extracts initial schema metadata from a validated raw dataset upload.
    /// </summary>
    public interface IRawDatasetMetadataExtractor
    {
        /// <summary>
        /// Extracts schema metadata for the specified dataset format and content.
        /// </summary>
        ExtractedRawDatasetMetadata Extract(byte[] content, DatasetFormat datasetFormat);
    }
}
