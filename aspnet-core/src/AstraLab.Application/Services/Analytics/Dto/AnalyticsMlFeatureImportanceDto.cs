namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a compact feature-importance row for analytics summaries.
    /// </summary>
    public class AnalyticsMlFeatureImportanceDto
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the importance score.
        /// </summary>
        public decimal ImportanceScore { get; set; }

        /// <summary>
        /// Gets or sets the importance rank.
        /// </summary>
        public int Rank { get; set; }
    }
}
