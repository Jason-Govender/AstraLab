using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Represents a persisted AI conversation thread returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(AIConversation))]
    public class AIConversationDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset identifier that scopes the conversation.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the owning user identifier for the conversation.
        /// </summary>
        public long OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the latest interaction in the conversation.
        /// </summary>
        public DateTime LastInteractionTime { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted conversation.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
