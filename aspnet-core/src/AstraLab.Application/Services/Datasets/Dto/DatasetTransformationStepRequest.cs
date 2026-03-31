using System.ComponentModel.DataAnnotations;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a single transformation step in a versioned transformation pipeline.
    /// </summary>
    public class DatasetTransformationStepRequest
    {
        /// <summary>
        /// Gets or sets the transformation type.
        /// </summary>
        [Required]
        public DatasetTransformationType TransformationType { get; set; }

        /// <summary>
        /// Gets or sets the serialized configuration payload for the transformation step.
        /// </summary>
        [Required]
        public string ConfigurationJson { get; set; }
    }
}
