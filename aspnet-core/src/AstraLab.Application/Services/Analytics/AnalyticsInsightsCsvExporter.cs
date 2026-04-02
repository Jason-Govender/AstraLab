using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.Dependency;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Exports structured analytics highlights to CSV.
    /// </summary>
    public class AnalyticsInsightsCsvExporter : IAnalyticsInsightsCsvExporter, ITransientDependency
    {
        /// <summary>
        /// Builds a CSV payload from the supplied analytics summary.
        /// </summary>
        public byte[] Export(DatasetAnalyticsSummaryDto summary)
        {
            var rows = new List<string>
            {
                Join("section", "itemType", "name", "value", "detail", "secondaryValue")
            };

            AddQualityRows(rows, summary);
            AddTransformationRows(rows, summary);
            AddAiRows(rows, summary);
            AddMlRows(rows, summary);

            return Encoding.UTF8.GetBytes(string.Join("\n", rows));
        }

        /// <summary>
        /// Adds quality-highlight rows to the CSV payload.
        /// </summary>
        private static void AddQualityRows(ICollection<string> rows, DatasetAnalyticsSummaryDto summary)
        {
            foreach (var column in summary?.QualityHighlights?.HighRiskColumns ?? new List<DatasetQualityColumnHighlightDto>())
            {
                rows.Add(Join(
                    "quality",
                    "highRiskColumn",
                    column.Name,
                    column.NullPercentage.ToString("0.##"),
                    "AnomalyPercentage=" + column.AnomalyPercentage.ToString("0.##"),
                    column.DataType));
            }
        }

        /// <summary>
        /// Adds transformation rows to the CSV payload.
        /// </summary>
        private static void AddTransformationRows(ICollection<string> rows, DatasetAnalyticsSummaryDto summary)
        {
            foreach (var transformation in summary?.TransformationOutcomes ?? new List<TransformationOutcomeSummaryDto>())
            {
                rows.Add(Join(
                    "transformation",
                    transformation.TransformationType.ToString(),
                    "Transformation " + transformation.DatasetTransformationId,
                    transformation.ExecutedAt.ToString("u"),
                    transformation.SummaryPreview,
                    transformation.ExecutionOrder.ToString()));
            }
        }

        /// <summary>
        /// Adds AI finding rows to the CSV payload.
        /// </summary>
        private static void AddAiRows(ICollection<string> rows, DatasetAnalyticsSummaryDto summary)
        {
            foreach (var finding in summary?.AiFindings?.RecentFindings ?? new List<AiFindingPreviewDto>())
            {
                rows.Add(Join(
                    "ai",
                    finding.Source,
                    finding.Title,
                    finding.CreationTime.ToString("u"),
                    finding.ContentPreview,
                    string.Empty));
            }
        }

        /// <summary>
        /// Adds machine-learning rows to the CSV payload.
        /// </summary>
        private static void AddMlRows(ICollection<string> rows, DatasetAnalyticsSummaryDto summary)
        {
            foreach (var metric in summary?.MlExperimentHighlights?.Metrics ?? new List<AnalyticsMlMetricDto>())
            {
                rows.Add(Join(
                    "ml",
                    "metric",
                    metric.MetricName,
                    metric.MetricValue.ToString("0.####"),
                    string.Empty,
                    string.Empty));
            }

            foreach (var featureImportance in summary?.MlExperimentHighlights?.TopFeatureImportances ?? new List<AnalyticsMlFeatureImportanceDto>())
            {
                rows.Add(Join(
                    "ml",
                    "featureImportance",
                    featureImportance.ColumnName,
                    featureImportance.ImportanceScore.ToString("0.####"),
                    "Rank=" + featureImportance.Rank,
                    string.Empty));
            }
        }

        /// <summary>
        /// Builds one escaped CSV row.
        /// </summary>
        private static string Join(params string[] values)
        {
            return string.Join(",", values.Select(Escape));
        }

        /// <summary>
        /// Escapes a CSV cell value.
        /// </summary>
        private static string Escape(string value)
        {
            var normalized = value ?? string.Empty;
            if (normalized.Contains("\""))
            {
                normalized = normalized.Replace("\"", "\"\"");
            }

            return normalized.Contains(",") || normalized.Contains("\n") || normalized.Contains("\r")
                ? "\"" + normalized + "\""
                : normalized;
        }
    }
}
