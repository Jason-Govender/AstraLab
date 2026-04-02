namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Requests a grounded natural-language answer for a dataset version.
    /// </summary>
    public class AskDatasetAiQuestionRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that grounds the answer.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the user question that should be answered.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the optional conversation identifier to continue.
        /// </summary>
        public long? ConversationId { get; set; }
    }
}
