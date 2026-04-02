namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Requests a grounded natural-language answer for a machine learning experiment.
    /// </summary>
    public class AskExperimentAiQuestionRequest
    {
        /// <summary>
        /// Gets or sets the machine learning experiment identifier that grounds the answer.
        /// </summary>
        public long MLExperimentId { get; set; }

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
