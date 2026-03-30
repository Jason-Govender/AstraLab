using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the dataset metadata returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(Dataset))]
    public class DatasetDto : EntityDto<long>
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
        /// Gets or sets the current dataset status.
        /// </summary>
        public DatasetStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the owning user identifier.
        /// </summary>
        public long OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the original uploaded file name.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the dataset creation time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
