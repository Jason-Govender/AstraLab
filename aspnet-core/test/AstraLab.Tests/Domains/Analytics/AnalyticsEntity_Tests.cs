using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Domains.Analytics
{
    public class AnalyticsEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_Insight_Report_And_Export_With_The_Full_Link_Graph()
        {
            long insightId = 0;
            long reportId = 0;
            long exportId = 0;
            long datasetProfileId = 0;
            long aiResponseId = 0;
            long mlExperimentId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = await CreateDatasetAsync(context, "analytics-foundation-dataset");
                var datasetVersion = await CreateDatasetVersionAsync(context, dataset.Id, 1, DatasetVersionType.Raw);
                var datasetProfile = await CreateDatasetProfileAsync(context, datasetVersion.Id);
                var aiResponse = await CreateAiResponseAsync(context, dataset, datasetVersion.Id);
                var mlExperiment = await CreateMlExperimentAsync(context, datasetVersion.Id);

                var insight = context.InsightRecords.Add(new InsightRecord
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = datasetProfile.Id,
                    MLExperimentId = mlExperiment.Id,
                    AIResponseId = aiResponse.Id,
                    Title = "Data quality insight",
                    Content = "Nulls in the amount column need attention.",
                    InsightType = InsightType.DataQuality,
                    InsightSourceType = InsightSourceType.AiGenerated,
                    MetadataJson = "{\"source\":\"profile\"}"
                }).Entity;

                var report = context.ReportRecords.Add(new ReportRecord
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = datasetProfile.Id,
                    MLExperimentId = mlExperiment.Id,
                    AIResponseId = aiResponse.Id,
                    Title = "Stakeholder report",
                    Summary = "Short summary",
                    Content = "# Report",
                    ReportFormat = ReportFormat.Markdown,
                    ReportSourceType = ReportSourceType.SystemGenerated,
                    MetadataJson = "{\"audience\":\"exec\"}"
                }).Entity;

                await context.SaveChangesAsync();

                var analyticsExport = context.AnalyticsExports.Add(new AnalyticsExport
                {
                    TenantId = dataset.TenantId,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = mlExperiment.Id,
                    InsightRecordId = insight.Id,
                    ReportRecordId = report.Id,
                    ExportType = AnalyticsExportType.Document,
                    DisplayName = "stakeholder-report.pdf",
                    StorageProvider = "s3-compatible",
                    StorageKey = "analytics/test/report.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 1024,
                    ChecksumSha256 = new string('a', AnalyticsExport.ChecksumSha256Length),
                    MetadataJson = "{\"exportedBy\":\"system\"}"
                }).Entity;

                await context.SaveChangesAsync();

                insightId = insight.Id;
                reportId = report.Id;
                exportId = analyticsExport.Id;
                datasetProfileId = datasetProfile.Id;
                aiResponseId = aiResponse.Id;
                mlExperimentId = mlExperiment.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var insight = await context.InsightRecords
                    .Include(item => item.DatasetProfile)
                    .Include(item => item.AIResponse)
                    .Include(item => item.MLExperiment)
                    .SingleAsync(item => item.Id == insightId);

                var report = await context.ReportRecords
                    .Include(item => item.DatasetProfile)
                    .Include(item => item.AIResponse)
                    .Include(item => item.MLExperiment)
                    .SingleAsync(item => item.Id == reportId);

                var analyticsExport = await context.AnalyticsExports
                    .Include(item => item.InsightRecord)
                    .Include(item => item.ReportRecord)
                    .Include(item => item.MLExperiment)
                    .SingleAsync(item => item.Id == exportId);

                insight.DatasetProfileId.ShouldBe(datasetProfileId);
                insight.AIResponseId.ShouldBe(aiResponseId);
                insight.MLExperimentId.ShouldBe(mlExperimentId);

                report.DatasetProfileId.ShouldBe(datasetProfileId);
                report.AIResponseId.ShouldBe(aiResponseId);
                report.MLExperimentId.ShouldBe(mlExperimentId);

                analyticsExport.InsightRecordId.ShouldBe(insightId);
                analyticsExport.ReportRecordId.ShouldBe(reportId);
                analyticsExport.MLExperimentId.ShouldBe(mlExperimentId);
            });
        }

        [Fact]
        public void Should_Define_The_Expected_Analytics_Indexes_In_Model()
        {
            UsingDbContext(context =>
            {
                var insightEntityType = context.Model.FindEntityType(typeof(InsightRecord));
                var reportEntityType = context.Model.FindEntityType(typeof(ReportRecord));
                var exportEntityType = context.Model.FindEntityType(typeof(AnalyticsExport));

                insightEntityType.GetIndexes().Any(item => item.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(InsightRecord.TenantId),
                        nameof(InsightRecord.DatasetVersionId),
                        nameof(InsightRecord.CreationTime)
                    })).ShouldBeTrue();

                reportEntityType.GetIndexes().Any(item => item.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(ReportRecord.TenantId),
                        nameof(ReportRecord.ReportSourceType),
                        nameof(ReportRecord.ReportFormat),
                        nameof(ReportRecord.CreationTime)
                    })).ShouldBeTrue();

                exportEntityType.GetIndexes().Any(item => item.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(AnalyticsExport.StorageProvider),
                        nameof(AnalyticsExport.StorageKey)
                    }) && item.IsUnique).ShouldBeTrue();
            });
        }

        [Fact]
        public void Should_Define_The_Expected_Analytics_Delete_Behaviors_In_Model()
        {
            UsingDbContext(context =>
            {
                var insightEntityType = context.Model.FindEntityType(typeof(InsightRecord));
                var reportEntityType = context.Model.FindEntityType(typeof(ReportRecord));
                var exportEntityType = context.Model.FindEntityType(typeof(AnalyticsExport));

                insightEntityType.GetForeignKeys().Single(item =>
                    item.Properties.Single().Name == nameof(InsightRecord.DatasetVersionId) &&
                    item.PrincipalEntityType.ClrType == typeof(DatasetVersion))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);

                insightEntityType.GetForeignKeys().Single(item =>
                    item.Properties.Single().Name == nameof(InsightRecord.DatasetProfileId) &&
                    item.PrincipalEntityType.ClrType == typeof(DatasetProfile))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);

                reportEntityType.GetForeignKeys().Single(item =>
                    item.Properties.Single().Name == nameof(ReportRecord.AIResponseId) &&
                    item.PrincipalEntityType.ClrType == typeof(AIResponse))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);

                exportEntityType.GetForeignKeys().Single(item =>
                    item.Properties.Single().Name == nameof(AnalyticsExport.InsightRecordId) &&
                    item.PrincipalEntityType.ClrType == typeof(InsightRecord))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);

                exportEntityType.GetForeignKeys().Single(item =>
                    item.Properties.Single().Name == nameof(AnalyticsExport.MLExperimentId) &&
                    item.PrincipalEntityType.ClrType == typeof(MLExperiment))
                    .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
            });
        }

        private async Task<Dataset> CreateDatasetAsync(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = AbpSession.GetTenantId(),
                Name = name,
                SourceFormat = DatasetFormat.Csv,
                Status = DatasetStatus.Ready,
                OwnerUserId = AbpSession.GetUserId(),
                OriginalFileName = name + ".csv"
            }).Entity;

            await context.SaveChangesAsync();
            return dataset;
        }

        private async Task<DatasetVersion> CreateDatasetVersionAsync(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetId,
            int versionNumber,
            DatasetVersionType versionType)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = AbpSession.GetTenantId(),
                DatasetId = datasetId,
                VersionNumber = versionNumber,
                VersionType = versionType,
                Status = DatasetVersionStatus.Active,
                RowCount = 20,
                ColumnCount = 2,
                SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}",
                SizeBytes = 2048
            }).Entity;

            await context.SaveChangesAsync();
            return datasetVersion;
        }

        private async Task<DatasetProfile> CreateDatasetProfileAsync(AstraLab.EntityFrameworkCore.AstraLabDbContext context, long datasetVersionId)
        {
            var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
            {
                TenantId = AbpSession.GetTenantId(),
                DatasetVersionId = datasetVersionId,
                RowCount = 20,
                DuplicateRowCount = 1,
                DataHealthScore = 82.5m,
                SummaryJson = "{\"totalNullCount\":3}"
            }).Entity;

            await context.SaveChangesAsync();
            return datasetProfile;
        }

        private async Task<AIResponse> CreateAiResponseAsync(AstraLab.EntityFrameworkCore.AstraLabDbContext context, Dataset dataset, long datasetVersionId)
        {
            var conversation = context.AIConversations.Add(new AIConversation
            {
                TenantId = dataset.TenantId,
                DatasetId = dataset.Id,
                OwnerUserId = dataset.OwnerUserId,
                LastInteractionTime = new DateTime(2026, 4, 2, 14, 0, 0, DateTimeKind.Utc)
            }).Entity;

            await context.SaveChangesAsync();

            var response = context.AIResponses.Add(new AIResponse
            {
                TenantId = dataset.TenantId,
                AIConversationId = conversation.Id,
                DatasetVersionId = datasetVersionId,
                ResponseContent = "AI explanation",
                ResponseType = AIResponseType.Explanation
            }).Entity;

            await context.SaveChangesAsync();
            return response;
        }

        private async Task<MLExperiment> CreateMlExperimentAsync(AstraLab.EntityFrameworkCore.AstraLabDbContext context, long datasetVersionId)
        {
            var experiment = context.MLExperiments.Add(new MLExperiment
            {
                TenantId = AbpSession.GetTenantId(),
                DatasetVersionId = datasetVersionId,
                Status = MLExperimentStatus.Completed,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = "random_forest_classifier",
                TrainingConfigurationJson = "{\"testSize\":0.2}",
                ExecutedAt = new DateTime(2026, 4, 2, 14, 15, 0, DateTimeKind.Utc)
            }).Entity;

            await context.SaveChangesAsync();
            return experiment;
        }
    }
}
