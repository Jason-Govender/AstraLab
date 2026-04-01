namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents a feature importance row returned to the frontend.
    /// </summary>
    public class MlModelFeatureImportanceDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the dataset column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the importance score.
        /// </summary>
        public decimal ImportanceScore { get; set; }

        /// <summary>
        /// Gets or sets the ordinal rank of the feature importance row.
        /// </summary>
        public int Rank { get; set; }
    }
}
