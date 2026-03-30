namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Represents a dataset column that should be profiled.
    /// </summary>
    public class ProfileDatasetColumnRequest
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column.
        /// </summary>
        public int Ordinal { get; set; }
    }
}
