using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the metadata required to create a dataset version.
    /// </summary>
    [AutoMapTo(typeof(DatasetVersion))]
    public class CreateDatasetVersionDto
    {
        /// <summary>
        /// Gets or sets the parent dataset identifier.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long DatasetId { get; set; }

        /// <summary>
        /// Gets or sets the version type.
        /// </summary>
        [Required]
        public DatasetVersionType VersionType { get; set; }

        /// <summary>
        /// Gets or sets the parent version identifier for derived versions.
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
        /// Gets or sets the version size in bytes.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long SizeBytes { get; set; }
    }
}
