using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using AstraLab.Services.AI;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates a concise stakeholder-facing narrative from the deterministic unified analytics summary.
    /// </summary>
    public class AnalyticsNarrativeGenerator : IAnalyticsNarrativeGenerator, ITransientDependency
    {
        private const int MaxPreviewLength = 240;

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IAiTextGenerationClient _aiTextGenerationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsNarrativeGenerator"/> class.
        /// </summary>
        public AnalyticsNarrativeGenerator(IAiTextGenerationClient aiTextGenerationClient)
        {
            _aiTextGenerationClient = aiTextGenerationClient;
        }

        /// <summary>
        /// Generates an optional narrative for the supplied deterministic analytics summary.
        /// </summary>
        public async Task<AnalyticsNarrativeDto> GenerateAsync(DatasetAnalyticsSummaryDto summary)
        {
            if (!HasMeaningfulContext(summary))
            {
                return new AnalyticsNarrativeDto
                {
                    Status = AnalyticsNarrativeStatus.Unavailable
                };
            }

            try
            {
                var request = new AiTextGenerationRequest
                {
                    SystemInstructions = BuildSystemInstructions(),
                    UserMessage = BuildUserMessage(summary)
                };

                var result = await _aiTextGenerationClient.GenerateTextAsync(request);
                var narrative = result?.Text?.Trim();

                if (string.IsNullOrWhiteSpace(narrative))
                {
                    return new AnalyticsNarrativeDto
                    {
                        Status = AnalyticsNarrativeStatus.Unavailable
                    };
                }

                return new AnalyticsNarrativeDto
                {
                    Status = AnalyticsNarrativeStatus.Generated,
                    Content = narrative
                };
            }
            catch (Exception)
            {
                return new AnalyticsNarrativeDto
                {
                    Status = AnalyticsNarrativeStatus.Failed,
                    FailureMessage = "Analytics narrative generation is currently unavailable."
                };
            }
        }

        /// <summary>
        /// Determines whether the supplied summary contains enough persisted context to justify a narrative.
        /// </summary>
        private static bool HasMeaningfulContext(DatasetAnalyticsSummaryDto summary)
        {
            return (summary?.QualityHighlights?.HasProfile ?? false) ||
                   (summary?.TransformationOutcomes?.Count ?? 0) > 0 ||
                   (summary?.AiFindings?.StoredAiResponseCount ?? 0) > 0 ||
                   (summary?.AiFindings?.StoredInsightRecordCount ?? 0) > 0 ||
                   (summary?.MlExperimentHighlights?.HasCompletedExperiment ?? false);
        }

        /// <summary>
        /// Builds the system-level instructions for stakeholder-facing analytics narratives.
        /// </summary>
        private static string BuildSystemInstructions()
        {
            return
                "You are AstraLab's analytics narrator. " +
                "Write a concise, stakeholder-facing narrative using only the provided aggregated analytics summary. " +
                "Do not mention raw rows, hidden system internals, or external facts. " +
                "Do not invent metrics, transformations, or findings. " +
                "Use plain language and keep the output compact. " +
                "Use exactly these section headings in this order: Overview, Key risks, Recent changes, ML highlights, Suggested next steps.";
        }

        /// <summary>
        /// Builds the deterministic aggregated payload supplied to the model.
        /// </summary>
        private static string BuildUserMessage(DatasetAnalyticsSummaryDto summary)
        {
            var payload = new
            {
                dataset = new
                {
                    summary.DatasetId,
                    summary.DatasetName,
                    summary.SourceFormat,
                    summary.DatasetStatus,
                    summary.DatasetVersionId,
                    summary.VersionNumber,
                    summary.VersionType,
                    summary.VersionStatus,
                    summary.ColumnCount,
                    summary.SizeBytes
                },
                qualityHighlights = new
                {
                    summary.QualityHighlights?.HasProfile,
                    summary.QualityHighlights?.RowCount,
                    summary.QualityHighlights?.DuplicateRowCount,
                    summary.QualityHighlights?.DataHealthScore,
                    summary.QualityHighlights?.TotalNullCount,
                    summary.QualityHighlights?.OverallNullPercentage,
                    summary.QualityHighlights?.TotalAnomalyCount,
                    summary.QualityHighlights?.OverallAnomalyPercentage,
                    highRiskColumns = summary.QualityHighlights?.HighRiskColumns
                        ?.Take(5)
                        .Select(item => new
                        {
                            item.Name,
                            item.DataType,
                            item.NullPercentage,
                            item.HasAnomalies,
                            item.AnomalyPercentage
                        })
                        .ToList()
                },
                transformationOutcomes = summary.TransformationOutcomes
                    ?.Take(5)
                    .Select(item => new
                    {
                        item.TransformationType,
                        item.ExecutedAt,
                        item.SummaryPreview
                    })
                    .ToList(),
                aiFindings = new
                {
                    summary.AiFindings?.StoredAiResponseCount,
                    summary.AiFindings?.StoredInsightRecordCount,
                    summary.AiFindings?.HasAutomaticInsight,
                    latestAutomaticInsightPreview = Truncate(summary.AiFindings?.LatestAutomaticInsightPreview),
                    latestManualInsightPreview = Truncate(summary.AiFindings?.LatestManualInsightPreview),
                    latestRecommendationPreview = Truncate(summary.AiFindings?.LatestRecommendationPreview),
                    recentFindings = summary.AiFindings?.RecentFindings
                        ?.Take(3)
                        .Select(item => new
                        {
                            item.Source,
                            item.Title,
                            ContentPreview = Truncate(item.ContentPreview)
                        })
                        .ToList()
                },
                mlExperimentHighlights = new
                {
                    summary.MlExperimentHighlights?.HasCompletedExperiment,
                    summary.MlExperimentHighlights?.AlgorithmKey,
                    summary.MlExperimentHighlights?.TaskType,
                    summary.MlExperimentHighlights?.TargetColumnName,
                    summary.MlExperimentHighlights?.FeatureCount,
                    summary.MlExperimentHighlights?.PrimaryMetricName,
                    summary.MlExperimentHighlights?.PrimaryMetricValue,
                    metrics = summary.MlExperimentHighlights?.Metrics?.Take(5).ToList(),
                    warnings = summary.MlExperimentHighlights?.Warnings?.Take(5).ToList(),
                    topFeatureImportances = summary.MlExperimentHighlights?.TopFeatureImportances
                        ?.Take(5)
                        .Select(item => new
                        {
                            item.ColumnName,
                            item.ImportanceScore,
                            item.Rank
                        })
                        .ToList()
                },
                dashboardSummary = summary.DashboardSummary
            };

            return "Aggregated analytics summary JSON:\n" + JsonSerializer.Serialize(payload, SerializerOptions);
        }

        /// <summary>
        /// Truncates long previews before they are sent to the model.
        /// </summary>
        private static string Truncate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            return normalized.Length <= MaxPreviewLength ? normalized : normalized.Substring(0, MaxPreviewLength) + "...";
        }
    }
}
