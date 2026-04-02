using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.AI;
using AstraLab.Services.Analytics;
using AstraLab.Services.Analytics.Dto;
using AstraLab.Services.Analytics.Storage;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Analytics
{
    public class AnalyticsSummaryAppService_Tests : AstraLabTestBase
    {
        private readonly IAiTextGenerationClient _aiTextGenerationClient;
        private readonly IAnalyticsExportStorage _analyticsExportStorage;
        private readonly IAnalyticsAppService _analyticsAppService;

        public AnalyticsSummaryAppService_Tests()
        {
            _aiTextGenerationClient = Substitute.For<IAiTextGenerationClient>();
            _analyticsExportStorage = Substitute.For<IAnalyticsExportStorage>();

            LocalIocManager.IocContainer.Register(
                Component.For<IAiTextGenerationClient>()
                    .Instance(_aiTextGenerationClient)
                    .IsDefault()
                    .LifestyleSingleton(),
                Component.For<IAnalyticsExportStorage>()
                    .Instance(_analyticsExportStorage)
                    .IsDefault()
                    .LifestyleSingleton());

            _analyticsAppService = Resolve<IAnalyticsAppService>();
        }

        [Fact]
        public async Task GetDatasetAnalyticsSummaryAsync_Should_Return_A_Full_Hybrid_Summary()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-summary-full", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                var seeded = AddProfileData(context, datasetVersion.Id, "target", "feature_one");

                context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = 1,
                    SourceDatasetVersionId = datasetVersion.Id,
                    ResultDatasetVersionId = null,
                    TransformationType = DatasetTransformationType.RemoveDuplicates,
                    ConfigurationJson = "{\"columns\":[\"target\"]}",
                    ExecutionOrder = 1,
                    ExecutedAt = new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc),
                    SummaryJson = "{\"removedRows\":3}"
                });
                context.SaveChanges();

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.UserId.Value,
                    LastInteractionTime = new DateTime(2026, 4, 2, 14, 5, 0, DateTimeKind.Utc)
                }).Entity;
                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Automatic dataset quality insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"profilingCompleted\",\"datasetProfileId\":" + seeded.DatasetProfileId + "}",
                    CreationTime = new DateTime(2026, 4, 2, 14, 6, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Prioritize cleaning target and feature_one.",
                    ResponseType = AIResponseType.Recommendation,
                    CreationTime = new DateTime(2026, 4, 2, 14, 7, 0, DateTimeKind.Utc)
                });

                context.InsightRecords.Add(new InsightRecord
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = seeded.DatasetProfileId,
                    Title = "AI finding",
                    Content = "The current profile shows moderate null exposure.",
                    InsightType = InsightType.DataQuality,
                    InsightSourceType = InsightSourceType.AiGenerated,
                    CreationTime = new DateTime(2026, 4, 2, 14, 8, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();

                var experiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.TargetColumnId, seeded.FeatureColumnId);

                return new
                {
                    DatasetVersionId = datasetVersion.Id,
                    ExperimentId = experiment.Id
                };
            });

            AiTextGenerationRequest capturedRequest = null;
            _aiTextGenerationClient.GenerateTextAsync(Arg.Do<AiTextGenerationRequest>(item => capturedRequest = item))
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nHealthy enough to explore.\n\nKey risks\nNull exposure in target.\n\nRecent changes\nDuplicates were removed.\n\nML highlights\nAccuracy looks solid.\n\nSuggested next steps\nValidate with more data.",
                    Provider = "groq",
                    Model = "llama-test"
                }));

            var result = await _analyticsAppService.GetDatasetAnalyticsSummaryAsync(new EntityDto<long>(scenario.DatasetVersionId));

            result.DatasetVersionId.ShouldBe(scenario.DatasetVersionId);
            result.QualityHighlights.HasProfile.ShouldBeTrue();
            result.QualityHighlights.HighRiskColumns.Count.ShouldBeGreaterThan(0);
            result.TransformationOutcomes.Count.ShouldBe(1);
            result.AiFindings.StoredAiResponseCount.ShouldBe(2);
            result.AiFindings.StoredInsightRecordCount.ShouldBe(1);
            result.AiFindings.HasAutomaticInsight.ShouldBeTrue();
            result.MlExperimentHighlights.HasCompletedExperiment.ShouldBeTrue();
            result.MlExperimentHighlights.MLExperimentId.ShouldBe(scenario.ExperimentId);
            result.MlExperimentHighlights.PrimaryMetricName.ShouldBe("accuracy");
            result.MlExperimentHighlights.PrimaryMetricValue.ShouldBe(0.91m);
            result.DashboardSummary.HasCompletedMlExperiment.ShouldBeTrue();
            result.DashboardSummary.RecentTransformationCount.ShouldBe(1);
            result.Narrative.Status.ShouldBe(AnalyticsNarrativeStatus.Generated);
            result.Narrative.Content.ShouldContain("Overview");

            capturedRequest.ShouldNotBeNull();
            capturedRequest.UserMessage.ShouldContain("Aggregated analytics summary JSON:");
            capturedRequest.UserMessage.ShouldNotContain("SchemaJson");
            capturedRequest.UserMessage.ShouldNotContain("TrainingConfigurationJson");
        }

        [Fact]
        public async Task GetDatasetAnalyticsSummaryAsync_Should_Return_A_Partial_Summary_When_Ai_And_Ml_Data_Are_Absent()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-summary-partial", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                AddProfileData(context, datasetVersion.Id, "amount", "segment");
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nThe dataset is ready for initial review.\n\nKey risks\nSome nulls remain.\n\nRecent changes\nNo recent transformations.\n\nML highlights\nNo completed experiments yet.\n\nSuggested next steps\nProfile columns with the highest nulls."
                }));

            var result = await _analyticsAppService.GetDatasetAnalyticsSummaryAsync(new EntityDto<long>(datasetVersionId));

            result.QualityHighlights.HasProfile.ShouldBeTrue();
            result.TransformationOutcomes.ShouldBeEmpty();
            result.AiFindings.StoredAiResponseCount.ShouldBe(0);
            result.AiFindings.StoredInsightRecordCount.ShouldBe(0);
            result.MlExperimentHighlights.HasCompletedExperiment.ShouldBeFalse();
            result.DashboardSummary.HasCompletedMlExperiment.ShouldBeFalse();
            result.Narrative.Status.ShouldBe(AnalyticsNarrativeStatus.Generated);
        }

        [Fact]
        public async Task GetDatasetDashboardSummaryAsync_Should_Return_A_Compact_View_Without_Invoking_The_Narrative_Path()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-dashboard", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                var seeded = AddProfileData(context, datasetVersion.Id, "target", "feature_one");
                CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.TargetColumnId, seeded.FeatureColumnId);
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns<Task<AiTextGenerationResult>>(_ => throw new InvalidOperationException("Narrative generation should not run for the dashboard endpoint."));

            var result = await _analyticsAppService.GetDatasetDashboardSummaryAsync(new EntityDto<long>(datasetVersionId));

            result.DatasetVersionId.ShouldBe(datasetVersionId);
            result.RowCount.ShouldBe(12);
            result.ColumnCount.ShouldBe(2);
            result.HasCompletedMlExperiment.ShouldBeTrue();
            result.PrimaryMetricName.ShouldBe("accuracy");
            result.PrimaryMetricValue.ShouldBe(0.91m);
        }

        [Fact]
        public async Task GetDatasetAnalyticsSummaryAsync_Should_Not_Fail_When_Narrative_Generation_Fails()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-summary-failing-narrative", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                AddProfileData(context, datasetVersion.Id, "amount", "segment");
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns<Task<AiTextGenerationResult>>(_ => throw new InvalidOperationException("Provider unavailable."));

            var result = await _analyticsAppService.GetDatasetAnalyticsSummaryAsync(new EntityDto<long>(datasetVersionId));

            result.QualityHighlights.HasProfile.ShouldBeTrue();
            result.Narrative.Status.ShouldBe(AnalyticsNarrativeStatus.Failed);
            result.Narrative.Content.ShouldBeNull();
            result.Narrative.FailureMessage.ShouldBe("Analytics narrative generation is currently unavailable.");
        }

        [Fact]
        public async Task GetDatasetAnalyticsSummaryAsync_Should_Reject_A_Dataset_Version_From_A_Different_Owner()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-summary-foreign-owner", AbpSession.UserId.Value + 25);
                return CreateDatasetVersion(context, dataset.Id, 1).Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _analyticsAppService.GetDatasetAnalyticsSummaryAsync(new EntityDto<long>(datasetVersionId)));
        }

        private static Dataset CreateDataset(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name, long ownerUserId)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = 1,
                Name = name,
                Description = name + " description",
                SourceFormat = DatasetFormat.Csv,
                Status = DatasetStatus.Ready,
                OwnerUserId = ownerUserId,
                OriginalFileName = name + ".csv"
            }).Entity;

            context.SaveChanges();
            return dataset;
        }

        private static DatasetVersion CreateDatasetVersion(AstraLab.EntityFrameworkCore.AstraLabDbContext context, long datasetId, int versionNumber)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = 1,
                DatasetId = datasetId,
                VersionNumber = versionNumber,
                VersionType = DatasetVersionType.Raw,
                Status = DatasetVersionStatus.Active,
                RowCount = 12,
                ColumnCount = 2,
                SchemaJson = "{\"columns\":[{\"name\":\"target\"},{\"name\":\"feature_one\"}]}",
                SizeBytes = 512,
                CreationTime = new DateTime(2026, 4, 2, 13, 0, 0, DateTimeKind.Utc)
            }).Entity;

            context.SaveChanges();
            return datasetVersion;
        }

        private static ProfileSeedResult AddProfileData(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetVersionId,
            string targetColumnName,
            string featureColumnName)
        {
            var targetColumn = context.DatasetColumns.Add(new DatasetColumn
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                Name = targetColumnName,
                DataType = "decimal",
                IsDataTypeInferred = true,
                Ordinal = 1,
                NullCount = 3,
                DistinctCount = 8
            }).Entity;

            var featureColumn = context.DatasetColumns.Add(new DatasetColumn
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                Name = featureColumnName,
                DataType = "decimal",
                IsDataTypeInferred = true,
                Ordinal = 2,
                NullCount = 1,
                DistinctCount = 10
            }).Entity;

            context.SaveChanges();

            var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                RowCount = 12,
                DuplicateRowCount = 2,
                DataHealthScore = 78.5m,
                SummaryJson = "{\"totalNullCount\":4,\"overallNullPercentage\":33.3,\"totalAnomalyCount\":2,\"overallAnomalyPercentage\":16.7}"
            }).Entity;

            context.SaveChanges();

            context.DatasetColumnProfiles.Add(new DatasetColumnProfile
            {
                TenantId = 1,
                DatasetProfileId = datasetProfile.Id,
                DatasetColumnId = targetColumn.Id,
                InferredDataType = "decimal",
                NullCount = 3,
                DistinctCount = 8,
                StatisticsJson = "{\"nullPercentage\":25.0,\"mean\":12.5,\"min\":1.0,\"max\":30.0,\"anomalyCount\":2,\"anomalyPercentage\":16.7,\"hasAnomalies\":true}"
            });

            context.DatasetColumnProfiles.Add(new DatasetColumnProfile
            {
                TenantId = 1,
                DatasetProfileId = datasetProfile.Id,
                DatasetColumnId = featureColumn.Id,
                InferredDataType = "decimal",
                NullCount = 1,
                DistinctCount = 10,
                StatisticsJson = "{\"nullPercentage\":8.3,\"mean\":7.5,\"min\":0.5,\"max\":14.0,\"anomalyCount\":0,\"anomalyPercentage\":0.0,\"hasAnomalies\":false}"
            });

            context.SaveChanges();

            return new ProfileSeedResult(targetColumn.Id, featureColumn.Id, datasetProfile.Id);
        }

        private static MLExperiment CreateCompletedMlExperiment(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetVersionId,
            long targetDatasetColumnId,
            long featureDatasetColumnId)
        {
            var experiment = context.MLExperiments.Add(new MLExperiment
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                TargetDatasetColumnId = targetDatasetColumnId,
                Status = MLExperimentStatus.Completed,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = "random_forest_classifier",
                TrainingConfigurationJson = "{\"testSize\":0.2,\"maxDepth\":5}",
                ExecutedAt = new DateTime(2026, 4, 2, 15, 0, 0, DateTimeKind.Utc),
                StartedAtUtc = new DateTime(2026, 4, 2, 15, 1, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 4, 2, 15, 2, 0, DateTimeKind.Utc),
                WarningsJson = "[\"class_imbalance\"]"
            }).Entity;

            context.SaveChanges();

            context.MLExperimentFeatures.Add(new MLExperimentFeature
            {
                TenantId = 1,
                MLExperimentId = experiment.Id,
                DatasetColumnId = featureDatasetColumnId,
                Ordinal = 1
            });

            context.SaveChanges();

            var model = context.MLModels.Add(new MLModel
            {
                TenantId = 1,
                MLExperimentId = experiment.Id,
                ModelType = "random_forest_classifier",
                ArtifactStorageProvider = "local-filesystem",
                ArtifactStorageKey = "ml-artifacts/model.joblib",
                PerformanceSummaryJson = "{\"primaryMetric\":\"accuracy\"}",
                WarningsJson = "[\"small_validation_split\"]"
            }).Entity;

            context.SaveChanges();

            context.MLModelMetrics.Add(new MLModelMetric
            {
                TenantId = 1,
                MLModelId = model.Id,
                MetricName = "accuracy",
                MetricValue = 0.91m
            });

            context.MLModelMetrics.Add(new MLModelMetric
            {
                TenantId = 1,
                MLModelId = model.Id,
                MetricName = "f1",
                MetricValue = 0.88m
            });

            context.MLModelFeatureImportances.Add(new MLModelFeatureImportance
            {
                TenantId = 1,
                MLModelId = model.Id,
                DatasetColumnId = featureDatasetColumnId,
                ImportanceScore = 0.73m,
                Rank = 1
            });

            context.SaveChanges();
            return experiment;
        }

        private class ProfileSeedResult
        {
            public ProfileSeedResult(long targetColumnId, long featureColumnId, long datasetProfileId)
            {
                TargetColumnId = targetColumnId;
                FeatureColumnId = featureColumnId;
                DatasetProfileId = datasetProfileId;
            }

            public long TargetColumnId { get; }

            public long FeatureColumnId { get; }

            public long DatasetProfileId { get; }
        }
    }
}
