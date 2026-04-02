using AstraLab.Core.Domains.AI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.AI;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class AiPromptBuilder_Tests
    {
        private readonly IAiPromptBuilder _aiPromptBuilder;

        public AiPromptBuilder_Tests()
        {
            _aiPromptBuilder = new AiPromptBuilder();
        }

        [Fact]
        public void Build_Should_Inject_Base_Dataset_Context_For_Summaries()
        {
            var result = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Summary,
                DatasetContext = BuildDatasetContext(),
                EnrichmentContext = null
            });

            result.SystemInstructions.ShouldContain("dataset assistant");
            result.UserMessage.ShouldContain("Dataset context JSON:");
            result.UserMessage.ShouldContain("sales-dataset");
            result.UserMessage.ShouldContain("Generate a dataset summary.");
            result.UserMessage.ShouldNotContain("Additional enrichment JSON:");
        }

        [Fact]
        public void Build_Should_Include_Enrichment_For_Recommendations_And_Question_Text_For_QA()
        {
            var recommendationResult = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Recommendation,
                DatasetContext = BuildDatasetContext(),
                EnrichmentContext = new AiDatasetInsightContext
                {
                    DatasetVersionId = 100,
                    HighSignalColumns = new[]
                    {
                        new AiInsightColumnContext
                        {
                            DatasetColumnId = 3,
                            Name = "amount",
                            DataType = "decimal",
                            NullPercentage = 40m
                        }
                    }
                }
            });

            recommendationResult.UserMessage.ShouldContain("Additional enrichment JSON:");
            recommendationResult.UserMessage.ShouldContain("amount");

            var questionAnswerResult = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.QuestionAnswer,
                DatasetContext = BuildDatasetContext(),
                UserQuestion = "What are the biggest risks?"
            });

            questionAnswerResult.UserMessage.ShouldContain("User question:");
            questionAnswerResult.UserMessage.ShouldContain("What are the biggest risks?");
            questionAnswerResult.UserMessage.ShouldContain("Answer first in one short paragraph");
        }

        [Fact]
        public void Build_Should_Use_Four_Section_Automatic_Insight_Prompt_Rules()
        {
            var result = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Insight,
                DatasetContext = BuildDatasetContext(),
                EnrichmentContext = new AiDatasetInsightContext
                {
                    DatasetVersionId = 100
                },
                IsAutomaticProfilingInsight = true
            });

            result.SystemInstructions.ShouldContain("Return exactly four short sections titled Summary");
            result.UserMessage.ShouldContain("Generate an automatic dataset insight after profiling completed.");
            result.UserMessage.ShouldContain("Use exactly these headings in order: Summary, Key data quality issues, Notable patterns or anomalies, Suggested next steps.");
        }

        [Fact]
        public void Build_Should_Inject_Ml_Experiment_Context_And_Use_Experiment_Specific_Rules()
        {
            var result = _aiPromptBuilder.Build(new AiPromptBuildRequest
            {
                ResponseType = AIResponseType.Summary,
                DatasetContext = BuildDatasetContext(),
                MlExperimentContext = new AiMlExperimentContext
                {
                    MLExperimentId = 55,
                    DatasetVersionId = 100,
                    AlgorithmKey = "random_forest_classifier",
                    ModelType = "random_forest_classifier",
                    HasModelOutput = true,
                    FeatureNames = new[] { "amount", "region" },
                    Metrics = new[]
                    {
                        new AiMlMetricContext
                        {
                            MetricName = "accuracy",
                            MetricValue = 0.91m
                        }
                    }
                }
            });

            result.SystemInstructions.ShouldContain("machine learning experiment context");
            result.UserMessage.ShouldContain("Machine learning experiment context JSON:");
            result.UserMessage.ShouldContain("random_forest_classifier");
            result.UserMessage.ShouldContain("Generate a machine learning experiment summary.");
        }

        private static AiDatasetContext BuildDatasetContext()
        {
            return new AiDatasetContext
            {
                Dataset = new AiDatasetSummaryContext
                {
                    DatasetId = 1,
                    Name = "sales-dataset",
                    Description = "Quarterly sales data",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = 42
                },
                Version = new AiDatasetVersionContext
                {
                    DatasetVersionId = 100,
                    DatasetId = 1,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active
                },
                Schema = new AiSchemaContext
                {
                    TotalColumnCount = 2,
                    HasSchemaJson = true,
                    SchemaJsonPreview = "{\"columns\":[{\"name\":\"amount\"}]}"
                },
                Profiling = new AiProfilingContext
                {
                    ProfileId = 88,
                    RowCount = 10,
                    DuplicateRowCount = 1,
                    DataHealthScore = 82.5m
                },
                Columns = new[]
                {
                    new AiColumnContext
                    {
                        DatasetColumnId = 3,
                        Name = "amount",
                        Ordinal = 1,
                        DataType = "decimal",
                        HasDetailedProfile = true,
                        NullPercentage = 20m
                    }
                },
                DetailedColumnCount = 1
            };
        }
    }
}
