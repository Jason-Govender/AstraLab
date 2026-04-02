using System.Collections.Generic;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents a provider-neutral text-generation request.
    /// </summary>
    public class AiTextGenerationRequest
    {
        /// <summary>
        /// Gets or sets the system-level instructions for the model.
        /// </summary>
        public string SystemInstructions { get; set; }

        /// <summary>
        /// Gets or sets the replayed prior conversation history.
        /// </summary>
        public IReadOnlyList<AiConversationHistoryMessage> ConversationHistory { get; set; }

        /// <summary>
        /// Gets or sets the final user prompt.
        /// </summary>
        public string UserMessage { get; set; }
    }
}
