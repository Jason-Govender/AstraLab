using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a persisted dataset transformation summary returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(DatasetTransformation))]
    public class DatasetTransformationDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the source dataset version identifier.
        /// </summary>
        public long SourceDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the resulting dataset version identifier when present.
        /// </summary>
        public long? ResultDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType { get; set; }

        /// <summary>
        /// Gets or sets the canonical serialized transformation configuration.
        /// </summary>
        public string ConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the execution order for the source dataset version.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the transformation executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// Gets or sets the serialized transformation summary payload.
        /// </summary>
        public string SummaryJson { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the persisted transformation record.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
