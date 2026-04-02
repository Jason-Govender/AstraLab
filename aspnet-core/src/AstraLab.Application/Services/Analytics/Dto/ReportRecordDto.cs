using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Analytics;

namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Represents a persisted stakeholder report returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(ReportRecord))]
    public class ReportRecordDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that anchors the report.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset profile identifier that informed the report.
        /// </summary>
        public long? DatasetProfileId { get; set; }

        /// <summary>
        /// Gets or sets the optional machine learning experiment identifier that informed the report.
        /// </summary>
        public long? MLExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional AI response identifier that originated the report.
        /// </summary>
        public long? AIResponseId { get; set; }

        /// <summary>
        /// Gets or sets the stakeholder-facing title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the optional short summary displayed with the report.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the full persisted report content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the report content format.
        /// </summary>
        public ReportFormat ReportFormat { get; set; }

        /// <summary>
        /// Gets or sets the source classification for the report.
        /// </summary>
        public ReportSourceType ReportSourceType { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized metadata payload.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted report.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
