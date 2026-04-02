using System.Collections.Generic;
using Abp.Dependency;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds replayable provider-neutral conversation history from persisted AI responses.
    /// </summary>
    public class AiConversationHistoryBuilder : IAiConversationHistoryBuilder, ITransientDependency
    {
        /// <summary>
        /// Builds replayable conversation messages in chronological order.
        /// </summary>
        public IReadOnlyList<AiConversationHistoryMessage> Build(IReadOnlyList<AIResponse> responses)
        {
            var output = new List<AiConversationHistoryMessage>();

            foreach (var response in responses)
            {
                if (!string.IsNullOrWhiteSpace(response.UserQuery))
                {
                    output.Add(new AiConversationHistoryMessage
                    {
                        Role = "user",
                        Content = response.UserQuery
                    });
                }

                output.Add(new AiConversationHistoryMessage
                {
                    Role = "assistant",
                    Content = response.ResponseContent
                });
            }

            return output;
        }
    }
}
