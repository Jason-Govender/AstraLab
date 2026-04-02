using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI.Dto
{
    /// <summary>
    /// Represents a persisted AI response returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(AIResponse))]
    public class AIResponseDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the conversation identifier that groups the response.
        /// </summary>
        public long AIConversationId { get; set; }

        /// <summary>
        /// Gets or sets the dataset version identifier that grounds the response.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional user query that led to the response.
        /// </summary>
        public string UserQuery { get; set; }

        /// <summary>
        /// Gets or sets the persisted AI-generated response content.
        /// </summary>
        public string ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets the response type used to classify the response.
        /// </summary>
        public AIResponseType ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the optional linked dataset transformation identifier.
        /// </summary>
        public long? DatasetTransformationId { get; set; }

        /// <summary>
        /// Gets or sets the optional linked machine learning experiment identifier.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted response.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
