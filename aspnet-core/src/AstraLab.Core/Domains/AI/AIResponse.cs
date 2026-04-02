using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.AI
{
    /// <summary>
    /// Represents a persisted assistant response tied to a dataset version and conversation thread.
    /// </summary>
    public class AIResponse : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// The database column type used for persisted user queries.
        /// </summary>
        public const string UserQueryColumnType = "text";

        /// <summary>
        /// The database column type used for persisted AI response content.
        /// </summary>
        public const string ResponseContentColumnType = "text";

        /// <summary>
        /// The database column type used for serialized metadata payloads.
        /// </summary>
        public const string MetadataJsonColumnType = "text";

        /// <summary>
        /// Gets or sets the tenant that owns the response.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the conversation identifier that groups this response.
        /// </summary>
        public long AIConversationId { get; set; }

        /// <summary>
        /// Gets or sets the conversation that groups this response.
        /// </summary>
        public AIConversation AIConversation { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that grounds this response.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version that grounds this response.
        /// </summary>
        public DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional user query that led to this response.
        /// </summary>
        public string UserQuery { get; set; }

        /// <summary>
        /// Gets or sets the persisted AI-generated response content.
        /// </summary>
        public string ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets the response type used to classify this persisted response.
        /// </summary>
        public AIResponseType ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the optional linked dataset transformation identifier.
        /// </summary>
        public long? DatasetTransformationId { get; set; }

        /// <summary>
        /// Gets or sets the optional linked dataset transformation.
        /// </summary>
        public DatasetTransformation DatasetTransformation { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload for future extensibility.
        /// </summary>
        public string MetadataJson { get; set; }
    }
}
