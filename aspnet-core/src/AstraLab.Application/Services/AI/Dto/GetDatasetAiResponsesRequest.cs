using Abp.Application.Services.Dto;

namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Requests a persisted AI response thread for the selected conversation.
    /// </summary>
    public class GetDatasetAiResponsesRequest : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the conversation identifier that owns the responses.
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thread should be returned in chronological order.
        /// </summary>
        public bool IsChronological { get; set; } = true;
    }
}
