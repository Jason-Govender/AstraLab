using System.IO;
using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Ingestion;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Parses, serializes, and derives metadata for tabular dataset content.
    /// </summary>
    public interface IDatasetTabularDataCodec
    {
        /// <summary>
        /// Parses the specified stored dataset content into a shared tabular model.
        /// </summary>
        Task<TabularDataset> ReadAsync(DatasetFormat datasetFormat, System.Collections.Generic.IReadOnlyList<DatasetColumn> columns, Stream content);

        /// <summary>
        /// Serializes the supplied tabular dataset into the requested dataset format.
        /// </summary>
        byte[] Write(TabularDataset dataset, DatasetFormat datasetFormat);

        /// <summary>
        /// Builds extracted metadata for the supplied transformed dataset.
        /// </summary>
        ExtractedRawDatasetMetadata BuildMetadata(TabularDataset dataset, DatasetFormat datasetFormat);

        /// <summary>
        /// Gets the default content type for the requested dataset format.
        /// </summary>
        string GetContentType(DatasetFormat datasetFormat);

        /// <summary>
        /// Gets the default file extension for the requested dataset format.
        /// </summary>
        string GetFileExtension(DatasetFormat datasetFormat);
    }
}
