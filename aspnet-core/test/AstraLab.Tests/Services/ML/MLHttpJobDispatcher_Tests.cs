using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AstraLab.Services.ML;
using AstraLab.Services.Storage;
using AstraLab.Web.Core.ML;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.ML
{
    public class MLHttpJobDispatcher_Tests
    {
        [Fact]
        public async Task DispatchAsync_Should_Send_Executor_Urls_Instead_Of_Local_Path_References()
        {
            var recordingHandler = new RecordingMessageHandler();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient(recordingHandler));

            var dispatcher = new MLHttpJobDispatcher(
                httpClientFactory,
                new MLExecutionOptions
                {
                    ExecutorBaseUrl = "http://ml-executor:8010",
                    CallbackBaseUrl = "http://abp-host",
                    SharedSecret = "test-secret",
                    DefaultArtifactStorageProvider = "s3-compatible",
                    ArtifactRootPath = "App_Data/MLArtifacts"
                },
                new MLExecutorFileAccessUrlFactory(
                    new MLExecutionOptions
                    {
                        CallbackBaseUrl = "http://abp-host",
                        SharedSecret = "test-secret",
                        DefaultArtifactStorageProvider = "s3-compatible",
                        ArtifactRootPath = "App_Data/MLArtifacts"
                    },
                    new MLExecutorFileAccessTokenService(
                        new MLExecutionOptions
                        {
                            CallbackBaseUrl = "http://abp-host",
                            SharedSecret = "test-secret",
                            DefaultArtifactStorageProvider = "s3-compatible",
                            ArtifactRootPath = "App_Data/MLArtifacts"
                        },
                        new ObjectStorageOptions
                        {
                            PresignedUrlTtlSeconds = 900
                        })));

            await dispatcher.DispatchAsync(new DispatchMlExperimentRequest
            {
                ExperimentId = 42,
                TenantId = 7,
                DatasetVersionId = 11,
                DatasetFormat = "csv",
                DatasetStorageProvider = "local-filesystem",
                DatasetStorageKey = "tenants/7/datasets/5/versions/11/raw/demo.csv",
                TaskType = "classification",
                AlgorithmKey = "logistic_regression",
                TrainingConfigurationJson = "{\"testSize\":0.2}",
                FeatureColumns =
                {
                    new DispatchMlExperimentColumn
                    {
                        DatasetColumnId = 100,
                        Name = "age",
                        DataType = "integer",
                        Ordinal = 1
                    }
                },
                TargetColumn = new DispatchMlExperimentColumn
                {
                    DatasetColumnId = 101,
                    Name = "label",
                    DataType = "boolean",
                    Ordinal = 2
                }
            });

            var requestJson = JsonDocument.Parse(recordingHandler.RecordedRequestContent);
            requestJson.RootElement.GetProperty("datasetDownloadUrl").GetString().ShouldStartWith("http://abp-host/api/internal/ml-storage/datasets?token=");
            requestJson.RootElement.GetProperty("artifactUploadUrl").GetString().ShouldStartWith("http://abp-host/api/internal/ml-storage/artifacts?token=");
            requestJson.RootElement.GetProperty("artifactStorageProvider").GetString().ShouldBe("s3-compatible");
            requestJson.RootElement.GetProperty("artifactStorageKey").GetString().ShouldBe("tenants/7/ml/experiments/42/model.joblib");
            requestJson.RootElement.EnumerateObject().Any(item => item.Name == "datasetStorageProvider").ShouldBeFalse();
            requestJson.RootElement.EnumerateObject().Any(item => item.Name == "datasetStorageKey").ShouldBeFalse();
        }

        private class RecordingMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage RecordedRequest { get; private set; }

            public string RecordedRequestContent { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RecordedRequest = request;
                RecordedRequestContent = request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
