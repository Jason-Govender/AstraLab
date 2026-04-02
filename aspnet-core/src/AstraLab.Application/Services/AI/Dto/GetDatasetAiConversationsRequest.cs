using Abp.Application.Services.Dto;

namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Requests dataset-scoped persisted AI conversations for the selected dataset or dataset version.
    /// </summary>
    public class GetDatasetAiConversationsRequest : PagedResultRequestDto
    {
        /// <summary>
        /// Gets or sets the dataset identifier that scopes the conversations.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset version identifier used to narrow the returned conversations.
        /// </summary>
        public long? DatasetVersionId { get; set; }
    }
}
