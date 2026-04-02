using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a persisted analytics insight returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(InsightRecord))]
    public class InsightRecordDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the insight.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset profile identifier that informed the insight.
        /// </summary>
        public long? DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier that informed the insight.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional AI response identifier that originated the insight.
        /// </summary>
        public long? AIResponseId { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the persisted insight content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the insight classification.
        /// </summary>
        public InsightType InsightType { get; set; }

        /// <summary>
        /// Gets or sets the source classification for the insight.
        /// </summary>
        public InsightSourceType InsightSourceType { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted insight.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
