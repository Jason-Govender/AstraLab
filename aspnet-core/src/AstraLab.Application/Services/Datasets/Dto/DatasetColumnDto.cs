using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents dataset column metadata returned by the application layer.
    /// </summary>
    [AutoMapFrom(typeof(DatasetColumn))]
    public class DatasetColumnDto : EntityDto<long>
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the column.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the declared or inferred data type name.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data type was inferred.
        /// </summary>
        public bool IsDataTypeInferred { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the column.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the null count when known.
        /// </summary>
        public long? NullCount { get; set; }

        /// <summary>
        /// Gets or sets the distinct count when known.
        /// </summary>
        public long? DistinctCount { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the column metadata record.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
