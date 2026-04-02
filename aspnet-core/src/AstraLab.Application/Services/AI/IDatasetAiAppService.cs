using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.AI.Dto;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Exposes dataset-scoped AI generation workflows for summaries, insights, recommendations, and Q&amp;A.
    /// </summary>
    public interface IDatasetAiAppService : IApplicationService
    {
        /// <summary>
        /// Generates a concise summary for the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateSummaryAsync(EntityDto<long> datasetVersionId);

        /// <summary>
        /// Generates concise data-quality insights for the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateInsightsAsync(EntityDto<long> datasetVersionId);

        /// <summary>
        /// Generates cleaning and transformation recommendations for the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateCleaningRecommendationsAsync(EntityDto<long> datasetVersionId);

        /// <summary>
        /// Generates a concise summary for the selected machine learning experiment.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateExperimentSummaryAsync(EntityDto<long> mlExperimentId);

        /// <summary>
        /// Generates concise next-step recommendations for the selected machine learning experiment.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> GenerateExperimentRecommendationsAsync(EntityDto<long> mlExperimentId);

        /// <summary>
        /// Answers a grounded natural-language question about the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> AskAsync(AskDatasetAiQuestionRequest input);

        /// <summary>
        /// Answers a grounded natural-language question about the selected machine learning experiment.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> AskExperimentAsync(AskExperimentAiQuestionRequest input);

        /// <summary>
        /// Gets a persisted AI conversation summary for the selected conversation.
        /// </summary>
        Task<AIConversationDto> GetConversationAsync(EntityDto<long> conversationId);

        /// <summary>
        /// Gets persisted AI conversations for the selected dataset or dataset version.
        /// </summary>
        Task<PagedResultDto<AIConversationDto>> GetConversationsAsync(GetDatasetAiConversationsRequest input);

        /// <summary>
        /// Gets persisted AI responses for the selected conversation thread.
        /// </summary>
        Task<PagedResultDto<AIResponseDto>> GetResponsesAsync(GetDatasetAiResponsesRequest input);

        /// <summary>
        /// Gets the latest persisted profiling-triggered automatic insight for the selected dataset version.
        /// </summary>
        Task<AIResponseDto> GetLatestAutomaticInsightAsync(EntityDto<long> datasetVersionId);

        /// <summary>
        /// Gets the latest persisted experiment-completed automatic insight for the selected machine learning experiment.
        /// </summary>
        Task<AIResponseDto> GetLatestAutomaticExperimentInsightAsync(EntityDto<long> mlExperimentId);
    }
}
