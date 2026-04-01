namespace AstraLab.Services.ML
{
    /// <summary>
    /// Represents a dataset column included in the executor dispatch payload.
    /// </summary>
    public class DispatchMlExperimentColumn
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
        /// Gets or sets the column data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the stable dataset ordinal.
        /// </summary>
        public int Ordinal { get; set; }
    }
}
