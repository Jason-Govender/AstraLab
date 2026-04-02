using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.Datasets;
using AstraLab.Services.AI.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Exposes dataset-scoped AI generation workflows.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetAiAppService : AstraLabAppServiceBase, IDatasetAiAppService
    {
        private const int RESPONSE_PREVIEW_MAX_LENGTH = 180;

        private readonly IAiDatasetResponseGenerator _aiDatasetResponseGenerator;
        private readonly IRepository<AIConversation, long> _aiConversationRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetAiAppService"/> class.
        /// </summary>
        public DatasetAiAppService(
            IAiDatasetResponseGenerator aiDatasetResponseGenerator,
            IRepository<AIConversation, long> aiConversationRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IRepository<MLExperiment, long> mlExperimentRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _aiDatasetResponseGenerator = aiDatasetResponseGenerator;
            _aiConversationRepository = aiConversationRepository;
            _aiResponseRepository = aiResponseRepository;
            _mlExperimentRepository = mlExperimentRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
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
        /// Generates a concise summary for the selected machine learning experiment.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateExperimentSummaryAsync(EntityDto<long> mlExperimentId)
        {
            var experiment = await GetValidatedExperimentAsync(mlExperimentId.Id, GetRequiredTenantId(), AbpSession.GetUserId());

            return await _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.Summary,
                experiment.DatasetVersionId,
                GetRequiredTenantId(),
                AbpSession.GetUserId(),
                mlExperimentId: experiment.Id);
        }

        /// <summary>
        /// Generates concise next-step recommendations for the selected machine learning experiment.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateExperimentRecommendationsAsync(EntityDto<long> mlExperimentId)
        {
            var experiment = await GetValidatedExperimentAsync(mlExperimentId.Id, GetRequiredTenantId(), AbpSession.GetUserId());

            return await _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.Recommendation,
                experiment.DatasetVersionId,
                GetRequiredTenantId(),
                AbpSession.GetUserId(),
                mlExperimentId: experiment.Id);
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

        /// <summary>
        /// Answers a grounded natural-language question about the selected machine learning experiment.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> AskExperimentAsync(AskExperimentAiQuestionRequest input)
        {
            var experiment = await GetValidatedExperimentAsync(input.MLExperimentId, GetRequiredTenantId(), AbpSession.GetUserId());

            return await _aiDatasetResponseGenerator.GenerateAsync(
                AIResponseType.QuestionAnswer,
                experiment.DatasetVersionId,
                GetRequiredTenantId(),
                AbpSession.GetUserId(),
                input.Question,
                input.ConversationId,
                experiment.Id);
        }

        /// <summary>
        /// Gets a persisted AI conversation summary for the selected conversation.
        /// </summary>
        public async Task<AIConversationDto> GetConversationAsync(EntityDto<long> conversationId)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var conversation = await GetValidatedConversationAsync(conversationId.Id, tenantId, ownerUserId);

            return await BuildConversationDtoAsync(conversation);
        }

        /// <summary>
        /// Gets persisted AI conversations for the selected dataset or dataset version.
        /// </summary>
        public async Task<PagedResultDto<AIConversationDto>> GetConversationsAsync(GetDatasetAiConversationsRequest input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(input.DatasetId, tenantId, ownerUserId);

            if (input.DatasetVersionId.HasValue)
            {
                var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(input.DatasetVersionId.Value, tenantId, ownerUserId);

                if (datasetVersion.DatasetId != input.DatasetId)
                {
                    throw new UserFriendlyException("The selected dataset version does not belong to the requested dataset.");
                }
            }

            if (input.MLExperimentId.HasValue)
            {
                var experiment = await GetValidatedExperimentAsync(input.MLExperimentId.Value, tenantId, ownerUserId);
                var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(experiment.DatasetVersionId, tenantId, ownerUserId);

                if (datasetVersion.DatasetId != input.DatasetId)
                {
                    throw new UserFriendlyException("The selected ML experiment does not belong to the requested dataset.");
                }

                if (input.DatasetVersionId.HasValue && input.DatasetVersionId.Value != experiment.DatasetVersionId)
                {
                    throw new UserFriendlyException("The selected ML experiment does not belong to the requested dataset version.");
                }
            }

            var responseQuery = _aiResponseRepository.GetAll();
            var conversationQuery = _aiConversationRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.OwnerUserId == ownerUserId &&
                    item.DatasetId == input.DatasetId);

            if (input.DatasetVersionId.HasValue)
            {
                var datasetVersionId = input.DatasetVersionId.Value;
                conversationQuery = conversationQuery.Where(item =>
                    responseQuery.Any(response =>
                        response.AIConversationId == item.Id &&
                        response.DatasetVersionId == datasetVersionId));
            }

            if (input.MLExperimentId.HasValue)
            {
                var mlExperimentId = input.MLExperimentId.Value;
                conversationQuery = conversationQuery.Where(item =>
                    responseQuery.Any(response =>
                        response.AIConversationId == item.Id &&
                        response.MLExperimentId == mlExperimentId));
            }

            var totalCount = await conversationQuery.CountAsync();
            var pagedConversations = await conversationQuery
                .OrderByDescending(item => item.LastInteractionTime)
                .ThenByDescending(item => item.Id)
                .PageBy(input)
                .ToListAsync();
            var conversationIds = pagedConversations.Select(item => item.Id).ToList();
            var responseLookup = await GetConversationResponseLookupAsync(conversationIds);

            return new PagedResultDto<AIConversationDto>(
                totalCount,
                pagedConversations
                    .Select(item =>
                    {
                        responseLookup.TryGetValue(item.Id, out var responses);
                        return BuildConversationDto(item, responses);
                    })
                    .ToList());
        }

        /// <summary>
        /// Gets persisted AI responses for the selected conversation thread.
        /// </summary>
        public async Task<PagedResultDto<AIResponseDto>> GetResponsesAsync(GetDatasetAiResponsesRequest input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            await GetValidatedConversationAsync(input.ConversationId, tenantId, ownerUserId);

            var query = _aiResponseRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.AIConversationId == input.ConversationId);
            var totalCount = await query.CountAsync();

            query = input.IsChronological
                ? query.OrderBy(item => item.CreationTime).ThenBy(item => item.Id)
                : query.OrderByDescending(item => item.CreationTime).ThenByDescending(item => item.Id);

            var responses = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<AIResponseDto>(
                totalCount,
                ObjectMapper.Map<List<AIResponseDto>>(responses));
        }

        /// <summary>
        /// Gets the latest profiling-triggered automatic insight for the selected dataset version when one exists.
        /// </summary>
        public async Task<AIResponseDto> GetLatestAutomaticInsightAsync(EntityDto<long> datasetVersionId)
        {
            var tenantId = GetRequiredTenantId();
            await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId.Id, tenantId, AbpSession.GetUserId());

            var latestAutomaticInsight = await _aiResponseRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.DatasetVersionId == datasetVersionId.Id &&
                    item.ResponseType == AIResponseType.Insight)
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            var response = latestAutomaticInsight
                .FirstOrDefault(item => AiAutomaticInsightMetadata.IsAutomaticProfilingInsight(item.MetadataJson));

            return response == null
                ? null
                : ObjectMapper.Map<AIResponseDto>(response);
        }

        /// <summary>
        /// Gets the latest experiment-completed automatic insight for the selected machine learning experiment when one exists.
        /// </summary>
        public async Task<AIResponseDto> GetLatestAutomaticExperimentInsightAsync(EntityDto<long> mlExperimentId)
        {
            var tenantId = GetRequiredTenantId();
            await GetValidatedExperimentAsync(mlExperimentId.Id, tenantId, AbpSession.GetUserId());

            var latestAutomaticInsight = await _aiResponseRepository.GetAll()
                .Where(item =>
                    item.TenantId == tenantId &&
                    item.MLExperimentId == mlExperimentId.Id &&
                    item.ResponseType == AIResponseType.Insight)
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            var response = latestAutomaticInsight
                .FirstOrDefault(item => AiAutomaticInsightMetadata.IsAutomaticExperimentInsight(item.MetadataJson));

            return response == null
                ? null
                : ObjectMapper.Map<AIResponseDto>(response);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset AI operations.");
            }

            return AbpSession.TenantId.Value;
        }

        /// <summary>
        /// Gets a tenant-owned conversation and validates dataset ownership before returning it.
        /// </summary>
        private async Task<AIConversation> GetValidatedConversationAsync(long conversationId, int tenantId, long ownerUserId)
        {
            var conversation = await _aiConversationRepository.GetAll()
                .Where(item =>
                    item.Id == conversationId &&
                    item.TenantId == tenantId &&
                    item.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (conversation == null)
            {
                throw new UserFriendlyException("The requested AI conversation could not be found.");
            }

            await _datasetOwnershipAccessChecker.GetDatasetForOwnerAsync(conversation.DatasetId, tenantId, ownerUserId);
            return conversation;
        }

        /// <summary>
        /// Gets a tenant-owned machine learning experiment scoped to the current dataset owner.
        /// </summary>
        private async Task<MLExperiment> GetValidatedExperimentAsync(long mlExperimentId, int tenantId, long ownerUserId)
        {
            var experiment = await _mlExperimentRepository.GetAll()
                .Where(item =>
                    item.Id == mlExperimentId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (experiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            return experiment;
        }

        /// <summary>
        /// Builds a persisted AI conversation DTO including latest-response summary details.
        /// </summary>
        private async Task<AIConversationDto> BuildConversationDtoAsync(AIConversation conversation)
        {
            var responseLookup = await GetConversationResponseLookupAsync(new List<long> { conversation.Id });

            responseLookup.TryGetValue(conversation.Id, out var responses);
            return BuildConversationDto(conversation, responses);
        }

        /// <summary>
        /// Loads persisted responses for the selected conversations so summary DTOs can be built efficiently.
        /// </summary>
        private async Task<Dictionary<long, List<AIResponse>>> GetConversationResponseLookupAsync(List<long> conversationIds)
        {
            if (conversationIds.Count == 0)
            {
                return new Dictionary<long, List<AIResponse>>();
            }

            var responses = await _aiResponseRepository.GetAll()
                .Where(item => conversationIds.Contains(item.AIConversationId))
                .OrderBy(item => item.CreationTime)
                .ThenBy(item => item.Id)
                .ToListAsync();

            return responses
                .GroupBy(item => item.AIConversationId)
                .ToDictionary(item => item.Key, item => item.ToList());
        }

        /// <summary>
        /// Maps a conversation and its persisted responses to a UI-friendly conversation summary DTO.
        /// </summary>
        private static AIConversationDto BuildConversationDto(AIConversation conversation, List<AIResponse> responses)
        {
            var latestResponse = responses?
                .OrderByDescending(item => item.CreationTime)
                .ThenByDescending(item => item.Id)
                .FirstOrDefault();

            return new AIConversationDto
            {
                Id = conversation.Id,
                DatasetId = conversation.DatasetId,
                OwnerUserId = conversation.OwnerUserId,
                LastInteractionTime = conversation.LastInteractionTime,
                CreationTime = conversation.CreationTime,
                ResponseCount = responses?.Count ?? 0,
                LatestDatasetVersionId = latestResponse?.DatasetVersionId,
                LatestResponseType = latestResponse?.ResponseType,
                LatestMLExperimentId = latestResponse?.MLExperimentId,
                LatestUserQuery = latestResponse?.UserQuery,
                LatestResponsePreview = BuildResponsePreview(latestResponse?.ResponseContent)
            };
        }

        /// <summary>
        /// Trims persisted AI response content to a compact preview suitable for list rendering.
        /// </summary>
        private static string BuildResponsePreview(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return null;
            }

            var trimmedContent = responseContent.Trim();
            return trimmedContent.Length <= RESPONSE_PREVIEW_MAX_LENGTH
                ? trimmedContent
                : trimmedContent.Substring(0, RESPONSE_PREVIEW_MAX_LENGTH - 1) + "…";
        }
    }
}
