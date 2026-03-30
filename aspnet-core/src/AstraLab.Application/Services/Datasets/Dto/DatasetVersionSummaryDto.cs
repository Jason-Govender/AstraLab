using System;
using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents summary metadata for a linked dataset version.
    /// </summary>
    public class DatasetVersionSummaryDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the version number.
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
        /// Gets or sets the creation time of the version.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the column count when known.
        /// </summary>
        public int? ColumnCount { get; set; }
    }
}
