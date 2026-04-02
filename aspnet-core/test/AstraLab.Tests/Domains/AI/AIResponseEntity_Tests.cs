using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Domains.AI
{
    public class AIResponseEntity_Tests : AstraLabTestBase
    {
        [Fact]
        public async Task Should_Persist_A_One_Off_AI_Response_As_A_Single_Response_Conversation_And_Load_The_Full_Relationship_Graph()
        {
            long conversationId = 0;
            long responseId = 0;
            long datasetId = 0;
            long datasetVersionId = 0;
            DateTime lastInteractionTime = new DateTime(2026, 4, 2, 8, 30, 0, DateTimeKind.Utc);

            await UsingDbContextAsync(async context =>
            {
                var dataset = await CreateDatasetAsync(context, "ai-one-off-dataset");
                var datasetVersion = await CreateDatasetVersionAsync(context, dataset.Id, 1, DatasetVersionType.Raw);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    OwnerUserId = dataset.OwnerUserId,
                    LastInteractionTime = lastInteractionTime
                }).Entity;

                await context.SaveChangesAsync();

                var response = context.AIResponses.Add(new AIResponse
                {
                    TenantId = dataset.TenantId,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = datasetVersion.Id,
                    UserQuery = "Summarize this dataset.",
                    ResponseContent = "This dataset contains one raw version with clean structural metadata.",
                    ResponseType = AIResponseType.Summary,
                    MetadataJson = "{\"source\":\"dataset-profile\"}"
                }).Entity;

                await context.SaveChangesAsync();

                datasetId = dataset.Id;
                datasetVersionId = datasetVersion.Id;
                conversationId = conversation.Id;
                responseId = response.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var conversation = await context.AIConversations
                    .Include(item => item.Dataset)
                    .Include(item => item.Responses)
                    .SingleAsync(item => item.Id == conversationId);

                conversation.DatasetId.ShouldBe(datasetId);
                conversation.Dataset.Id.ShouldBe(datasetId);
                conversation.OwnerUserId.ShouldBe(AbpSession.GetUserId());
                conversation.LastInteractionTime.ShouldBe(lastInteractionTime);
                conversation.Responses.Count.ShouldBe(1);
                conversation.Responses.Single().Id.ShouldBe(responseId);

                var response = await context.AIResponses
                    .Include(item => item.AIConversation)
                    .Include(item => item.DatasetVersion)
                    .SingleAsync(item => item.Id == responseId);

                response.AIConversation.Id.ShouldBe(conversationId);
                response.DatasetVersion.Id.ShouldBe(datasetVersionId);
                response.UserQuery.ShouldBe("Summarize this dataset.");
                response.ResponseContent.ShouldBe("This dataset contains one raw version with clean structural metadata.");
                response.ResponseType.ShouldBe(AIResponseType.Summary);
                response.MetadataJson.ShouldBe("{\"source\":\"dataset-profile\"}");
            });
        }

        [Fact]
        public async Task Should_Persist_Multiple_AI_Responses_In_A_Single_Conversation_In_Chronological_Order()
        {
            long conversationId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = await CreateDatasetAsync(context, "ai-multi-turn-dataset");
                var rawVersion = await CreateDatasetVersionAsync(context, dataset.Id, 1, DatasetVersionType.Raw);
                var processedVersion = await CreateDatasetVersionAsync(context, dataset.Id, 2, DatasetVersionType.Processed, rawVersion.Id);

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    OwnerUserId = dataset.OwnerUserId,
                    LastInteractionTime = new DateTime(2026, 4, 2, 9, 5, 0, DateTimeKind.Utc)
                }).Entity;

                await context.SaveChangesAsync();

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = dataset.TenantId,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = rawVersion.Id,
                    UserQuery = "What quality issues should I fix first?",
                    ResponseContent = "Start with missing-value handling on the raw version.",
                    ResponseType = AIResponseType.Recommendation,
                    CreationTime = new DateTime(2026, 4, 2, 9, 0, 0, DateTimeKind.Utc)
                });

                context.AIResponses.Add(new AIResponse
                {
                    TenantId = dataset.TenantId,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = processedVersion.Id,
                    UserQuery = "Explain why this processed version looks better.",
                    ResponseContent = "The processed version reduces null-heavy rows and keeps the dominant schema intact.",
                    ResponseType = AIResponseType.Explanation,
                    CreationTime = new DateTime(2026, 4, 2, 9, 1, 0, DateTimeKind.Utc)
                });

                await context.SaveChangesAsync();
                conversationId = conversation.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var orderedResponses = await context.AIResponses
                    .Where(item => item.AIConversationId == conversationId)
                    .OrderBy(item => item.CreationTime)
                    .Select(item => item.ResponseContent)
                    .ToListAsync();

                orderedResponses.Count.ShouldBe(2);
                orderedResponses[0].ShouldBe("Start with missing-value handling on the raw version.");
                orderedResponses[1].ShouldBe("The processed version reduces null-heavy rows and keeps the dominant schema intact.");
            });
        }

        [Fact]
        public async Task Should_Persist_An_AI_Response_With_An_Optional_Dataset_Transformation_Link()
        {
            long responseId = 0;
            long transformationId = 0;

            await UsingDbContextAsync(async context =>
            {
                var dataset = await CreateDatasetAsync(context, "ai-transformation-link-dataset");
                var sourceVersion = await CreateDatasetVersionAsync(context, dataset.Id, 1, DatasetVersionType.Raw);
                var resultVersion = await CreateDatasetVersionAsync(context, dataset.Id, 2, DatasetVersionType.Processed, sourceVersion.Id);

                var transformation = context.DatasetTransformations.Add(new DatasetTransformation
                {
                    TenantId = dataset.TenantId,
                    SourceDatasetVersionId = sourceVersion.Id,
                    ResultDatasetVersionId = resultVersion.Id,
                    TransformationType = DatasetTransformationType.RemoveDuplicates,
                    ConfigurationJson = "{\"columns\":[\"customerId\"]}",
                    ExecutionOrder = 1,
                    ExecutedAt = new DateTime(2026, 4, 2, 9, 45, 0, DateTimeKind.Utc),
                    SummaryJson = "{\"removedRows\":14}"
                }).Entity;

                await context.SaveChangesAsync();

                var conversation = context.AIConversations.Add(new AIConversation
                {
                    TenantId = dataset.TenantId,
                    DatasetId = dataset.Id,
                    OwnerUserId = dataset.OwnerUserId,
                    LastInteractionTime = new DateTime(2026, 4, 2, 9, 46, 0, DateTimeKind.Utc)
                }).Entity;

                await context.SaveChangesAsync();

                var response = context.AIResponses.Add(new AIResponse
                {
                    TenantId = dataset.TenantId,
                    AIConversationId = conversation.Id,
                    DatasetVersionId = resultVersion.Id,
                    ResponseContent = "Removing duplicate customer rows improved the data health score.",
                    ResponseType = AIResponseType.Insight,
                    DatasetTransformationId = transformation.Id
                }).Entity;

                await context.SaveChangesAsync();

                transformationId = transformation.Id;
                responseId = response.Id;
            });

            await UsingDbContextAsync(async context =>
            {
                var response = await context.AIResponses
                    .Include(item => item.DatasetTransformation)
                    .SingleAsync(item => item.Id == responseId);

                response.DatasetTransformationId.ShouldBe(transformationId);
                response.DatasetTransformation.ShouldNotBeNull();
                response.DatasetTransformation.Id.ShouldBe(transformationId);
            });
        }

        [Fact]
        public void Should_Define_The_Expected_AI_Conversation_Index_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIConversation));
                var index = entityType.GetIndexes()
                    .Single(item => item.Properties.Select(property => property.Name)
                        .SequenceEqual(new[]
                        {
                            nameof(AIConversation.TenantId),
                            nameof(AIConversation.DatasetId),
                            nameof(AIConversation.OwnerUserId),
                            nameof(AIConversation.LastInteractionTime)
                        }));

                index.IsUnique.ShouldBeFalse();
            });
        }

        [Fact]
        public void Should_Define_The_Expected_AI_Response_Indexes_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIResponse));

                var conversationIndex = entityType.GetIndexes()
                    .Single(item => item.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(AIResponse.AIConversationId), nameof(AIResponse.CreationTime) }));

                var datasetVersionIndex = entityType.GetIndexes()
                    .Single(item => item.Properties.Select(property => property.Name)
                        .SequenceEqual(new[]
                        {
                            nameof(AIResponse.TenantId),
                            nameof(AIResponse.DatasetVersionId),
                            nameof(AIResponse.ResponseType),
                            nameof(AIResponse.CreationTime)
                        }));

                var transformationIndex = entityType.GetIndexes()
                    .Single(item => item.Properties.Select(property => property.Name)
                        .SequenceEqual(new[] { nameof(AIResponse.DatasetTransformationId) }));

                conversationIndex.IsUnique.ShouldBeFalse();
                datasetVersionIndex.IsUnique.ShouldBeFalse();
                transformationIndex.IsUnique.ShouldBeFalse();
            });
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Dataset_To_AI_Conversation_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIConversation));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(item =>
                        item.Properties.Single().Name == nameof(AIConversation.DatasetId) &&
                        item.PrincipalEntityType.ClrType == typeof(Dataset));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_AI_Conversation_To_AI_Response_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIResponse));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(item =>
                        item.Properties.Single().Name == nameof(AIResponse.AIConversationId) &&
                        item.PrincipalEntityType.ClrType == typeof(AIConversation));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Cascade_Delete_From_Dataset_Version_To_AI_Response_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIResponse));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(item =>
                        item.Properties.Single().Name == nameof(AIResponse.DatasetVersionId) &&
                        item.PrincipalEntityType.ClrType == typeof(DatasetVersion));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
            });
        }

        [Fact]
        public void Should_Define_Restrict_Delete_From_Dataset_Transformation_To_AI_Response_In_Model()
        {
            UsingDbContext(context =>
            {
                var entityType = context.Model.FindEntityType(typeof(AIResponse));
                var foreignKey = entityType.GetForeignKeys()
                    .Single(item =>
                        item.Properties.Single().Name == nameof(AIResponse.DatasetTransformationId) &&
                        item.PrincipalEntityType.ClrType == typeof(DatasetTransformation));

                foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
            });
        }

        private async Task<Dataset> CreateDatasetAsync(AstraLab.EntityFrameworkCore.AstraLabDbContext context, string name)
        {
            var dataset = context.Datasets.Add(new Dataset
            {
                TenantId = AbpSession.GetTenantId(),
                Name = name,
                SourceFormat = DatasetFormat.Csv,
                OwnerUserId = AbpSession.GetUserId(),
                OriginalFileName = name + ".csv"
            }).Entity;

            await context.SaveChangesAsync();
            return dataset;
        }

        private async Task<DatasetVersion> CreateDatasetVersionAsync(
            AstraLab.EntityFrameworkCore.AstraLabDbContext context,
            long datasetId,
            int versionNumber,
            DatasetVersionType versionType,
            long? parentVersionId = null)
        {
            var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
            {
                TenantId = AbpSession.GetTenantId(),
                DatasetId = datasetId,
                VersionNumber = versionNumber,
                VersionType = versionType,
                Status = DatasetVersionStatus.Active,
                ParentVersionId = parentVersionId,
                SizeBytes = 256
            }).Entity;

            await context.SaveChangesAsync();
            return datasetVersion;
        }
    }
}
