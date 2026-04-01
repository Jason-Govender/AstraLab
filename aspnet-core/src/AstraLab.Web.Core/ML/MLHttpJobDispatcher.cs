using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;

namespace AstraLab.Services.ML
{
    /// <summary>
    /// Dispatches ML experiment jobs to the external Python executor over HTTP.
    /// </summary>
    public class MLHttpJobDispatcher : IMLJobDispatcher, ITransientDependency
    {
        /// <summary>
        /// The shared secret header used for internal ML service communication.
        /// </summary>
        public const string SharedSecretHeaderName = "X-AstraLab-ML-Secret";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MLExecutionOptions _mlExecutionOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MLHttpJobDispatcher"/> class.
        /// </summary>
        public MLHttpJobDispatcher(IHttpClientFactory httpClientFactory, MLExecutionOptions mlExecutionOptions)
        {
            _httpClientFactory = httpClientFactory;
            _mlExecutionOptions = mlExecutionOptions;
        }

        /// <summary>
        /// Dispatches the specified experiment payload to the external executor.
        /// </summary>
        public async Task DispatchAsync(DispatchMlExperimentRequest request)
        {
            ValidateRequest(request);

            using (var httpClient = _httpClientFactory.CreateClient())
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildExecutorJobsUrl()))
            {
                httpRequest.Headers.Add(SharedSecretHeaderName, _mlExecutionOptions.SharedSecret);
                httpRequest.Content = new StringContent(
                    JsonSerializer.Serialize(BuildTransportPayload(request), CreateJsonSerializerOptions()),
                    Encoding.UTF8,
                    "application/json");

                var httpResponse = await httpClient.SendAsync(httpRequest);
                if (httpResponse.IsSuccessStatusCode)
                {
                    return;
                }

                var responseBody = await httpResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(responseBody)
                    ? "The ML executor did not accept the experiment job."
                    : responseBody);
            }
        }

        private string BuildExecutorJobsUrl()
        {
            return $"{_mlExecutionOptions.ExecutorBaseUrl.TrimEnd('/')}/jobs";
        }

        private MlExecutorJobRequest BuildTransportPayload(DispatchMlExperimentRequest request)
        {
            return new MlExecutorJobRequest
            {
                ExperimentId = request.ExperimentId,
                TenantId = request.TenantId,
                DatasetVersionId = request.DatasetVersionId,
                DatasetFormat = request.DatasetFormat,
                DatasetStorageProvider = request.DatasetStorageProvider,
                DatasetStorageKey = request.DatasetStorageKey,
                TaskType = request.TaskType,
                AlgorithmKey = request.AlgorithmKey,
                TrainingConfigurationJson = request.TrainingConfigurationJson,
                FeatureColumns = request.FeatureColumns
                    .Select(item => new MlExecutorJobColumn
                    {
                        DatasetColumnId = item.DatasetColumnId,
                        Name = item.Name,
                        DataType = item.DataType,
                        Ordinal = item.Ordinal
                    })
                    .ToList(),
                TargetColumn = request.TargetColumn == null
                    ? null
                    : new MlExecutorJobColumn
                    {
                        DatasetColumnId = request.TargetColumn.DatasetColumnId,
                        Name = request.TargetColumn.Name,
                        DataType = request.TargetColumn.DataType,
                        Ordinal = request.TargetColumn.Ordinal
                    },
                CompletedCallbackUrl = $"{_mlExecutionOptions.CallbackBaseUrl.TrimEnd('/')}/api/services/app/ml-experiments/callbacks/experiment-completed",
                FailedCallbackUrl = $"{_mlExecutionOptions.CallbackBaseUrl.TrimEnd('/')}/api/services/app/ml-experiments/callbacks/experiment-failed"
            };
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private void ValidateRequest(DispatchMlExperimentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(_mlExecutionOptions.ExecutorBaseUrl))
            {
                throw new InvalidOperationException("The ML executor base URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_mlExecutionOptions.CallbackBaseUrl))
            {
                throw new InvalidOperationException("The ML callback base URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_mlExecutionOptions.SharedSecret))
            {
                throw new InvalidOperationException("The ML shared secret is not configured.");
            }
        }

        private class MlExecutorJobRequest
        {
            public long ExperimentId { get; set; }

            public int TenantId { get; set; }

            public long DatasetVersionId { get; set; }

            public string DatasetFormat { get; set; }

            public string DatasetStorageProvider { get; set; }

            public string DatasetStorageKey { get; set; }

            public string TaskType { get; set; }

            public string AlgorithmKey { get; set; }

            public string TrainingConfigurationJson { get; set; }

            public System.Collections.Generic.List<MlExecutorJobColumn> FeatureColumns { get; set; }

            public MlExecutorJobColumn TargetColumn { get; set; }

            public string CompletedCallbackUrl { get; set; }

            public string FailedCallbackUrl { get; set; }
        }

        private class MlExecutorJobColumn
        {
            public long DatasetColumnId { get; set; }

            public string Name { get; set; }

            public string DataType { get; set; }

            public int Ordinal { get; set; }
        }
    }
}
