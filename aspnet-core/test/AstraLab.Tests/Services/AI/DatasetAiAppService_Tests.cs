using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
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

        private static void AddProfileData(AstraLab.EntityFrameworkCore.AstraLabDbContext context, long datasetVersionId, string columnName)
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
        }
    }
}
