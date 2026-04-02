using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using AstraLab.Core.Domains.AI;
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
    public class AnalyticsReportingAppService_Tests : AstraLabTestBase
    {
        private readonly IAiTextGenerationClient _aiTextGenerationClient;
        private readonly InMemoryAnalyticsExportStorage _analyticsExportStorage;
        private readonly IAnalyticsAppService _analyticsAppService;

        public AnalyticsReportingAppService_Tests()
        {
            _aiTextGenerationClient = Substitute.For<IAiTextGenerationClient>();
            _analyticsExportStorage = new InMemoryAnalyticsExportStorage();

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
        public async Task GenerateDatasetReportAsync_Should_Create_A_Persisted_Html_Report()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "report-generation", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                var seeded = AddProfileData(context, datasetVersion.Id, "target", "feature_one");
                CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.TargetColumnId, seeded.FeatureColumnId);
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nThis dataset is ready for stakeholder review.\n\nKey risks\nNull exposure remains visible.\n\nRecent changes\nRecent cleaning has reduced duplicates.\n\nML highlights\nThe latest model is promising.\n\nSuggested next steps\nValidate with a broader sample."
                }));

            var result = await _analyticsAppService.GenerateDatasetReportAsync(new GenerateDatasetReportRequest
            {
                DatasetVersionId = datasetVersionId
            });

            result.DatasetVersionId.ShouldBe(datasetVersionId);
            result.Report.ReportFormat.ShouldBe(Core.Domains.Analytics.ReportFormat.Html);
            result.Report.ReportSourceType.ShouldBe(Core.Domains.Analytics.ReportSourceType.AiGenerated);
            result.Report.Content.ShouldContain("<h2>Dataset quality highlights</h2>");
            result.Report.Content.ShouldContain("<h2>ML highlights</h2>");

            await UsingDbContextAsync(async context =>
            {
                context.ReportRecords.Count().ShouldBe(1);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ExportDatasetReportPdfAsync_Should_Create_A_Pdf_Export_And_Associated_Report()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "report-pdf-export", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                var seeded = AddProfileData(context, datasetVersion.Id, "target", "feature_one");
                CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.TargetColumnId, seeded.FeatureColumnId);
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nA concise exportable report.\n\nKey risks\nSome nulls remain.\n\nRecent changes\nRecent cleaning was applied.\n\nML highlights\nAccuracy is strong.\n\nSuggested next steps\nShare the current summary with stakeholders."
                }));

            var result = await _analyticsAppService.ExportDatasetReportPdfAsync(new ExportDatasetReportPdfRequest
            {
                DatasetVersionId = datasetVersionId
            });

            result.Export.ExportType.ShouldBe(Core.Domains.Analytics.AnalyticsExportType.Document);
            result.Export.ReportRecordId.ShouldBe(result.Report.Id);
            result.Export.ContentType.ShouldBe("application/pdf");
            result.Export.DisplayName.ShouldEndWith(".pdf");

            var storedBytes = _analyticsExportStorage.GetStoredContent(result.Export.StorageKey);
            storedBytes.ShouldNotBeNull();
            System.Text.Encoding.ASCII.GetString(storedBytes.Take(4).ToArray()).ShouldBe("%PDF");
        }

        [Fact]
        public async Task ExportDatasetInsightsCsvAsync_Should_Create_A_Csv_Export_Without_Raw_Rows()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "report-csv-export", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                var seeded = AddProfileData(context, datasetVersion.Id, "target", "feature_one");

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.UserId.Value,
                    LastInteractionTime = DateTime.UtcNow
                }).Entity;
                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Automatic stakeholder insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"profilingCompleted\",\"datasetProfileId\":" + seeded.DatasetProfileId + "}"
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nCSV-ready narrative.\n\nKey risks\nNulls remain.\n\nRecent changes\nNo transformations yet.\n\nML highlights\nNo ML run yet.\n\nSuggested next steps\nProfile again after cleaning."
                }));

            var result = await _analyticsAppService.ExportDatasetInsightsCsvAsync(new ExportDatasetInsightsCsvRequest
            {
                DatasetVersionId = datasetVersionId
            });

            result.Export.ExportType.ShouldBe(Core.Domains.Analytics.AnalyticsExportType.Spreadsheet);
            result.Export.ContentType.ShouldBe("text/csv");

            var csv = System.Text.Encoding.UTF8.GetString(_analyticsExportStorage.GetStoredContent(result.Export.StorageKey));
            csv.ShouldContain("section,itemType,name,value,detail,secondaryValue");
            csv.ShouldContain("quality,highRiskColumn,target");
            csv.ShouldContain("ai,AIResponse,Insight");
            csv.ShouldNotContain("row 1");
        }

        [Fact]
        public async Task ExportDatasetReportPdfAsync_Should_Not_Persist_Partial_Records_When_Storage_Fails()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "report-storage-failure", AbpSession.UserId.Value);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
                AddProfileData(context, datasetVersion.Id, "target", "feature_one");
                return datasetVersion.Id;
            });

            _analyticsExportStorage.FailOnStore = true;
            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Overview\nFailure path.\n\nKey risks\nStorage issue.\n\nRecent changes\nNone.\n\nML highlights\nNone.\n\nSuggested next steps\nRetry later."
                }));

            await Should.ThrowAsync<InvalidOperationException>(() =>
                _analyticsAppService.ExportDatasetReportPdfAsync(new ExportDatasetReportPdfRequest
                {
                    DatasetVersionId = datasetVersionId
                }));

            await UsingDbContextAsync(async context =>
            {
                context.ReportRecords.Count().ShouldBe(0);
                context.AnalyticsExports.Count().ShouldBe(0);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GenerateDatasetReportAsync_Should_Reject_A_Dataset_Version_From_A_Different_Owner()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "report-foreign-owner", AbpSession.UserId.Value + 99);
                return CreateDatasetVersion(context, dataset.Id, 1).Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _analyticsAppService.GenerateDatasetReportAsync(new GenerateDatasetReportRequest
                {
                    DatasetVersionId = datasetVersionId
                }));
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
                SizeBytes = 512
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
                TrainingConfigurationJson = "{\"testSize\":0.2}",
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

        private class InMemoryAnalyticsExportStorage : IAnalyticsExportStorage
        {
            private readonly Dictionary<string, byte[]> _contents = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            public bool FailOnStore { get; set; }

            public Task<StoredAnalyticsExportResult> StoreAsync(StoreAnalyticsExportRequest request)
            {
                if (FailOnStore)
                {
                    throw new InvalidOperationException("Analytics export storage is unavailable.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    request.Content.CopyTo(memoryStream);
                    _contents[request.StorageKey] = memoryStream.ToArray();
                }

                return Task.FromResult(new StoredAnalyticsExportResult
                {
                    StorageProvider = "memory",
                    StorageKey = request.StorageKey
                });
            }

            public Task<Stream> OpenReadAsync(OpenReadAnalyticsExportRequest request)
            {
                return Task.FromResult<Stream>(new MemoryStream(GetStoredContent(request.StorageKey), writable: false));
            }

            public Task DeleteAsync(DeleteAnalyticsExportRequest request)
            {
                _contents.Remove(request.StorageKey);
                return Task.CompletedTask;
            }

            public byte[] GetStoredContent(string storageKey)
            {
                return _contents.TryGetValue(storageKey, out var content)
                    ? content
                    : null;
            }
        }
    }
}
