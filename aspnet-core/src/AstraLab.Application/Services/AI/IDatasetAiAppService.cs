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
        /// Answers a grounded natural-language question about the selected dataset version.
        /// </summary>
        Task<GenerateDatasetAiResponseResult> AskAsync(AskDatasetAiQuestionRequest input);
    }
}
