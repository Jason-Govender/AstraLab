using System;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.AI;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class GenerateAutomaticDatasetInsightJob_Tests : AstraLabTestBase
    {
        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;

        public GenerateAutomaticDatasetInsightJob_Tests()
        {
            _aiDatasetResponseGenerator = Substitute.For<IAiDatasetResponseGenerator>();

            LocalIocManager.IocContainer.Register(
                Component.For<IAiDatasetResponseGenerator>()
                    .Instance(_aiDatasetResponseGenerator)
                    .IsDefault()
                    .LifestyleSingleton());
        }

        [Fact]
        public async Task ExecuteAsync_Should_Generate_When_Profile_Is_Current_And_No_Duplicate_Exists()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-current-profile-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                var datasetProfile = CreateDatasetProfile(context, datasetVersion.Id);

                return new GenerateAutomaticDatasetInsightJobArgs
                {
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = datasetProfile.Id,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            _aiDatasetResponseGenerator.GenerateAutomaticInsightAsync(
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    Arg.Any<long>())
                .Returns(Task.FromResult(new AstraLab.Services.AI.Dto.GenerateDatasetAiResponseResult()));

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.Received(1).GenerateAutomaticInsightAsync(
                args.DatasetVersionId,
                args.DatasetProfileId,
                args.TenantId,
                args.OwnerUserId);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Skip_When_Profile_Is_Stale()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-stale-profile-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                CreateDatasetProfile(context, datasetVersion.Id);

                return new GenerateAutomaticDatasetInsightJobArgs
                {
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = 999999,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.DidNotReceive().GenerateAutomaticInsightAsync(
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<int>(),
                Arg.Any<long>());
        }

        [Fact]
        public async Task ExecuteAsync_Should_Skip_When_Automatic_Insight_For_The_Same_Profile_Already_Exists()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-duplicate-profile-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                var datasetProfile = CreateDatasetProfile(context, datasetVersion.Id);
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
                    ResponseContent = "Already generated.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"profilingCompleted\",\"datasetProfileId\":" + datasetProfile.Id + "}"
                });
                context.SaveChanges();

                return new GenerateAutomaticDatasetInsightJobArgs
                {
                    DatasetVersionId = datasetVersion.Id,
                    DatasetProfileId = datasetProfile.Id,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.DidNotReceive().GenerateAutomaticInsightAsync(
                Arg.Any<long>(),
                Arg.Any<long>(),
                Arg.Any<int>(),
                Arg.Any<long>());
        }

        private Dataset CreateDataset(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = 1,
                Name = name,
                Description = name + " description",
                SourceFormat = DatasetFormat.Csv,
                Status = DatasetStatus.Ready,
                OwnerUserId = AbpSession.UserId.Value,
                OriginalFileName = name + ".csv"
            }).Entity;

            context.SaveChanges();
            return dataset;
        }

        private static DatasetVersion CreateDatasetVersion(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetId)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = 1,
                DatasetId = datasetId,
                VersionNumber = 1,
                VersionType = DatasetVersionType.Raw,
                Status = DatasetVersionStatus.Active,
                RowCount = 12,
                ColumnCount = 1,
                SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}",
                SizeBytes = 128
            }).Entity;

            context.SaveChanges();
            return datasetVersion;
        }

        private static DatasetProfile CreateDatasetProfile(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetVersionId)
        {
            var profile = context.DatasetProfiles.Add(new DatasetProfile
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                RowCount = 12,
                DuplicateRowCount = 1,
                DataHealthScore = 81.3m,
                SummaryJson = "{\"totalNullCount\":2}"
            }).Entity;

            context.SaveChanges();
            return profile;
        }

        private async Task ExecuteJobAsync(GenerateAutomaticDatasetInsightJobArgs args)
        {
            using (var unitOfWork = Resolve<IUnitOfWorkManager>().Begin())
            {
                await Resolve<GenerateAutomaticDatasetInsightJob>().ExecuteAsync(args);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
