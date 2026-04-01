using System;

namespace AstraLab.Services.ML.Dto
{
    /// <summary>
    /// Represents the failure callback payload from the ML executor.
    /// </summary>
    public class FailMlExperimentCallbackRequest
    {
        /// <summary>
        /// Gets or sets the experiment identifier.
        /// </summary>
        public long ExperimentId { get; set; }

        /// <summary>
        /// Gets or sets the optional started-at timestamp provided by the executor.
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the optional completed-at timestamp provided by the executor.
        /// </summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the failure message.
        /// </summary>
        public string FailureMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional serialized warnings payload.
        /// </summary>
        public string WarningsJson { get; set; }
    }
}
