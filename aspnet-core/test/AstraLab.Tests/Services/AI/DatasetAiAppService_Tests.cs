using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Core.Domains.ML;
using AstraLab.Services.AI;
using AstraLab.Services.AI.Dto;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class DatasetAiAppService_Tests : AstraLabTestBase
    {
        private readonly IAiTextGenerationClient _aiTextGenerationClient;
        private readonly IDatasetAiAppService _datasetAiAppService;

        public DatasetAiAppService_Tests()
        {
            _aiTextGenerationClient = Substitute.For<IAiTextGenerationClient>();

            LocalIocManager.IocContainer.Register(
                Component.For<IAiTextGenerationClient>()
                    .Instance(_aiTextGenerationClient)
                    .IsDefault()
                    .LifestyleSingleton());

            _datasetAiAppService = Resolve<IDatasetAiAppService>();
        }

        [Fact]
        public async Task GenerateSummaryAsync_Should_Create_A_Persisted_Summary_Response_And_Conversation()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-summary-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                AddProfileData(context, datasetVersion.Id, "amount");
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "This dataset is small and mostly clean.",
                    Provider = "groq",
                    Model = "llama-test",
                    ProviderResponseId = "resp_summary",
                    UsageJson = "{\"output_tokens\":12}"
                }));

            var result = await _datasetAiAppService.GenerateSummaryAsync(new EntityDto<long>(datasetVersionId));

            result.ConversationId.ShouldBeGreaterThan(0L);
            result.Response.ResponseType.ShouldBe(AIResponseType.Summary);
            result.Response.UserQuery.ShouldBeNull();
            result.Response.ResponseContent.ShouldBe("This dataset is small and mostly clean.");
            result.Response.MetadataJson.ShouldContain("\"provider\":\"groq\"");
            result.Response.MetadataJson.ShouldContain("\"totalColumns\":1");

            await UsingDbContextAsync(async context =>
            {
                context.AIConversations.Count().ShouldBe(1);
                context.AIResponses.Count().ShouldBe(1);
                context.AIResponses.Single().AIConversationId.ShouldBe(result.ConversationId);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GenerateCleaningRecommendationsAsync_Should_Include_Enrichment_And_Persist_A_Recommendation()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-recommendation-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                AddProfileData(context, datasetVersion.Id, "amount");
                context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = 1,
                    SourceDatasetVersionId = datasetVersion.Id,
                    ResultDatasetVersionId = null,
                    TransformationType = DatasetTransformationType.RemoveDuplicates,
                    ConfigurationJson = "{\"columns\":[]}",
                    ExecutionOrder = 1,
                    ExecutedAt = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
                    SummaryJson = "{\"removedRows\":4}"
                });
                context.SaveChanges();
                return datasetVersion.Id;
            });

            AiTextGenerationRequest capturedRequest = null;
            _aiTextGenerationClient.GenerateTextAsync(Arg.Do<AiTextGenerationRequest>(item => capturedRequest = item))
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Prioritize missing-value cleanup on amount and then remove duplicates.",
                    Provider = "groq",
                    Model = "llama-test",
                    ProviderResponseId = "resp_recommendation"
                }));

            var result = await _datasetAiAppService.GenerateCleaningRecommendationsAsync(new EntityDto<long>(datasetVersionId));

            result.Response.ResponseType.ShouldBe(AIResponseType.Recommendation);
            capturedRequest.ShouldNotBeNull();
            capturedRequest.UserMessage.ShouldContain("Additional enrichment JSON:");
            capturedRequest.UserMessage.ShouldContain("amount");
            capturedRequest.UserMessage.ShouldContain("removeDuplicates");
        }

        [Fact]
        public async Task AskAsync_Should_Replay_Prior_Turns_And_Persist_A_Question_Answer_Response()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-qa-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                AddProfileData(context, datasetVersion.Id, "sales");

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    UserQuery = "What stands out first?",
                    ResponseContent = "Nulls in sales are the biggest issue.",
                    ResponseType = AIResponseType.QuestionAnswer
                });
                context.SaveChanges();

                return new
                {
                    datasetVersion.Id,
                    ConversationId = conversation.Id
                };
            });

            AiTextGenerationRequest capturedRequest = null;
            _aiTextGenerationClient.GenerateTextAsync(Arg.Do<AiTextGenerationRequest>(item => capturedRequest = item))
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "Sales has moderate null exposure but otherwise looks stable.",
                    Provider = "groq",
                    Model = "llama-test",
                    ProviderResponseId = "resp_qa"
                }));

            var result = await _datasetAiAppService.AskAsync(new AskDatasetAiQuestionRequest
            {
                DatasetVersionId = scenario.Id,
                ConversationId = scenario.ConversationId,
                Question = "How healthy is this dataset?"
            });

            result.ConversationId.ShouldBe(scenario.ConversationId);
            result.Response.ResponseType.ShouldBe(AIResponseType.QuestionAnswer);
            result.Response.UserQuery.ShouldBe("How healthy is this dataset?");
            capturedRequest.ConversationHistory.Count.ShouldBe(2);
            capturedRequest.ConversationHistory[0].Role.ShouldBe("user");
            capturedRequest.ConversationHistory[1].Role.ShouldBe("assistant");
        }

        [Fact]
        public async Task AskAsync_Should_Reject_An_Empty_Question()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-empty-question-dataset", AbpSession.GetUserId());
                return CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw).Id;
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAiAppService.AskAsync(new AskDatasetAiQuestionRequest
                {
                    DatasetVersionId = datasetVersionId,
                    Question = "   "
                }));

            exception.Message.ShouldBe("A question is required for dataset AI Q&A.");
        }

        [Fact]
        public async Task AskAsync_Should_Reject_Conversation_Reuse_Across_Different_Dataset_Versions()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-version-mismatch-dataset", AbpSession.GetUserId());
                var rawVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var processedVersion = CreateDatasetVersion(context, dataset.Id, 2, DatasetVersionType.Processed, rawVersion.Id);
                AddProfileData(context, rawVersion.Id, "amount");
                AddProfileData(context, processedVersion.Id, "amount");

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = rawVersion.Id,
                    UserQuery = "Old question",
                    ResponseContent = "Old answer",
                    ResponseType = AIResponseType.QuestionAnswer
                });
                context.SaveChanges();

                return new
                {
                    RawVersionId = rawVersion.Id,
                    ProcessedVersionId = processedVersion.Id,
                    ConversationId = conversation.Id
                };
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAiAppService.AskAsync(new AskDatasetAiQuestionRequest
                {
                    DatasetVersionId = scenario.ProcessedVersionId,
                    ConversationId = scenario.ConversationId,
                    Question = "Can I continue this thread?"
                }));

            exception.Message.ShouldBe("The selected AI conversation belongs to a different dataset version and cannot be reused for this Q&A request.");
        }

        [Fact]
        public async Task GenerateExperimentSummaryAsync_Should_Create_A_Persisted_Experiment_Linked_Response()
        {
            var experimentId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-experiment-summary-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var seeded = AddProfileData(context, datasetVersion.Id, "amount");
                return CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId).Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns(Task.FromResult(new AiTextGenerationResult
                {
                    Text = "This experiment looks promising but needs stronger validation.",
                    Provider = "groq",
                    Model = "llama-test",
                    ProviderResponseId = "resp_ml_summary"
                }));

            var result = await _datasetAiAppService.GenerateExperimentSummaryAsync(new EntityDto<long>(experimentId));

            result.Response.ResponseType.ShouldBe(AIResponseType.Summary);
            result.Response.MLExperimentId.ShouldBe(experimentId);
            result.Response.MetadataJson.ShouldContain("\"mlExperimentId\":" + experimentId);
        }

        [Fact]
        public async Task AskExperimentAsync_Should_Reject_Conversation_Reuse_Across_Different_Experiments()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-experiment-mismatch-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var seeded = AddProfileData(context, datasetVersion.Id, "amount");
                var firstExperiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId);
                var secondExperiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId, "linear_regression");

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 13, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = firstExperiment.Id,
                    UserQuery = "How does the first run look?",
                    ResponseContent = "It looks stable.",
                    ResponseType = AIResponseType.QuestionAnswer
                });
                context.SaveChanges();

                return new
                {
                    secondExperiment.Id,
                    ConversationId = conversation.Id
                };
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAiAppService.AskExperimentAsync(new AskExperimentAiQuestionRequest
                {
                    MLExperimentId = scenario.Id,
                    ConversationId = scenario.ConversationId,
                    Question = "Can I continue this thread on another run?"
                }));

            exception.Message.ShouldBe("The selected AI conversation belongs to a different machine learning experiment and cannot be reused for this request.");
        }

        [Fact]
        public async Task GenerateSummaryAsync_Should_Reject_A_Dataset_Version_From_A_Different_Owner()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-owner-dataset", AbpSession.GetUserId() + 10);
                return CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw).Id;
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetAiAppService.GenerateSummaryAsync(new EntityDto<long>(datasetVersionId)));
        }

        [Fact]
        public async Task GenerateInsightsAsync_Should_Not_Persist_Partial_Records_When_The_Provider_Fails()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-failing-provider-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                AddProfileData(context, datasetVersion.Id, "amount");
                return datasetVersion.Id;
            });

            _aiTextGenerationClient.GenerateTextAsync(Arg.Any<AiTextGenerationRequest>())
                .Returns<Task<AiTextGenerationResult>>(_ => throw new UserFriendlyException("Provider down."));

            await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAiAppService.GenerateInsightsAsync(new EntityDto<long>(datasetVersionId)));

            await UsingDbContextAsync(async context =>
            {
                context.AIConversations.Count().ShouldBe(0);
                context.AIResponses.Count().ShouldBe(0);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GetLatestAutomaticInsightAsync_Should_Return_The_Newest_Profiling_Triggered_Insight_Only()
        {
            var datasetVersionId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-auto-insight-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                AddProfileData(context, datasetVersion.Id, "amount");

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Manual insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"provider\":\"groq\"}",
                    CreationTime = new DateTime(2026, 4, 2, 11, 5, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Older automatic insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"profilingCompleted\",\"datasetProfileId\":1,\"provider\":\"groq\"}",
                    CreationTime = new DateTime(2026, 4, 2, 11, 10, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Latest automatic insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"profilingCompleted\",\"datasetProfileId\":2,\"provider\":\"groq\"}",
                    CreationTime = new DateTime(2026, 4, 2, 11, 20, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return datasetVersion.Id;
            });

            var result = await _datasetAiAppService.GetLatestAutomaticInsightAsync(new EntityDto<long>(datasetVersionId));

            result.ShouldNotBeNull();
            result.ResponseContent.ShouldBe("Latest automatic insight.");
        }

        [Fact]
        public async Task GetLatestAutomaticExperimentInsightAsync_Should_Return_The_Newest_Experiment_Triggered_Insight_Only()
        {
            var experimentId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-auto-experiment-insight-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var seeded = AddProfileData(context, datasetVersion.Id, "amount");
                var experiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 30, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = experiment.Id,
                    ResponseContent = "Manual experiment insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"provider\":\"groq\"}",
                    CreationTime = new DateTime(2026, 4, 2, 12, 31, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = experiment.Id,
                    ResponseContent = "Older automatic experiment insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"experimentCompleted\",\"mlExperimentId\":" + experiment.Id + "}",
                    CreationTime = new DateTime(2026, 4, 2, 12, 32, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = experiment.Id,
                    ResponseContent = "Latest automatic experiment insight.",
                    ResponseType = AIResponseType.Insight,
                    MetadataJson = "{\"generationTrigger\":\"experimentCompleted\",\"mlExperimentId\":" + experiment.Id + "}",
                    CreationTime = new DateTime(2026, 4, 2, 12, 33, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return experiment.Id;
            });

            var result = await _datasetAiAppService.GetLatestAutomaticExperimentInsightAsync(new EntityDto<long>(experimentId));

            result.ShouldNotBeNull();
            result.ResponseContent.ShouldBe("Latest automatic experiment insight.");
        }

        [Fact]
        public async Task GetConversationAsync_Should_Return_The_Persisted_Conversation_Summary()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-get-conversation-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 11, 30, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    UserQuery = "Summarize this data",
                    ResponseContent = "A concise summary for the assistant route.",
                    ResponseType = AIResponseType.Summary,
                    CreationTime = new DateTime(2026, 4, 2, 11, 31, 0, DateTimeKind.Utc)
                });
                context.SaveChanges();

                return conversation.Id;
            });

            var result = await _datasetAiAppService.GetConversationAsync(new EntityDto<long>(scenario));

            result.ShouldNotBeNull();
            result.Id.ShouldBe(scenario);
            result.ResponseCount.ShouldBe(1);
            result.LatestResponseType.ShouldBe(AIResponseType.Summary);
            result.LatestUserQuery.ShouldBe("Summarize this data");
            result.LatestResponsePreview.ShouldContain("assistant route");
        }

        [Fact]
        public async Task GetConversationsAsync_Should_Filter_By_Dataset_Version_And_Sort_By_Latest_Interaction()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-conversation-list-dataset", AbpSession.GetUserId());
                var rawVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var processedVersion = CreateDatasetVersion(context, dataset.Id, 2, DatasetVersionType.Processed, rawVersion.Id);

                var olderConversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
                }).Entity;
                var newerConversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = olderConversation.Id,
                    DatasetVersionId = rawVersion.Id,
                    ResponseContent = "Older raw-version conversation.",
                    ResponseType = AIResponseType.Summary,
                    CreationTime = new DateTime(2026, 4, 2, 10, 5, 0, DateTimeKind.Utc)
                });
                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = newerConversation.Id,
                    DatasetVersionId = processedVersion.Id,
                    ResponseContent = "Newer processed-version conversation.",
                    ResponseType = AIResponseType.QuestionAnswer,
                    CreationTime = new DateTime(2026, 4, 2, 12, 5, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();

                return new
                {
                    DatasetId = dataset.Id,
                    RawVersionId = rawVersion.Id,
                    ProcessedVersionId = processedVersion.Id,
                    OlderConversationId = olderConversation.Id,
                    NewerConversationId = newerConversation.Id
                };
            });

            var allResults = await _datasetAiAppService.GetConversationsAsync(new GetDatasetAiConversationsRequest
            {
                DatasetId = scenario.DatasetId,
                SkipCount = 0,
                MaxResultCount = 10
            });

            allResults.TotalCount.ShouldBe(2);
            allResults.Items.First().Id.ShouldBe(scenario.NewerConversationId);
            allResults.Items.Last().Id.ShouldBe(scenario.OlderConversationId);

            var filteredResults = await _datasetAiAppService.GetConversationsAsync(new GetDatasetAiConversationsRequest
            {
                DatasetId = scenario.DatasetId,
                DatasetVersionId = scenario.RawVersionId,
                SkipCount = 0,
                MaxResultCount = 10
            });

            filteredResults.TotalCount.ShouldBe(1);
            filteredResults.Items.Single().Id.ShouldBe(scenario.OlderConversationId);
        }

        [Fact]
        public async Task GetConversationsAsync_Should_Filter_By_MLExperimentId()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-experiment-conversation-list-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);
                var seeded = AddProfileData(context, datasetVersion.Id, "amount");
                var firstExperiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId);
                var secondExperiment = CreateCompletedMlExperiment(context, datasetVersion.Id, seeded.ColumnId, "linear_regression");

                var firstConversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 10, 0, DateTimeKind.Utc)
                }).Entity;

                var secondConversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 20, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = firstConversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = firstExperiment.Id,
                    ResponseContent = "First experiment response.",
                    ResponseType = AIResponseType.Summary
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = secondConversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    MLExperimentId = secondExperiment.Id,
                    ResponseContent = "Second experiment response.",
                    ResponseType = AIResponseType.Summary
                });

                context.SaveChanges();

                return new
                {
                    DatasetId = dataset.Id,
                    FirstExperimentId = firstExperiment.Id,
                    SecondConversationId = secondConversation.Id
                };
            });

            var result = await _datasetAiAppService.GetConversationsAsync(new GetDatasetAiConversationsRequest
            {
                DatasetId = scenario.DatasetId,
                MLExperimentId = scenario.FirstExperimentId,
                SkipCount = 0,
                MaxResultCount = 10
            });

            result.TotalCount.ShouldBe(1);
            result.Items.Single().LatestMLExperimentId.ShouldBe(scenario.FirstExperimentId);
            result.Items.Single().Id.ShouldNotBe(scenario.SecondConversationId);
        }

        [Fact]
        public async Task GetResponsesAsync_Should_Return_Chronological_Conversation_Turns()
        {
            var scenario = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-response-thread-dataset", AbpSession.GetUserId());
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = AbpSession.GetUserId(),
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    UserQuery = "First question",
                    ResponseContent = "First answer",
                    ResponseType = AIResponseType.QuestionAnswer,
                    CreationTime = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
                });
                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    UserQuery = "Second question",
                    ResponseContent = "Second answer",
                    ResponseType = AIResponseType.QuestionAnswer,
                    CreationTime = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc)
                });

                context.SaveChanges();
                return conversation.Id;
            });

            var result = await _datasetAiAppService.GetResponsesAsync(new GetDatasetAiResponsesRequest
            {
                ConversationId = scenario,
                SkipCount = 0,
                MaxResultCount = 10,
                IsChronological = true
            });

            result.TotalCount.ShouldBe(2);
            result.Items.First().UserQuery.ShouldBe("First question");
            result.Items.Last().UserQuery.ShouldBe("Second question");
        }

        [Fact]
        public async Task GetResponsesAsync_Should_Reject_A_Conversation_From_A_Different_Owner()
        {
            var conversationId = UsingDbContext(context =>
            {
                var dataset = CreateDataset(context, "ai-foreign-conversation-dataset", AbpSession.GetUserId() + 20);
                var datasetVersion = CreateDatasetVersion(context, dataset.Id, 1, DatasetVersionType.Raw);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    OwnerUserId = dataset.OwnerUserId,
                    LastInteractionTime = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc)
                }).Entity;

                context.SaveChanges();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = 1,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    ResponseContent = "Foreign response",
                    ResponseType = AIResponseType.Summary
                });
                context.SaveChanges();

                return conversation.Id;
            });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetAiAppService.GetResponsesAsync(new GetDatasetAiResponsesRequest
                {
                    ConversationId = conversationId,
                    SkipCount = 0,
                    MaxResultCount = 10
                }));

            exception.Message.ShouldBe("The requested AI conversation could not be found.");
        }

        private static Dataset CreateDataset(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name, long ownerUserId)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = 1,
                Name = name,
                Description = name + " description",
                SourceFormat = DatasetFormat.Csv,
                Status = DatasetStatus.Ready,
                OwnerUserId = ownerUserId,
                OriginalFileName = name + ".csv"
            }).Entity;

            context.SaveChanges();
            return dataset;
        }

        private static DatasetVersion CreateDatasetVersion(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetId,
            int versionNumber,
            DatasetVersionType versionType,
            long? parentVersionId = null)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = 1,
                DatasetId = datasetId,
                VersionNumber = versionNumber,
                VersionType = versionType,
                Status = DatasetVersionStatus.Active,
                ParentVersionId = parentVersionId,
                RowCount = 10,
                ColumnCount = 1,
                SchemaJson = "{\"columns\":[{\"name\":\"amount\"}]}",
                SizeBytes = 256,
                CreationTime = new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)
            }).Entity;

            context.SaveChanges();
            return datasetVersion;
        }

        private static ProfileSeedResult AddProfileData(AstraLab.EntityFrameworkCore.AstraLabDbContext context, long datasetVersionId, string columnName)
        {
            var column = context.DatasetColumns.Add(new DatasetColumn
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                Name = columnName,
                DataType = "decimal",
                IsDataTypeInferred = true,
                Ordinal = 1,
                NullCount = 2,
                DistinctCount = 6
            }).Entity;

            context.SaveChanges();

            var datasetProfile = context.DatasetProfiles.Add(new DatasetProfile
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                RowCount = 10,
                DuplicateRowCount = 1,
                DataHealthScore = 82.5m,
                SummaryJson = "{\"totalNullCount\":2,\"overallNullPercentage\":20.0,\"totalAnomalyCount\":1,\"overallAnomalyPercentage\":10.0}"
            }).Entity;

            context.SaveChanges();

            context.DatasetColumnProfiles.Add(new DatasetColumnProfile
            {
                TenantId = 1,
                DatasetProfileId = datasetProfile.Id,
                DatasetColumnId = column.Id,
                InferredDataType = "decimal",
                NullCount = 2,
                DistinctCount = 6,
                StatisticsJson = "{\"nullPercentage\":20.0,\"mean\":10.5,\"min\":1.0,\"max\":22.0,\"anomalyCount\":1,\"anomalyPercentage\":10.0,\"hasAnomalies\":true}"
            });

            context.SaveChanges();
            return new ProfileSeedResult(column.Id, datasetProfile.Id);
        }

        private static MLExperiment CreateCompletedMlExperiment(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetVersionId,
            long targetDatasetColumnId,
            string algorithmKey = "random_forest_classifier")
        {
            var experiment = context.MLExperiments.Add(new MLExperiment
            {
                TenantId = 1,
                DatasetVersionId = datasetVersionId,
                TargetDatasetColumnId = targetDatasetColumnId,
                Status = MLExperimentStatus.Completed,
                TaskType = MLTaskType.Classification,
                AlgorithmKey = algorithmKey,
                TrainingConfigurationJson = "{\"testSize\":0.2}",
                ExecutedAt = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
                StartedAtUtc = new DateTime(2026, 4, 2, 12, 1, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2026, 4, 2, 12, 2, 0, DateTimeKind.Utc),
                WarningsJson = "[\"class_imbalance\"]"
            }).Entity;

            context.SaveChanges();

            var model = context.MLModels.Add(new MLModel
            {
                TenantId = 1,
                MLExperimentId = experiment.Id,
                ModelType = algorithmKey,
                ArtifactStorageProvider = "local-filesystem",
                ArtifactStorageKey = "ml-artifacts/test/model.joblib",
                PerformanceSummaryJson = "{\"primaryMetric\":\"accuracy\"}",
                WarningsJson = "[\"small_validation_split\"]"
            }).Entity;

            context.SaveChanges();

            context.MLModelMetrics.Add(new MLModelMetric
            {
                TenantId = 1,
                MLModelId = model.Id,
                MetricName = "accuracy",
                MetricValue = 0.91m
            });

            context.SaveChanges();
            return experiment;
        }

        private class ProfileSeedResult
        {
            public ProfileSeedResult(long columnId, long datasetProfileId)
            {
                ColumnId = columnId;
                DatasetProfileId = datasetProfileId;
            }

            public long ColumnId { get; }

            public long DatasetProfileId { get; }
        }
    }
}
