using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Core.Domains.AI
{
    /// <summary>
    /// Represents a tenant-scoped dataset conversation thread that groups related AI responses.
    /// </summary>
    public class AIConversation : FullAuditedEntity<long>, IMustHaveTenant
    {
        /// <summary>
        /// Gets or sets the tenant that owns the conversation thread.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Gets or sets the dataset identifier that scopes the conversation.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the dataset that scopes the conversation.
        /// </summary>
        public Dataset Dataset { get; set; }

        /// <summary>
        /// Gets or sets the owning user identifier for the conversation.
        /// </summary>
        public long OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the latest persisted interaction in the thread.
        /// </summary>
        public DateTime LastInteractionTime { get; set; }

        /// <summary>
        /// Gets or sets the persisted responses recorded for the conversation.
        /// </summary>
        public ICollection<AIResponse> Responses { get; set; } = new List<AIResponse>();
    }
}
