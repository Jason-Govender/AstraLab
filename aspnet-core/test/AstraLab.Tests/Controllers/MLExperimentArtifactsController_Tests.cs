using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Controllers;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Controllers
{
    public class MLExperimentArtifactsController_Tests : AstraLabTestBase
    {
        private readonly IMLArtifactStorage _mlArtifactStorage;
        private readonly IAbpSession _abpSession;

        public MLExperimentArtifactsController_Tests()
        {
            _mlArtifactStorage = Resolve<IMLArtifactStorage>();
            _abpSession = Resolve<IAbpSession>();
        }

        [Fact]
        public async Task DownloadArtifactAsync_Should_Return_File_For_A_Tenant_Owned_Artifact()
        {
            var experimentId = await SeedExperimentWithArtifactAsync("controller-model-content", ownerUserId: AbpSession.UserId.Value);
            var controller = new MLExperimentArtifactsController(Resolve<IMLArtifactAccessService>(), _abpSession);

            var result = await controller.DownloadArtifactAsync(experimentId);

            var fileResult = result.ShouldBeOfType<FileStreamResult>();
            fileResult.ContentType.ShouldBe("application/octet-stream");
            fileResult.FileDownloadName.ShouldBe($"ml-experiment-{experimentId}-random_forest_classifier.joblib");

            using (var reader = new StreamReader(fileResult.FileStream, Encoding.UTF8, false, 1024, true))
            {
                (await reader.ReadToEndAsync()).ShouldBe("controller-model-content");
            }
        }

        [Fact]
        public async Task DownloadArtifactAsync_Should_Return_NotFound_When_No_Artifact_Exists()
        {
            var experimentId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "ml-no-artifact",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.UserId.Value,
                    OriginalFileName = "ml-no-artifact.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 256
                }).Entity;

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Status = MLExperimentStatus.Completed,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "random_forest_classifier",
                    TrainingConfigurationJson = "{}",
                    ExecutedAt = System.DateTime.UtcNow
                }).Entity;

                context.SaveChanges();
                return experiment.Id;
            });

            var controller = new MLExperimentArtifactsController(Resolve<IMLArtifactAccessService>(), _abpSession);

            var result = await controller.DownloadArtifactAsync(experimentId);

            result.ShouldBeOfType<NotFoundObjectResult>()
                .Value
                .ToString()
                .ShouldContain("does not have a stored artifact");
        }

        [Fact]
        public async Task DownloadArtifactAsync_Should_Return_NotFound_For_A_Foreign_Experiment()
        {
            var experimentId = await SeedExperimentWithArtifactAsync("foreign-model-content", ownerUserId: AbpSession.UserId.Value + 10);
            var controller = new MLExperimentArtifactsController(Resolve<IMLArtifactAccessService>(), _abpSession);

            var result = await controller.DownloadArtifactAsync(experimentId);

            result.ShouldBeOfType<NotFoundObjectResult>()
                .Value
                .ToString()
                .ShouldContain("could not be found");
        }

        private async Task<long> SeedExperimentWithArtifactAsync(string content, long ownerUserId)
        {
            var storedArtifact = await _mlArtifactStorage.StoreAsync(new StoreMlArtifactRequest
            {
                StorageProvider = "local-filesystem",
                StorageKey = $"artifacts/tenant-1/{ownerUserId}/model.joblib",
                Content = new MemoryStream(Encoding.UTF8.GetBytes(content))
            });

            return UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "ml-artifact-download",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = ownerUserId,
                    OriginalFileName = "ml-artifact-download.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 256
                }).Entity;

                context.SaveChanges();

                var experiment = context.MLExperiments.Add(new MLExperiment
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    Status = MLExperimentStatus.Completed,
                    TaskType = MLTaskType.Classification,
                    AlgorithmKey = "random_forest_classifier",
                    TrainingConfigurationJson = "{}",
                    ExecutedAt = System.DateTime.UtcNow
                }).Entity;

                context.SaveChanges();

                context.MLModels.Add(new MLModel
                {
                    TenantId = 1,
                    MLExperimentId = experiment.Id,
                    ModelType = "random_forest_classifier",
                    ArtifactStorageProvider = storedArtifact.StorageProvider,
                    ArtifactStorageKey = storedArtifact.StorageKey,
                    PerformanceSummaryJson = "{\"primaryMetric\":\"accuracy\"}"
                });

                context.SaveChanges();
                return experiment.Id;
            });
        }
    }
}
