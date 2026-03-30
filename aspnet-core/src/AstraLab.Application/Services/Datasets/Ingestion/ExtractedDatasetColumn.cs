namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Represents a single extracted dataset column before it is persisted.
    /// </summary>
    public class ExtractedDatasetColumn
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the inferred column data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data type was inferred.
        /// </summary>
        public bool IsDataTypeInferred { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the extracted column.
        /// </summary>
        public int Ordinal { get; set; }
    }
}
