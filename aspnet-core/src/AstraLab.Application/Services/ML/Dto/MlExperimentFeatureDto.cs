namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a selected feature column for an experiment response.
    /// </summary>
    public class MlExperimentFeatureDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stable ordinal for the feature selection row.
        /// </summary>
        public int Ordinal { get; set; }
    }
}
