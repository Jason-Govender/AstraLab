namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents one persisted machine learning feature-importance row included in AI experiment context.
    /// </summary>
    public class AiMlFeatureImportanceContext
    {
        /// <summary>
        /// Gets or sets the dataset column identifier.
        /// </summary>
        public long DatasetColumnId { get; set; }

        /// <summary>
        /// Gets or sets the optional column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the importance score.
        /// </summary>
        public decimal ImportanceScore { get; set; }

        /// <summary>
        /// Gets or sets the persisted rank.
        /// </summary>
        public int Rank { get; set; }
    }
}
