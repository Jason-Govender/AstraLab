using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Represents the schema metadata extracted from a raw dataset upload.
    /// </summary>
    public class ExtractedRawDatasetMetadata
    {
        /// <summary>
        /// Gets or sets the serialized schema payload.
        /// </summary>
        public string SchemaJson { get; set; }

        /// <summary>
        /// Gets or sets the extracted column count.
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the ordered extracted columns.
        /// </summary>
        public IReadOnlyList<ExtractedDatasetColumn> Columns { get; set; } = new List<ExtractedDatasetColumn>();
    }
}
