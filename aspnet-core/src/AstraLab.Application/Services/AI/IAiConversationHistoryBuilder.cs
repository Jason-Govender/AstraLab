using System.Collections.Generic;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds replayable conversation history from persisted AI responses.
    /// </summary>
    public interface IAiConversationHistoryBuilder
    {
        /// <summary>
        /// Builds replayable conversation messages in chronological order.
        /// </summary>
        IReadOnlyList<AiConversationHistoryMessage> Build(IReadOnlyList<AIResponse> responses);
    }
}
