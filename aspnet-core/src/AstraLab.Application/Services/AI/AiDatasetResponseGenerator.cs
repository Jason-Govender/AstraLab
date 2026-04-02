using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.AI.Dto;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Orchestrates grounded dataset AI generation, conversation replay, and persistence.
    /// </summary>
    public class AiDatasetResponseGenerator : AstraLabAppServiceBase, IAiDatasetResponseGenerator, ITransientDependency
    {
        private static readonly JsonSerializerOptions MetadataSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IAiDatasetContextBuilder _aiDatasetContextBuilder;
        private readonly IAiDatasetInsightReader _aiDatasetInsightReader;
        private readonly IAiPromptBuilder _aiPromptBuilder;
        private readonly IAiConversationHistoryBuilder _aiConversationHistoryBuilder;
        private readonly IAiTextGenerationClient _aiTextGenerationClient;
        private readonly IRepository<AIConversation, long> _aiConversationRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiDatasetResponseGenerator"/> class.
        /// </summary>
        public AiDatasetResponseGenerator(
            IAiDatasetContextBuilder aiDatasetContextBuilder,
            IAiDatasetInsightReader aiDatasetInsightReader,
            IAiPromptBuilder aiPromptBuilder,
            IAiConversationHistoryBuilder aiConversationHistoryBuilder,
            IAiTextGenerationClient aiTextGenerationClient,
            IRepository<AIConversation, long> aiConversationRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _aiDatasetContextBuilder = aiDatasetContextBuilder;
            _aiDatasetInsightReader = aiDatasetInsightReader;
            _aiPromptBuilder = aiPromptBuilder;
            _aiConversationHistoryBuilder = aiConversationHistoryBuilder;
            _aiTextGenerationClient = aiTextGenerationClient;
            _aiConversationRepository = aiConversationRepository;
            _aiResponseRepository = aiResponseRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Generates and persists a dataset-scoped AI response.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateAsync(
            AIResponseType responseType,
            long datasetVersionId,
            int tenantId,
            long ownerUserId,
            string userQuery = null,
            long? conversationId = null)
        {
            ValidateRequest(responseType, userQuery);

            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var datasetContext = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);
            var enrichmentContext = ShouldIncludeEnrichment(responseType)
                ? await _aiDatasetInsightReader.ReadAsync(datasetVersionId, tenantId, ownerUserId)
                : null;

            var conversation = conversationId.HasValue
                ? await GetValidatedConversationAsync(conversationId.Value, datasetVersion, tenantId, ownerUserId)
                : null;
            var priorResponses = conversation == null
                ? new List<AIResponse>()
                : await GetValidatedConversationResponsesAsync(conversation.Id, datasetVersionId);
            var conversationHistory = _aiConversationHistoryBuilder.Build(priorResponses);

            var prompt = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = responseType,
                DatasetContext = datasetContext,
                EnrichmentContext = enrichmentContext,
                UserQuestion = userQuery
            });

            var generatedText = await _aiTextGenerationClient.GenerateTextAsync(new AiTextGenerationRequest
            {
                SystemInstructions = prompt.SystemInstructions,
                ConversationHistory = conversationHistory,
                UserMessage = prompt.UserMessage
            });

            if (string.IsNullOrWhiteSpace(generatedText.Text))
            {
                throw new UserFriendlyException("The AI provider did not return usable response text.");
            }

            var persistedConversation = conversation ?? await _aiConversationRepository.InsertAsync(new AIConversation
            {
                TenantId = tenantId,
                DatasetId = datasetVersion.DatasetId,
                OwnerUserId = ownerUserId,
                LastInteractionTime = DateTime.UtcNow
            });

            var response = await _aiResponseRepository.InsertAsync(new AIResponse
            {
                TenantId = tenantId,
                AIConversation = persistedConversation,
                DatasetVersionId = datasetVersionId,
                UserQuery = string.IsNullOrWhiteSpace(userQuery) ? null : userQuery.Trim(),
                ResponseContent = generatedText.Text.Trim(),
                ResponseType = responseType,
                MetadataJson = BuildMetadataJson(generatedText, datasetContext, enrichmentContext, conversationHistory.Count)
            });

            persistedConversation.LastInteractionTime = DateTime.UtcNow;
            await CurrentUnitOfWork.SaveChangesAsync();

            return new GenerateDatasetAiResponseResult
            {
                ConversationId = persistedConversation.Id,
                Response = ObjectMapper.Map<AIResponseDto>(response)
            };
        }

        /// <summary>
        /// Validates the incoming request shape for the selected task.
        /// </summary>
        private static void ValidateRequest(AIResponseType responseType, string userQuery)
        {
            if (responseType == AIResponseType.QuestionAnswer && string.IsNullOrWhiteSpace(userQuery))
            {
                throw new UserFriendlyException("A question is required for dataset AI Q&A.");
            }
        }

        /// <summary>
        /// Gets the existing conversation after validating tenant, owner, and dataset scope.
        /// </summary>
        private async Task<AIConversation> GetValidatedConversationAsync(
            long conversationId,
            DatasetVersion datasetVersion,
            int tenantId,
            long ownerUserId)
        {
            var conversation = await _aiConversationRepository.GetAll()
                .Where(item =>
                    item.Id == conversationId &&
                    item.TenantId == tenantId &&
                    item.OwnerUserId == ownerUserId &&
                    item.DatasetId == datasetVersion.DatasetId)
                .SingleOrDefaultAsync();

            if (conversation == null)
            {
                throw new UserFriendlyException("The requested AI conversation could not be found for the selected dataset.");
            }

            return conversation;
        }

        /// <summary>
        /// Loads prior responses and prevents version-mismatched conversation reuse.
        /// </summary>
        private async Task<List<AIResponse>> GetValidatedConversationResponsesAsync(long conversationId, long datasetVersionId)
        {
            var allResponses = await _aiResponseRepository.GetAll()
                .Where(item => item.AIConversationId == conversationId)
                .OrderBy(item => item.CreationTime)
                .ThenBy(item => item.Id)
                .ToListAsync();

            if (allResponses.Any(item => item.DatasetVersionId != datasetVersionId))
            {
                throw new UserFriendlyException("The selected AI conversation belongs to a different dataset version and cannot be reused for this Q&A request.");
            }

            return allResponses.Count <= AiDatasetGenerationDefaults.MaxConversationResponses
                ? allResponses
                : allResponses.Skip(allResponses.Count - AiDatasetGenerationDefaults.MaxConversationResponses).ToList();
        }

        /// <summary>
        /// Determines whether extra enrichment should be loaded for the selected task.
        /// </summary>
        private static bool ShouldIncludeEnrichment(AIResponseType responseType)
        {
            return responseType == AIResponseType.Recommendation || responseType == AIResponseType.QuestionAnswer;
        }

        /// <summary>
        /// Builds the compact persisted metadata payload for a successful provider call.
        /// </summary>
        private static string BuildMetadataJson(
            AiTextGenerationResult result,
            AiDatasetContext datasetContext,
            AiDatasetInsightContext enrichmentContext,
            int replayedMessageCount)
        {
            return JsonSerializer.Serialize(new
            {
                provider = result.Provider,
                model = result.Model,
                providerResponseId = result.ProviderResponseId,
                usageJson = result.UsageJson,
                context = new
                {
                    totalColumns = datasetContext.Columns.Count,
                    detailedColumns = datasetContext.DetailedColumnCount,
                    wasColumnContextPruned = datasetContext.IsColumnContextPruned,
                    enrichedHighSignalColumns = enrichmentContext?.HighSignalColumns?.Count ?? 0,
                    enrichedTransformations = enrichmentContext?.RecentTransformations?.Count ?? 0,
                    replayedConversationMessages = replayedMessageCount
                }
            }, MetadataSerializerOptions);
        }
    }
}
