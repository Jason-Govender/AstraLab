namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Returns a persisted AI response together with its resolved conversation identifier.
    /// </summary>
    public class GenerateDatasetAiResponseResult
    {
        /// <summary>
        /// Gets or sets the conversation identifier that groups the response.
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the persisted AI response.
        /// </summary>
        public AIResponseDto Response { get; set; }
    }
}
