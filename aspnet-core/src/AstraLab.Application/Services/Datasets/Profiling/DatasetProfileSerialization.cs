using System.Collections.Generic;
using System.Text.Json;

namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Serializes and deserializes persisted dataset profiling payloads.
    /// </summary>
    public static class DatasetProfileSerialization
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Builds the serialized dataset-level summary payload.
        /// </summary>
        public static string BuildSummaryJson(long totalNullCount, decimal overallNullPercentage)
        {
            return JsonSerializer.Serialize(new DatasetProfileSummaryPayload
            {
                TotalNullCount = totalNullCount,
                OverallNullPercentage = overallNullPercentage
            }, SerializerOptions);
        }

        /// <summary>
        /// Builds the serialized column-level statistics payload.
        /// </summary>
        public static string BuildColumnStatisticsJson(
            decimal nullPercentage,
            decimal? mean,
            decimal? min,
            decimal? max)
        {
            return JsonSerializer.Serialize(new DatasetColumnStatisticsPayload
            {
                NullPercentage = nullPercentage,
                Mean = mean,
                Min = min,
                Max = max
            }, SerializerOptions);
        }

        /// <summary>
        /// Reads the null percentage from a persisted column statistics payload.
        /// </summary>
        public static decimal ReadNullPercentage(string statisticsJson)
        {
            if (string.IsNullOrWhiteSpace(statisticsJson))
            {
                return 0m;
            }

            var payload = JsonSerializer.Deserialize<DatasetColumnStatisticsPayload>(statisticsJson, SerializerOptions);
            return payload?.NullPercentage ?? 0m;
        }

        private class DatasetProfileSummaryPayload
        {
            public long TotalNullCount { get; set; }

            public decimal OverallNullPercentage { get; set; }
        }

        private class DatasetColumnStatisticsPayload
        {
            public decimal NullPercentage { get; set; }

            public decimal? Mean { get; set; }

            public decimal? Min { get; set; }

            public decimal? Max { get; set; }
        }
    }
}
