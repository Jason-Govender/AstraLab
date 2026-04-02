using System;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.AI;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class GenerateAutomaticExperimentInsightJob_Tests : AstraLabTestBase
    {
        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;

        public GenerateAutomaticExperimentInsightJob_Tests()
        {
            _aiDatasetResponseGenerator = Substitute.For<IAiDatasetResponseGenerator>();

            LocalIocManager.IocContainer.Register(
                Component.For<IAiDatasetResponseGenerator>()
                    .Instance(_aiDatasetResponseGenerator)
                    .IsDefault()
                    .LifestyleSingleton());
        }

        [Fact]
        public async Task ExecuteAsync_Should_Generate_When_The_Experiment_Is_Completed_And_No_Duplicate_Exists()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-current-experiment-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                var experiment = CreateCompletedExperiment(context, datasetVersion.Id);

                return new GenerateAutomaticExperimentInsightJobArgs
                {
                    MLExperimentId = experiment.Id,
                    DatasetVersionId = datasetVersion.Id,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            _aiDatasetResponseGenerator.GenerateAutomaticExperimentInsightAsync(
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    Arg.Any<long>())
                .Returns(Task.FromResult(new AstraLab.Services.AI.Dto.GenerateDatasetAiResponseResult()));

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.Received(1).GenerateAutomaticExperimentInsightAsync(
                args.MLExperimentId,
                args.TenantId,
                args.OwnerUserId);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Skip_When_The_Experiment_Is_Not_Completed()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-pending-experiment-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                var experiment = CreateCompletedExperiment(context, datasetVersion.Id, MLExperimentStatus.Pending, includeModel: false);

                return new GenerateAutomaticExperimentInsightJobArgs
                {
                    MLExperimentId = experiment.Id,
                    DatasetVersionId = datasetVersion.Id,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.DidNotReceive().GenerateAutomaticExperimentInsightAsync(
                Arg.Any<long>(),
                Arg.Any<int>(),
                Arg.Any<long>());
        }

        [Fact]
        public async Task ExecuteAsync_Should_Skip_When_Automatic_Insight_For_The_Experiment_Already_Exists()
        {
            var args = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "job-duplicate-experiment-dataset");
                var datasetVersion = CreateDatasetVersion(context, dataset.Id);
                var experiment = CreateCompletedExperiment(context, datasetVersion.Id);
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
                    MLExperimentId = experiment.Id,
                    ResponseContent = "Already generated.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"experimentCompleted\",\"mlExperimentId\":" + experiment.Id + "}"
                });
                context.SaveChanges();

                return new GenerateAutomaticExperimentInsightJobArgs
                {
                    MLExperimentId = experiment.Id,
                    DatasetVersionId = datasetVersion.Id,
                    TenantId = 1,
                    OwnerUserId = AbpSession.UserId.Value
                };
            });

            await ExecuteJobAsync(args);

            await _aiDatasetResponseGenerator.DidNotReceive().GenerateAutomaticExperimentInsightAsync(
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

        private static MLExperiment CreateCompletedExperiment(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetVersionId,
            MLExperimentStatus status = MLExperimentStatus.Completed,
            bool includeModel = true)
        {
            var experiment = context.MLExperiments.Add(new MLExperiment
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                Status = status,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = "random_forest_classifier",
                TrainingConfigurationJson = "{}",
                ExecutedAt = DateTime.UtcNow,
                StartedAtUtc = DateTime.UtcNow.AddMinutes(-3),
                CompletedAtUtc = status == MLExperimentStatus.Completed
                    ? DateTime.UtcNow.AddMinutes(-1)
                    : (DateTime?)null
            }).Entity;

            context.SaveChanges();

            if (includeModel)
            {
                context.MLModels.Add(new MLModel
                {
                    TenantId = 1,
                    MLExperimentId = experiment.Id,
                    ModelType = "random_forest_classifier",
                    PerformanceSummaryJson = "{\"primaryMetric\":\"accuracy\"}"
                });
                context.SaveChanges();
            }

            return experiment;
        }

        private async Task ExecuteJobAsync(GenerateAutomaticExperimentInsightJobArgs args)
        {
            using (var unitOfWork = Resolve<IUnitOfWorkManager>().Begin())
            {
                await Resolve<GenerateAutomaticExperimentInsightJob>().ExecuteAsync(args);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
