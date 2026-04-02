using System.Text.Json;
using System.Text.Json.Serialization;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Provides shared metadata helpers for profiling-triggered automatic dataset insights.
    /// </summary>
    public static class AiAutomaticInsightMetadata
    {
        /// <summary>
        /// The metadata trigger value used for profiling-completed automatic insights.
        /// </summary>
        public const string ProfilingCompletedGenerationTrigger = "profilingCompleted";

        /// <summary>
        /// The metadata trigger value used for experiment-completed automatic insights.
        /// </summary>
        public const string ExperimentCompletedGenerationTrigger = "experimentCompleted";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Determines whether the persisted metadata belongs to an automatic profiling-triggered insight.
        /// </summary>
        public static bool IsAutomaticProfilingInsight(string metadataJson, long datasetProfileId)
        {
            if (!TryRead(metadataJson, out var metadata))
            {
                return false;
            }

            return metadata.GenerationTrigger == ProfilingCompletedGenerationTrigger &&
                   metadata.DatasetProfileId == datasetProfileId;
        }

        /// <summary>
        /// Determines whether the persisted metadata belongs to any profiling-triggered automatic insight.
        /// </summary>
        public static bool IsAutomaticProfilingInsight(string metadataJson)
        {
            if (!TryRead(metadataJson, out var metadata))
            {
                return false;
            }

            return metadata.GenerationTrigger == ProfilingCompletedGenerationTrigger;
        }

        /// <summary>
        /// Determines whether the persisted metadata belongs to an automatic experiment-completed insight.
        /// </summary>
        public static bool IsAutomaticExperimentInsight(string metadataJson, long mlExperimentId)
        {
            if (!TryRead(metadataJson, out var metadata))
            {
                return false;
            }

            return metadata.GenerationTrigger == ExperimentCompletedGenerationTrigger &&
                   metadata.MLExperimentId == mlExperimentId;
        }

        /// <summary>
        /// Determines whether the persisted metadata belongs to any experiment-completed automatic insight.
        /// </summary>
        public static bool IsAutomaticExperimentInsight(string metadataJson)
        {
            if (!TryRead(metadataJson, out var metadata))
            {
                return false;
            }

            return metadata.GenerationTrigger == ExperimentCompletedGenerationTrigger;
        }

        /// <summary>
        /// Reads automatic insight metadata when it is available and valid.
        /// </summary>
        public static bool TryRead(string metadataJson, out AutomaticInsightMetadata metadata)
        {
            metadata = null;

            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                metadata = JsonSerializer.Deserialize<AutomaticInsightMetadata>(metadataJson, SerializerOptions);
                return metadata != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents the automatic-insight metadata fields persisted inside AI response metadata JSON.
        /// </summary>
        public class AutomaticInsightMetadata
        {
            /// <summary>
            /// Gets or sets the generation trigger recorded for the response.
            /// </summary>
            public string GenerationTrigger { get; set; }

            /// <summary>
            /// Gets or sets the dataset profile identifier used to ground the automatic insight.
            /// </summary>
            public long? DatasetProfileId { get; set; }

            /// <summary>
            /// Gets or sets the machine learning experiment identifier used to ground the automatic insight.
            /// </summary>
            public long? MLExperimentId { get; set; }
        }
    }
}
