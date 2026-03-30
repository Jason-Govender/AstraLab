using System;
using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a datasets-page list item with latest version summary metadata.
    /// </summary>
    public class DatasetListItemDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional dataset description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the source format of the dataset.
        /// </summary>
        public DatasetFormat SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets the dataset status.
        /// </summary>
        public DatasetStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the dataset creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the current dataset version identifier when available.
        /// </summary>
        public long? CurrentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the current dataset version number when available.
        /// </summary>
        public int? CurrentVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the current dataset version status when available.
        /// </summary>
        public DatasetVersionStatus? CurrentVersionStatus { get; set; }

        /// <summary>
        /// Gets or sets the current dataset version column count when available.
        /// </summary>
        public int? ColumnCount { get; set; }
    }
}
