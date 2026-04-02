namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents a single replayable conversation message.
    /// </summary>
    public class AiConversationHistoryMessage
    {
        /// <summary>
        /// Gets or sets the provider role name.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Content { get; set; }
    }
}
