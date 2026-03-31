using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents the request to execute a transformation pipeline from a source dataset version.
    /// </summary>
    public class TransformDatasetVersionRequest
    {
        /// <summary>
        /// Gets or sets the source dataset version identifier.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long SourceDatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the ordered transformation steps to execute.
        /// </summary>
        [Required]
        public List<DatasetTransformationStepRequest> Steps { get; set; } = new List<DatasetTransformationStepRequest>();
    }
}
