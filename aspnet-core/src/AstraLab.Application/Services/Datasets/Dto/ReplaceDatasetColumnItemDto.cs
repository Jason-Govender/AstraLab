using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a single dataset column item within a replacement request.
    /// </summary>
    [AutoMapTo(typeof(DatasetColumn))]
    public class ReplaceDatasetColumnItemDto
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        [Required]
        [StringLength(DatasetColumn.MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the declared or inferred data type name.
        /// </summary>
        [Required]
        [StringLength(DatasetColumn.MaxDataTypeLength)]
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data type was inferred.
        /// </summary>
        public bool IsDataTypeInferred { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the null count when known.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long? NullCount { get; set; }

        /// <summary>
        /// Gets or sets the distinct count when known.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long? DistinctCount { get; set; }
    }
}
