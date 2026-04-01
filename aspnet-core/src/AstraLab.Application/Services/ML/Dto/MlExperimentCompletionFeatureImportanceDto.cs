namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a feature importance row sent by the ML executor callback.
    /// </summary>
    public class MlExperimentCompletionFeatureImportanceDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the importance score.
        /// </summary>
        public decimal ImportanceScore { get; set; }

        /// <summary>
        /// Gets or sets the ordinal rank.
        /// </summary>
        public int Rank { get; set; }
    }
}
