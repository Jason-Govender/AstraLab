using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.AI.Dto;
using AstraLab.Services.Datasets;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Orchestrates grounded dataset and experiment AI generation, conversation replay, and persistence.
    /// </summary>
    public class AiDatasetResponseGenerator : AstraLabAppServiceBase, IAiDatasetResponseGenerator, ITransientDependency
    {
        private static readonly JsonSerializerOptions MetadataSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IAiDatasetContextBuilder _aiDatasetContextBuilder;
        private readonly IAiMlExperimentContextBuilder _aiMlExperimentContextBuilder;
        private readonly IAiDatasetInsightReader _aiDatasetInsightReader;
        private readonly IAiPromptBuilder _aiPromptBuilder;
        private readonly IAiConversationHistoryBuilder _aiConversationHistoryBuilder;
        private readonly IAiTextGenerationClient _aiTextGenerationClient;
        private readonly IRepository<AIConversation, long> _aiConversationRepository;
        private readonly IRepository<AIResponse, long> _aiResponseRepository;
        private readonly IRepository<MLExperiment, long> _mlExperimentRepository;
        private readonly IDatasetOwnershipAccessChecker _datasetOwnershipAccessChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiDatasetResponseGenerator"/> class.
        /// </summary>
        public AiDatasetResponseGenerator(
            IAiDatasetContextBuilder aiDatasetContextBuilder,
            IAiMlExperimentContextBuilder aiMlExperimentContextBuilder,
            IAiDatasetInsightReader aiDatasetInsightReader,
            IAiPromptBuilder aiPromptBuilder,
            IAiConversationHistoryBuilder aiConversationHistoryBuilder,
            IAiTextGenerationClient aiTextGenerationClient,
            IRepository<AIConversation, long> aiConversationRepository,
            IRepository<AIResponse, long> aiResponseRepository,
            IRepository<MLExperiment, long> mlExperimentRepository,
            IDatasetOwnershipAccessChecker datasetOwnershipAccessChecker)
        {
            _aiDatasetContextBuilder = aiDatasetContextBuilder;
            _aiMlExperimentContextBuilder = aiMlExperimentContextBuilder;
            _aiDatasetInsightReader = aiDatasetInsightReader;
            _aiPromptBuilder = aiPromptBuilder;
            _aiConversationHistoryBuilder = aiConversationHistoryBuilder;
            _aiTextGenerationClient = aiTextGenerationClient;
            _aiConversationRepository = aiConversationRepository;
            _aiResponseRepository = aiResponseRepository;
            _mlExperimentRepository = mlExperimentRepository;
            _datasetOwnershipAccessChecker = datasetOwnershipAccessChecker;
        }

        /// <summary>
        /// Generates and persists a dataset-scoped or experiment-scoped AI response.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateAsync(
            AIResponseType responseType,
            long datasetVersionId,
            int tenantId,
            long ownerUserId,
            string userQuery = null,
            long? conversationId = null,
            long? mlExperimentId = null)
        {
            ValidateRequest(responseType, userQuery);

            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var mlExperiment = mlExperimentId.HasValue
                ? await GetValidatedExperimentAsync(mlExperimentId.Value, datasetVersionId, tenantId, ownerUserId)
                : null;
            var datasetContext = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);
            var enrichmentContext = ShouldIncludeEnrichment(responseType)
                ? await _aiDatasetInsightReader.ReadAsync(datasetVersionId, tenantId, ownerUserId)
                : null;
            var mlExperimentContext = mlExperimentId.HasValue
                ? await _aiMlExperimentContextBuilder.BuildAsync(mlExperimentId.Value, tenantId, ownerUserId)
                : null;

            var conversation = conversationId.HasValue
                ? await GetValidatedConversationAsync(conversationId.Value, datasetVersion, tenantId, ownerUserId)
                : null;
            var priorResponses = conversation == null
                ? new List<AIResponse>()
                : await GetValidatedConversationResponsesAsync(conversation.Id, datasetVersionId, mlExperimentId);
            var conversationHistory = _aiConversationHistoryBuilder.Build(priorResponses);

            var prompt = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = responseType,
                DatasetContext = datasetContext,
                EnrichmentContext = enrichmentContext,
                MlExperimentContext = mlExperimentContext,
                UserQuestion = userQuery,
                IsAutomaticProfilingInsight = false,
                IsAutomaticExperimentInsight = false
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
                MLExperimentId = mlExperiment?.Id,
                MetadataJson = BuildMetadataJson(
                    generatedText,
                    datasetContext,
                    enrichmentContext,
                    mlExperimentContext,
                    conversationHistory.Count,
                    null,
                    null,
                    mlExperiment?.Id)
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
        /// Generates and persists a profiling-triggered automatic insight for the selected dataset version.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateAutomaticInsightAsync(
            long datasetVersionId,
            long datasetProfileId,
            int tenantId,
            long ownerUserId)
        {
            var datasetVersion = await _datasetOwnershipAccessChecker.GetDatasetVersionForOwnerAsync(datasetVersionId, tenantId, ownerUserId);
            var datasetContext = await _aiDatasetContextBuilder.BuildAsync(datasetVersionId, tenantId, ownerUserId);
            var enrichmentContext = await _aiDatasetInsightReader.ReadAsync(datasetVersionId, tenantId, ownerUserId);
            var prompt = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Insight,
                DatasetContext = datasetContext,
                EnrichmentContext = enrichmentContext,
                IsAutomaticProfilingInsight = true
            });

            var generatedText = await _aiTextGenerationClient.GenerateTextAsync(new AiTextGenerationRequest
            {
                SystemInstructions = prompt.SystemInstructions,
                ConversationHistory = new List<AiConversationHistoryMessage>(),
                UserMessage = prompt.UserMessage
            });

            if (string.IsNullOrWhiteSpace(generatedText.Text))
            {
                throw new UserFriendlyException("The AI provider did not return usable response text.");
            }

            var conversation = await _aiConversationRepository.InsertAsync(new AIConversation
            {
                TenantId = tenantId,
                DatasetId = datasetVersion.DatasetId,
                OwnerUserId = ownerUserId,
                LastInteractionTime = DateTime.UtcNow
            });

            var response = await _aiResponseRepository.InsertAsync(new AIResponse
            {
                TenantId = tenantId,
                AIConversation = conversation,
                DatasetVersionId = datasetVersionId,
                UserQuery = null,
                ResponseContent = generatedText.Text.Trim(),
                ResponseType = AIResponseType.Insight,
                MetadataJson = BuildMetadataJson(
                    generatedText,
                    datasetContext,
                    enrichmentContext,
                    null,
                    0,
                    AiAutomaticInsightMetadata.ProfilingCompletedGenerationTrigger,
                    datasetProfileId,
                    null)
            });

            conversation.LastInteractionTime = DateTime.UtcNow;
            await CurrentUnitOfWork.SaveChangesAsync();

            return new GenerateDatasetAiResponseResult
            {
                ConversationId = conversation.Id,
                Response = ObjectMapper.Map<AIResponseDto>(response)
            };
        }

        /// <summary>
        /// Generates and persists an experiment-completed automatic insight for the selected machine learning experiment.
        /// </summary>
        public async Task<GenerateDatasetAiResponseResult> GenerateAutomaticExperimentInsightAsync(
            long mlExperimentId,
            int tenantId,
            long ownerUserId)
        {
            var mlExperiment = await GetValidatedExperimentAsync(mlExperimentId, null, tenantId, ownerUserId);
            var datasetContext = await _aiDatasetContextBuilder.BuildAsync(mlExperiment.DatasetVersionId, tenantId, ownerUserId);
            var enrichmentContext = await _aiDatasetInsightReader.ReadAsync(mlExperiment.DatasetVersionId, tenantId, ownerUserId);
            var mlExperimentContext = await _aiMlExperimentContextBuilder.BuildAsync(mlExperimentId, tenantId, ownerUserId);
            var prompt = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Insight,
                DatasetContext = datasetContext,
                EnrichmentContext = enrichmentContext,
                MlExperimentContext = mlExperimentContext,
                IsAutomaticExperimentInsight = true
            });

            var generatedText = await _aiTextGenerationClient.GenerateTextAsync(new AiTextGenerationRequest
            {
                SystemInstructions = prompt.SystemInstructions,
                ConversationHistory = new List<AiConversationHistoryMessage>(),
                UserMessage = prompt.UserMessage
            });

            if (string.IsNullOrWhiteSpace(generatedText.Text))
            {
                throw new UserFriendlyException("The AI provider did not return usable response text.");
            }

            var conversation = await _aiConversationRepository.InsertAsync(new AIConversation
            {
                TenantId = tenantId,
                DatasetId = mlExperiment.DatasetVersion.DatasetId,
                OwnerUserId = ownerUserId,
                LastInteractionTime = DateTime.UtcNow
            });

            var response = await _aiResponseRepository.InsertAsync(new AIResponse
            {
                TenantId = tenantId,
                AIConversation = conversation,
                DatasetVersionId = mlExperiment.DatasetVersionId,
                UserQuery = null,
                ResponseContent = generatedText.Text.Trim(),
                ResponseType = AIResponseType.Insight,
                MLExperimentId = mlExperimentId,
                MetadataJson = BuildMetadataJson(
                    generatedText,
                    datasetContext,
                    enrichmentContext,
                    mlExperimentContext,
                    0,
                    AiAutomaticInsightMetadata.ExperimentCompletedGenerationTrigger,
                    null,
                    mlExperimentId)
            });

            conversation.LastInteractionTime = DateTime.UtcNow;
            await CurrentUnitOfWork.SaveChangesAsync();

            return new GenerateDatasetAiResponseResult
            {
                ConversationId = conversation.Id,
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
        /// Loads prior responses and prevents version-mismatched or experiment-mismatched conversation reuse.
        /// </summary>
        private async Task<List<AIResponse>> GetValidatedConversationResponsesAsync(
            long conversationId,
            long datasetVersionId,
            long? mlExperimentId)
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

            if (allResponses.Any(item => item.MLExperimentId != mlExperimentId))
            {
                throw new UserFriendlyException(mlExperimentId.HasValue
                    ? "The selected AI conversation belongs to a different machine learning experiment and cannot be reused for this request."
                    : "The selected AI conversation belongs to a machine learning experiment and cannot be reused for a dataset-only request.");
            }

            return allResponses.Count <= AiDatasetGenerationDefaults.MaxConversationResponses
                ? allResponses
                : allResponses.Skip(allResponses.Count - AiDatasetGenerationDefaults.MaxConversationResponses).ToList();
        }

        /// <summary>
        /// Gets a tenant-owned machine learning experiment and validates dataset-version scope when supplied.
        /// </summary>
        private async Task<MLExperiment> GetValidatedExperimentAsync(
            long mlExperimentId,
            long? expectedDatasetVersionId,
            int tenantId,
            long ownerUserId)
        {
            var mlExperiment = await _mlExperimentRepository.GetAll()
                .Include(item => item.DatasetVersion)
                    .ThenInclude(item => item.Dataset)
                .Where(item =>
                    item.Id == mlExperimentId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (mlExperiment == null)
            {
                throw new UserFriendlyException("The requested ML experiment could not be found.");
            }

            if (expectedDatasetVersionId.HasValue && mlExperiment.DatasetVersionId != expectedDatasetVersionId.Value)
            {
                throw new UserFriendlyException("The selected ML experiment does not belong to the requested dataset version.");
            }

            return mlExperiment;
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
            AiMlExperimentContext mlExperimentContext,
            int replayedMessageCount,
            string generationTrigger,
            long? datasetProfileId,
            long? mlExperimentId)
        {
            return JsonSerializer.Serialize(new
            {
                generationTrigger,
                datasetProfileId,
                mlExperimentId,
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
                    replayedConversationMessages = replayedMessageCount,
                    experimentMetrics = mlExperimentContext?.Metrics?.Count ?? 0,
                    experimentFeatureImportances = mlExperimentContext?.FeatureImportances?.Count ?? 0,
                    experimentWarnings = mlExperimentContext?.Warnings?.Count ?? 0
                }
            }, MetadataSerializerOptions);
        }
    }
}
