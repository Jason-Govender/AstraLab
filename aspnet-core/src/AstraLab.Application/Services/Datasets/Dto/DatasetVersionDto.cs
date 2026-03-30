using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents dataset version metadata returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(DatasetVersion))]
    public class DatasetVersionDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the parent dataset identifier.
        /// </summary>
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the sequential version number within the dataset.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the version type.
        /// </summary>
        public DatasetVersionType VersionType { get; set; }

        /// <summary>
        /// Gets or sets the version lifecycle status.
        /// </summary>
        public DatasetVersionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the parent version identifier when present.
        /// </summary>
        public long? ParentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the row count when known.
        /// </summary>
        public int? RowCount { get; set; }

        /// <summary>
        /// Gets or sets the column count when known.
        /// </summary>
        public int? ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized schema payload when known.
        /// </summary>
        public string SchemaJson { get; set; }

        /// <summary>
        /// Gets or sets the dataset version size in bytes.
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the version.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
