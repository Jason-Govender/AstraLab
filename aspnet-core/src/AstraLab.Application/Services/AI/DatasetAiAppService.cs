using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.AI;
using AstraLab.Services.AI.Dto;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Exposes dataset-scoped AI generation workflows.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetAiAppService : AstraLabAppServiceBase, IDatasetAiAppService
    {
        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetAiAppService"/> class.
        /// </summary>
        public DatasetAiAppService(IAiDatasetResponseGenerator aiDatasetResponseGenerator)
        {
            _aiDatasetResponseGenerator = aiDatasetResponseGenerator;
        }

        /// <summary>
        /// Generates a concise summary for the selected dataset version.
        /// </summary>
        public Task<GenerateDatasetAiResponseResult> GenerateSummaryAsync(EntityDto<long> datasetVersionId)
        {
            return _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.Summary,
                datasetVersionId.Id,
                GetRequiredTenantId(),
                AbpSession.GetUserId());
        }

        /// <summary>
        /// Generates concise data-quality insights for the selected dataset version.
        /// </summary>
        public Task<GenerateDatasetAiResponseResult> GenerateInsightsAsync(EntityDto<long> datasetVersionId)
        {
            return _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.Insight,
                datasetVersionId.Id,
                GetRequiredTenantId(),
                AbpSession.GetUserId());
        }

        /// <summary>
        /// Generates cleaning and transformation recommendations for the selected dataset version.
        /// </summary>
        public Task<GenerateDatasetAiResponseResult> GenerateCleaningRecommendationsAsync(EntityDto<long> datasetVersionId)
        {
            return _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.Recommendation,
                datasetVersionId.Id,
                GetRequiredTenantId(),
                AbpSession.GetUserId());
        }

        /// <summary>
        /// Answers a grounded natural-language question about the selected dataset version.
        /// </summary>
        public Task<GenerateDatasetAiResponseResult> AskAsync(AskDatasetAiQuestionRequest input)
        {
            return _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.QuestionAnswer,
                input.DatasetVersionId,
                GetRequiredTenantId(),
                AbpSession.GetUserId(),
                input.Question,
                input.ConversationId);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset AI operations.");
            }

            return AbpSession.TenantId.Value;
        }
    }
}
