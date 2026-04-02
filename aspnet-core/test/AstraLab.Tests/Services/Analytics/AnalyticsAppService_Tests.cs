using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.UI;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.Analytics;
using AstraLab.Services.Analytics.Dto;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Analytics
{
    public class AnalyticsAppService_Tests : AstraLabTestBase
    {
        private readonly IAnalyticsAppService _analyticsAppService;

        public AnalyticsAppService_Tests()
        {
            _analyticsAppService = Resolve<IAnalyticsAppService>();
        }

        [Fact]
        public async Task GetInsightAsync_Should_Return_A_Tenant_Owned_Insight()
        {
            var insightId = UsingDbContext(context =>
            {
                var scenario = CreateScenario(context, "analytics-get-insight", AbpSession.UserId.Value);
                var insight = context.InsightRecords.Add(new InsightRecord
                {
                    TenantId = 1,
                    DatasetVersionId = scenario.DatasetVersionId,
                    DatasetProfileId = scenario.DatasetProfileId,
                    AIResponseId = scenario.AIResponseId,
                    Title = "Owned insight",
                    Content = "Owned content",
                    InsightType = InsightType.Summary,
                    InsightSourceType = InsightSourceType.AiGenerated
                }).Entity;

                context.SaveChanges();
                return insight.Id;
            });

            var result = await _analyticsAppService.GetInsightAsync(new EntityDto<long>(insightId));

            result.Title.ShouldBe("Owned insight");
            result.InsightType.ShouldBe(InsightType.Summary);
        }

        [Fact]
        public async Task GetInsightsAsync_Should_Filter_By_Profile_And_Source_Type()
        {
            var scenario = UsingDbContext(context =>
            {
                var created = CreateScenario(context, "analytics-list-insights", AbpSession.UserId.Value);

                context.InsightRecords.Add(new InsightRecord
                {
                    TenantId = 1,
                    DatasetVersionId = created.DatasetVersionId,
                    DatasetProfileId = created.DatasetProfileId,
                    Title = "AI insight",
                    Content = "AI content",
                    InsightType = InsightType.DataQuality,
                    InsightSourceType = InsightSourceType.AiGenerated,
                    CreationTime = new DateTime(2026, 4, 2, 15, 0, 0, DateTimeKind.Utc)
                });

                context.InsightRecords.Add(new InsightRecord
                {
                    TenantId = 1,
                    DatasetVersionId = created.DatasetVersionId,
                    Title = "System insight",
                    Content = "System content",
                    InsightType = InsightType.Pattern,
                    InsightSourceType = InsightSourceType.SystemGenerated,
                    CreationTime = new DateTime(2026, 4, 2, 15, 10, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return created;
            });

            var result = await _analyticsAppService.GetInsightsAsync(new GetInsightsRequest
            {
                DatasetVersionId = scenario.DatasetVersionId,
                DatasetProfileId = scenario.DatasetProfileId,
                InsightSourceType = InsightSourceType.AiGenerated,
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.TotalCount.ShouldBe(1);
            result.Items.Single().Title.ShouldBe("AI insight");
        }

        [Fact]
        public async Task GetReportAsync_Should_Reject_A_Report_From_A_Different_Owner()
        {
            var reportId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "analytics-foreign-report", ownerUserId: AbpSession.UserId.Value + 10);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);

                var report = context.ReportRecords.Add(new ReportRecord
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Title = "Foreign report",
                    Content = "Hidden",
                    ReportFormat = ReportFormat.Markdown,
                    ReportSourceType = ReportSourceType.SystemGenerated
                }).Entity;

                context.SaveChanges();
                return report.Id;
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _analyticsAppService.GetReportAsync(new EntityDto<long>(reportId)));

            exception.Message.ShouldBe("The requested analytics report could not be found.");
        }

        [Fact]
        public async Task GetExportsAsync_Should_Filter_By_Experiment_And_Export_Type()
        {
            var scenario = UsingDbContext(context =>
            {
                var created = CreateScenario(context, "analytics-list-exports", AbpSession.UserId.Value);

                var otherExperiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = created.DatasetVersionId,
                    Status = MLExperimentStatus.Completed,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "xgboost_classifier",
                    TrainingConfigurationJson = "{\"testSize\":0.3}",
                    ExecutedAt = new DateTime(2026, 4, 2, 16, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AnalyticsExports.Add(new AnalyticsExport
                {
                    TenantId = 1,
                    DatasetVersionId = created.DatasetVersionId,
                    MLExperimentId = created.MLExperimentId,
                    ExportType = AnalyticsExportType.Document,
                    DisplayName = "report.pdf",
                    StorageProvider = "s3-compatible",
                    StorageKey = "analytics/report.pdf",
                    CreationTime = new DateTime(2026, 4, 2, 16, 1, 0, DateTimeKind.Utc)
                });

                context.AnalyticsExports.Add(new AnalyticsExport
                {
                    TenantId = 1,
                    DatasetVersionId = created.DatasetVersionId,
                    MLExperimentId = otherExperiment.Id,
                    ExportType = AnalyticsExportType.Spreadsheet,
                    DisplayName = "report.xlsx",
                    StorageProvider = "s3-compatible",
                    StorageKey = "analytics/report.xlsx",
                    CreationTime = new DateTime(2026, 4, 2, 16, 2, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return created;
            });

            var result = await _analyticsAppService.GetExportsAsync(new GetAnalyticsExportsRequest
            {
                DatasetVersionId = scenario.DatasetVersionId,
                MLExperimentId = scenario.MLExperimentId,
                ExportType = AnalyticsExportType.Document,
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.TotalCount.ShouldBe(1);
            result.Items.Single().DisplayName.ShouldBe("report.pdf");
        }

        private static AnalyticsScenario CreateScenario(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name, long ownerUserId)
        {
            var dataset = CreateDataset(context, name, ownerUserId);
            var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1);
            var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
            {
                TenantId = 1,
                DatasetVersionId = datasetVersion.Id,
                RowCount = 10,
                DuplicateRowCount = 0,
                DataHealthScore = 90m
            }).Entity;

            context.SaveChanges();

            var conversation = context.AIConversations.Add(new AIConversation
            {
                TenantId = 1,
                DatasetId = dataset.Id,
                OwnerUserId = dataset.OwnerUserId,
                LastInteractionTime = new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc)
            }).Entity;

            context.SaveChanges();

            var aiResponse = context.AIResponses.Add(new AIResponse
            {
                TenantId = 1,
                AIConversationId = conversation.Id,
                DatasetVersionId = datasetVersion.Id,
                ResponseContent = "AI summary",
                ResponseType = AIResponseType.Summary
            }).Entity;

            context.SaveChanges();

            var experiment = context.MLExperiments.Add(new MLExperiment
            {
                TenantId = 1,
                DatasetVersionId = datasetVersion.Id,
                Status = MLExperimentStatus.Completed,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = "random_forest_classifier",
                TrainingConfigurationJson = "{\"testSize\":0.2}",
                ExecutedAt = new DateTime(2026, 4, 2, 14, 10, 0, DateTimeKind.Utc)
            }).Entity;

            context.SaveChanges();

            return new AnalyticsScenario(datasetVersion.Id, datasetProfile.Id, aiResponse.Id, experiment.Id);
        }

        private static Dataset CreateDataset(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name, long ownerUserId)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = 1,
                Name = name,
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
                RowCount = 10,
                ColumnCount = 1,
                SizeBytes = 100
            }).Entity;

            context.SaveChanges();
            return datasetVersion;
        }

        private class AnalyticsScenario
        {
            public AnalyticsScenario(long datasetVersionId, long datasetProfileId, long aiResponseId, long mlExperimentId)
            {
                DatasetVersionId = datasetVersionId;
                DatasetProfileId = datasetProfileId;
                AIResponseId = aiResponseId;
                MLExperimentId = mlExperimentId;
            }

            public long DatasetVersionId { get; }

            public long DatasetProfileId { get; }

            public long AIResponseId { get; }

            public long MLExperimentId { get; }
        }
    }
}
