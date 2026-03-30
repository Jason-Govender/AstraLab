using System.Collections.Generic;
using System.IO;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Represents the input required to profile a dataset version.
    /// </summary>
    public class ProfileDatasetVersionRequest
    {
        /// <summary>
        /// Gets or sets the dataset format.
        /// </summary>
        public DatasetFormat DatasetFormat { get; set; }

        /// <summary>
        /// Gets or sets the ordered columns for the dataset version.
        /// </summary>
        public IReadOnlyList<ProfileDatasetColumnRequest> Columns { get; set; }

        /// <summary>
        /// Gets or sets the raw dataset content stream.
        /// </summary>
        public Stream Content { get; set; }
    }
}
