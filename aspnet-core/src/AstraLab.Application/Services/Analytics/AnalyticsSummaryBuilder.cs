using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.AI;
using AstraLab.Services.Analytics.Dto;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Builds unified analytics summaries by aggregating profiling, transformation, AI, and machine-learning outputs.
    /// </summary>
    public class AnalyticsSummaryBuilder : IAnalyticsSummaryBuilder, ITransientDependency
    {
        private const int MaxHighRiskColumns = 5;
        private const int MaxTransformationOutcomes = 5;
        private const int MaxRecentFindings = 3;
        private const int MaxMetrics = 5;
        private const int MaxFeatureImportances = 5;
        private const int MaxPreviewLength = 240;

        private readonly IRepository<DatasetVersion, long> _datasetVersionRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IRepository<InsightRecord, long> _insightRecordRepository;
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;
        private readonly IAiDatasetContextBuilder _aiDatasetContextBuilder;
        private readonly IAiDatasetInsightReader _aiDatasetInsightReader;
        private readonly IAiMlExperimentContextBuilder _aiMlExperimentContextBuilder;
        private readonly IAnalyticsNarrativeGenerator _analyticsNarrativeGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSummaryBuilder"/> class.
        /// </summary>
        public AnalyticsSummaryBuilder(
            IRepository<DatasetVersion, long> datasetVersionRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IRepository<InsightRecord, long> insightRecordRepository,
            IRepository<MLExperiment, long> mlExperimentRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker,
            IAiDatasetContextBuilder aiDatasetContextBuilder,
            IAiDatasetInsightReader aiDatasetInsightReader,
            IAiMlExperimentContextBuilder aiMlExperimentContextBuilder,
            IAnalyticsNarrativeGenerator analyticsNarrativeGenerator)
        {
            _datasetVersionRepository = datasetVersionRepository;
            _aiResponseRepository = aiResponseRepository;
            _insightRecordRepository = insightRecordRepository;
            _mlExperimentRepository = mlExperimentRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
            _aiDatasetContextBuilder = aiDatasetContextBuilder;
            _aiDatasetInsightReader = aiDatasetInsightReader;
            _aiMlExperimentContextBuilder = aiMlExperimentContextBuilder;
            _analyticsNarrativeGenerator = analyticsNarrativeGenerator;
        }

        /// <summary>
        /// Builds the full unified analytics summary for the selected dataset version.
        /// </summary>
        public async Task<DatasetAnalyticsSummaryDto> BuildAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var summary = await BuildDeterministicSummaryAsync(datasetVersionId, tenantId, ownerUserId);
            summary.Narrative = await _analyticsNarrativeGenerator.GenerateAsync(summary);
            return summary;
        }

        /// <summary>
        /// Builds the compact dashboard summary for the selected dataset version.
        /// </summary>
        public async Task<AnalyticsDashboardSummaryDto> BuildDashboardAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            var summary = await BuildDeterministicSummaryAsync(datasetVersionId, tenantId, ownerUserId);
            return summary.DashboardSummary;
        }

        // #region private methods

        /// <summary>
        /// Builds the deterministic portion of the unified analytics summary without AI narrative generation.
        /// </summary>
        private async Task<DatasetAnalyticsSummaryDto> BuildDeterministicSummaryAsync(long datasetVersionId, int tenantId, long ownerUserId)
        {
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);

            var datasetVersion = await _datasetVersionRepository.GetAll()
                .Include(item => item.Dataset)
                .Where(item => item.Id == datasetVersionId && item.TenantId == tenantId)
                .SingleAsync();

            var datasetContext = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);
            var datasetInsightContext = await _aiDatasetInsightReader.ReadAsync(datasetVersionId, tenantId, ownerUserId);
            var latestCompletedExperiment = await GetLatestCompletedExperimentAsync(datasetVersionId, tenantId);
            var mlExperimentContext = latestCompletedExperiment == null
                ? null
                : await _aiMlExperimentContextBuilder.BuildAsync(latestCompletedExperiment.Id, tenantId, ownerUserId);

            var aiResponses = await _aiResponseRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            var insightRecords = await _insightRecordRepository.GetAll()
                .Where(item => item.TenantId == tenantId && item.DatasetVersionId == datasetVersionId)
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            var qualityHighlights = BuildQualityHighlights(datasetContext, datasetInsightContext);
            var transformationOutcomes = BuildTransformationOutcomes(datasetInsightContext);
            var aiFindings = BuildAiFindings(aiResponses, insightRecords);
            var mlHighlights = BuildMlHighlights(latestCompletedExperiment, mlExperimentContext);
            var dashboardSummary = BuildDashboardSummary(datasetVersionId, datasetVersion.ColumnCount, qualityHighlights, transformationOutcomes, aiFindings, mlHighlights);

            return new DatasetAnalyticsSummaryDto
            {
                DatasetId = datasetVersion.DatasetId,
                DatasetName = datasetVersion.Dataset?.Name,
                SourceFormat = datasetVersion.Dataset?.SourceFormat ?? default,
                DatasetStatus = datasetVersion.Dataset?.Status ?? default,
                DatasetVersionId = datasetVersion.Id,
                VersionNumber = datasetVersion.VersionNumber,
                VersionType = datasetVersion.VersionType,
                VersionStatus = datasetVersion.Status,
                ColumnCount = datasetVersion.ColumnCount,
                SizeBytes = datasetVersion.SizeBytes,
                QualityHighlights = qualityHighlights,
                TransformationOutcomes = transformationOutcomes,
                AiFindings = aiFindings,
                MlExperimentHighlights = mlHighlights,
                DashboardSummary = dashboardSummary,
                Narrative = new AnalyticsNarrativeDto
                {
                    Status = AnalyticsNarrativeStatus.Unavailable
                }
            };
        }

        /// <summary>
        /// Builds the dataset-quality highlights from the existing AI dataset context and enrichment readers.
        /// </summary>
        private static DatasetQualityHighlightsDto BuildQualityHighlights(AiDatasetContext datasetContext, AiDatasetInsightContext datasetInsightContext)
        {
            return new DatasetQualityHighlightsDto
            {
                HasProfile = datasetContext?.Profiling != null,
                DatasetProfileId = datasetContext?.Profiling?.ProfileId,
                RowCount = datasetContext?.Profiling?.RowCount,
                DuplicateRowCount = datasetInsightContext?.DuplicateRowCount,
                DataHealthScore = datasetInsightContext?.DataHealthScore,
                TotalNullCount = datasetInsightContext?.TotalNullCount,
                OverallNullPercentage = datasetInsightContext?.OverallNullPercentage,
                TotalAnomalyCount = datasetInsightContext?.TotalAnomalyCount,
                OverallAnomalyPercentage = datasetInsightContext?.OverallAnomalyPercentage,
                HighRiskColumns = (datasetInsightContext?.HighSignalColumns ?? Array.Empty<AiInsightColumnContext>())
                    .Take(MaxHighRiskColumns)
                    .Select(item => new DatasetQualityColumnHighlightDto
                    {
                        DatasetColumnId = item.DatasetColumnId,
                        Name = item.Name,
                        DataType = item.DataType,
                        NullCount = item.NullCount,
                        NullPercentage = item.NullPercentage,
                        DistinctCount = item.DistinctCount,
                        HasAnomalies = item.HasAnomalies,
                        AnomalyCount = item.AnomalyCount,
                        AnomalyPercentage = item.AnomalyPercentage,
                        Mean = item.Mean,
                        Min = item.Min,
                        Max = item.Max
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Builds the recent transformation outcomes from the existing dataset insight enrichment.
        /// </summary>
        private static IReadOnlyList<TransformationOutcomeSummaryDto> BuildTransformationOutcomes(AiDatasetInsightContext datasetInsightContext)
        {
            return (datasetInsightContext?.RecentTransformations ?? Array.Empty<AiTransformationHistoryContext>())
                .Take(MaxTransformationOutcomes)
                .Select(item => new TransformationOutcomeSummaryDto
                {
                    DatasetTransformationId = item.DatasetTransformationId,
                    TransformationType = item.TransformationType,
                    SourceDatasetVersionId = item.SourceDatasetVersionId,
                    ResultDatasetVersionId = item.ResultDatasetVersionId,
                    ExecutionOrder = item.ExecutionOrder,
                    ExecutedAt = item.ExecutedAt,
                    SummaryPreview = BuildSummaryPreview(item.SummaryJson)
                })
                .ToList();
        }

        /// <summary>
        /// Builds the persisted AI findings section from AI responses and analytics insight records.
        /// </summary>
        private static AiFindingsSummaryDto BuildAiFindings(IReadOnlyList<AIResponse> aiResponses, IReadOnlyList<InsightRecord> insightRecords)
        {
            var latestAutomaticInsight = aiResponses.FirstOrDefault(item =>
                item.ResponseType == AIResponseType.Insight &&
                (AiAutomaticInsightMetadata.IsAutomaticProfilingInsight(item.MetadataJson) ||
                 AiAutomaticInsightMetadata.IsAutomaticExperimentInsight(item.MetadataJson)));

            var latestManualInsightResponse = aiResponses.FirstOrDefault(item =>
                item.ResponseType == AIResponseType.Insight &&
                !AiAutomaticInsightMetadata.IsAutomaticProfilingInsight(item.MetadataJson) &&
                !AiAutomaticInsightMetadata.IsAutomaticExperimentInsight(item.MetadataJson));

            var latestAiGeneratedInsightRecord = insightRecords.FirstOrDefault(item => item.InsightSourceType == InsightSourceType.AiGenerated);
            var latestRecommendation = aiResponses.FirstOrDefault(item => item.ResponseType == AIResponseType.Recommendation);

            var latestFindingTime = aiResponses
                .Select(item => (DateTime?)item.CreationTime)
                .Concat(insightRecords.Select(item => (DateTime?)item.CreationTime))
                .OrderByDescending(item => item)
                .FirstOrDefault();

            return new AiFindingsSummaryDto
            {
                StoredAiResponseCount = aiResponses.Count,
                StoredInsightRecordCount = insightRecords.Count,
                HasAutomaticInsight = latestAutomaticInsight != null,
                LatestAutomaticInsightPreview = BuildPreview(latestAutomaticInsight?.ResponseContent),
                LatestManualInsightPreview = BuildPreview(latestManualInsightResponse?.ResponseContent ?? latestAiGeneratedInsightRecord?.Content),
                LatestRecommendationPreview = BuildPreview(latestRecommendation?.ResponseContent),
                LatestFindingTime = latestFindingTime,
                RecentFindings = BuildRecentFindings(aiResponses, insightRecords)
            };
        }

        /// <summary>
        /// Builds the most recent persisted AI finding previews.
        /// </summary>
        private static IReadOnlyList<AiFindingPreviewDto> BuildRecentFindings(IReadOnlyList<AIResponse> aiResponses, IReadOnlyList<InsightRecord> insightRecords)
        {
            var aiResponseFindings = aiResponses.Select(item => new AiFindingPreviewDto
            {
                Source = "AIResponse",
                Title = item.ResponseType.ToString(),
                ContentPreview = BuildPreview(item.ResponseContent),
                CreationTime = item.CreationTime
            });

            var insightRecordFindings = insightRecords.Select(item => new AiFindingPreviewDto
            {
                Source = "InsightRecord",
                Title = item.Title,
                ContentPreview = BuildPreview(item.Content),
                CreationTime = item.CreationTime
            });

            return aiResponseFindings
                .Concat(insightRecordFindings)
                .OrderByDescending(item => item.CreationTime)
                .Take(MaxRecentFindings)
                .ToList();
        }

        /// <summary>
        /// Builds the latest completed machine-learning experiment highlights.
        /// </summary>
        private static MlExperimentHighlightsDto BuildMlHighlights(MLExperiment latestCompletedExperiment, AiMlExperimentContext mlExperimentContext)
        {
            if (latestCompletedExperiment == null || mlExperimentContext == null)
            {
                return new MlExperimentHighlightsDto();
            }

            var metrics = (mlExperimentContext.Metrics ?? Array.Empty<AiMlMetricContext>())
                .Take(MaxMetrics)
                .Select(item => new AnalyticsMlMetricDto
                {
                    MetricName = item.MetricName,
                    MetricValue = item.MetricValue
                })
                .ToList();

            var featureImportances = (mlExperimentContext.FeatureImportances ?? Array.Empty<AiMlFeatureImportanceContext>())
                .Take(MaxFeatureImportances)
                .Select(item => new AnalyticsMlFeatureImportanceDto
                {
                    DatasetColumnId = item.DatasetColumnId,
                    ColumnName = item.ColumnName,
                    ImportanceScore = item.ImportanceScore,
                    Rank = item.Rank
                })
                .ToList();

            var primaryMetricName = ResolvePrimaryMetricName(mlExperimentContext);
            var primaryMetric = metrics.FirstOrDefault(item =>
                string.Equals(item.MetricName, primaryMetricName, StringComparison.OrdinalIgnoreCase)) ??
                metrics.FirstOrDefault();

            return new MlExperimentHighlightsDto
            {
                HasCompletedExperiment = true,
                MLExperimentId = latestCompletedExperiment.Id,
                Status = mlExperimentContext.Status,
                TaskType = mlExperimentContext.TaskType,
                AlgorithmKey = mlExperimentContext.AlgorithmKey,
                ModelType = mlExperimentContext.ModelType,
                TargetColumnName = mlExperimentContext.TargetColumnName,
                FeatureCount = mlExperimentContext.FeatureNames?.Count ?? 0,
                FeatureNames = (mlExperimentContext.FeatureNames ?? Array.Empty<string>()).Take(MaxFeatureImportances).ToList(),
                Metrics = metrics,
                PrimaryMetricName = primaryMetric?.MetricName,
                PrimaryMetricValue = primaryMetric?.MetricValue,
                TopFeatureImportances = featureImportances,
                Warnings = (mlExperimentContext.Warnings ?? Array.Empty<string>()).Take(MaxFeatureImportances).ToList(),
                ExecutedAt = latestCompletedExperiment.CompletedAtUtc ?? latestCompletedExperiment.ExecutedAt,
                HasModelOutput = mlExperimentContext.HasModelOutput
            };
        }

        /// <summary>
        /// Builds the compact dashboard summary from the deterministic section outputs.
        /// </summary>
        private static AnalyticsDashboardSummaryDto BuildDashboardSummary(
            long datasetVersionId,
            int? columnCount,
            DatasetQualityHighlightsDto qualityHighlights,
            IReadOnlyList<TransformationOutcomeSummaryDto> transformationOutcomes,
            AiFindingsSummaryDto aiFindings,
            MlExperimentHighlightsDto mlHighlights)
        {
            return new AnalyticsDashboardSummaryDto
            {
                DatasetVersionId = datasetVersionId,
                DataHealthScore = qualityHighlights?.DataHealthScore,
                RowCount = qualityHighlights?.RowCount,
                ColumnCount = columnCount,
                HighRiskColumnCount = qualityHighlights?.HighRiskColumns?.Count ?? 0,
                RecentTransformationCount = transformationOutcomes?.Count ?? 0,
                StoredInsightCount = aiFindings?.StoredInsightRecordCount ?? 0,
                StoredAiResponseCount = aiFindings?.StoredAiResponseCount ?? 0,
                HasAutomaticAiInsight = aiFindings?.HasAutomaticInsight ?? false,
                HasCompletedMlExperiment = mlHighlights?.HasCompletedExperiment ?? false,
                PrimaryMetricName = mlHighlights?.PrimaryMetricName,
                PrimaryMetricValue = mlHighlights?.PrimaryMetricValue,
                MlWarningCount = mlHighlights?.Warnings?.Count ?? 0
            };
        }

        /// <summary>
        /// Gets the latest completed experiment for the selected dataset version.
        /// </summary>
        private async Task<MLExperiment> GetLatestCompletedExperimentAsync(long datasetVersionId, int tenantId)
        {
            return await _mlExperimentRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == datasetVersionId &&
                    item.Status == MLExperimentStatus.Completed)
                .OrderByDescending(item => item.CompletedAtUtc ?? item.ExecutedAt)
                .ThenByDescending(item => item.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Resolves the primary metric name from compact model summary metadata when available.
        /// </summary>
        private static string ResolvePrimaryMetricName(AiMlExperimentContext mlExperimentContext)
        {
            if (string.IsNullOrWhiteSpace(mlExperimentContext?.PerformanceSummaryJson))
            {
                return null;
            }

            try
            {
                using (var document = JsonDocument.Parse(mlExperimentContext.PerformanceSummaryJson))
                {
                    if (document.RootElement.TryGetProperty("primaryMetric", out var property) &&
                        property.ValueKind == JsonValueKind.String)
                    {
                        return property.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // ignore malformed compact summary payloads and fall back to the metric list
            }

            return null;
        }

        /// <summary>
        /// Builds a compact preview from persisted long-form content.
        /// </summary>
        private static string BuildPreview(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            return normalized.Length <= MaxPreviewLength ? normalized : normalized.Substring(0, MaxPreviewLength) + "...";
        }

        /// <summary>
        /// Builds a compact preview from serialized transformation summary payloads.
        /// </summary>
        private static string BuildSummaryPreview(string summaryJson)
        {
            if (string.IsNullOrWhiteSpace(summaryJson))
            {
                return null;
            }

            try
            {
                using (var document = JsonDocument.Parse(summaryJson))
                {
                    var compact = JsonSerializer.Serialize(document.RootElement);
                    return BuildPreview(compact);
                }
            }
            catch (JsonException)
            {
                return BuildPreview(summaryJson);
            }
        }

        // #endregion private methods
    }
}
