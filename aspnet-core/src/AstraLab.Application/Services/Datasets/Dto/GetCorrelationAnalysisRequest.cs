using System.Collections.Generic;

namespace AstraLab.Services.Datasets.Dto
{
    /// <summary>
    /// Represents a request for correlation analysis data.
    /// </summary>
    public class GetCorrelationAnalysisRequest
    {
        /// <summary>
        /// Gets or sets the dataset version identifier that owns the requested columns.
        /// </summary>
        public long DatasetVersionId { get; set; }

        /// <summary>
        /// Gets or sets the numeric dataset column identifiers to correlate.
        /// </summary>
        public List<long> DatasetColumnIds { get; set; } = new List<long>();
    }
}
