using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abp.Dependency;
using AstraLab.Core.Domains.AI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Builds concise deterministic prompts for dataset AI workflows.
    /// </summary>
    public class AiPromptBuilder : IAiPromptBuilder, ITransientDependency
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        /// <summary>
        /// Builds a prompt payload for the requested dataset AI task.
        /// </summary>
        public AiPromptBuildResult Build(AiPromptBuildRequest request)
        {
            return new AiPromptBuildResult
            {
                SystemInstructions = BuildSystemInstructions(request.ResponseType, request.IsAutomaticProfilingInsight),
                UserMessage = BuildUserMessage(request)
            };
        }

        /// <summary>
        /// Builds the task-specific system prompt.
        /// </summary>
        private static string BuildSystemInstructions(AIResponseType responseType, bool isAutomaticProfilingInsight)
        {
            if (isAutomaticProfilingInsight)
            {
                return "You are AstraLab's dataset assistant. Use only the provided dataset context and enrichment. " +
                       "Do not invent raw values, rows, or external facts. Write for a non-technical user in clear, plain language. " +
                       "Return exactly four short sections titled Summary, Key data quality issues, Notable patterns or anomalies, and Suggested next steps.";
            }

            var taskGuidance = responseType switch
            {
                AIResponseType.Summary => "Write a concise dataset summary with a short overview and the most important risks.",
                AIResponseType.Insight => "Identify the most important data-quality and structure insights. Focus on the top findings only.",
                AIResponseType.Recommendation => "Recommend prioritized cleaning and transformation actions with brief rationale and expected impact.",
                AIResponseType.QuestionAnswer => "Answer the user's question directly first, then support it with brief evidence from the dataset context.",
                _ => "Provide a concise and grounded explanation based only on the supplied dataset context."
            };

            return "You are AstraLab's dataset assistant. Use only the provided dataset context and enrichment. Do not invent raw values, rows, or external facts. " +
                   "Be concise, practical, and specific. When context is missing, say so clearly. " +
                   taskGuidance;
        }

        /// <summary>
        /// Builds the final user prompt in a deterministic serialized format.
        /// </summary>
        private static string BuildUserMessage(AiPromptBuildRequest request)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Task:");
            builder.AppendLine(GetTaskLabel(request.ResponseType, request.IsAutomaticProfilingInsight));
            builder.AppendLine();

            if (!string.IsNullOrWhiteSpace(request.UserQuestion))
            {
                builder.AppendLine("User question:");
                builder.AppendLine(request.UserQuestion.Trim());
                builder.AppendLine();
            }

            builder.AppendLine("Dataset context JSON:");
            builder.AppendLine(JsonSerializer.Serialize(request.DatasetContext, SerializerOptions));

            if (request.EnrichmentContext != null)
            {
                builder.AppendLine();
                builder.AppendLine("Additional enrichment JSON:");
                builder.AppendLine(JsonSerializer.Serialize(request.EnrichmentContext, SerializerOptions));
            }

            builder.AppendLine();
            builder.AppendLine("Response rules:");
            builder.AppendLine(GetResponseRules(request.ResponseType, request.IsAutomaticProfilingInsight));
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Gets the concise task label included in the prompt body.
        /// </summary>
        private static string GetTaskLabel(AIResponseType responseType, bool isAutomaticProfilingInsight)
        {
            if (isAutomaticProfilingInsight)
            {
                return "Generate an automatic dataset insight after profiling completed.";
            }

            return responseType switch
            {
                AIResponseType.Summary => "Generate a dataset summary.",
                AIResponseType.Insight => "Generate concise data-quality insights.",
                AIResponseType.Recommendation => "Generate cleaning and transformation recommendations.",
                AIResponseType.QuestionAnswer => "Answer the user's dataset question.",
                _ => "Explain the dataset context."
            };
        }

        /// <summary>
        /// Gets the task-specific output rules that keep responses concise and useful.
        /// </summary>
        private static string GetResponseRules(AIResponseType responseType, bool isAutomaticProfilingInsight)
        {
            if (isAutomaticProfilingInsight)
            {
                return "Use exactly these headings in order: Summary, Key data quality issues, Notable patterns or anomalies, Suggested next steps. " +
                       "Keep each section short, plain-language, and grounded in the provided profiling and schema context only.";
            }

            return responseType switch
            {
                AIResponseType.Summary => "Use at most two short paragraphs or four bullet points. Mention structure, quality, and one notable risk.",
                AIResponseType.Insight => "List up to three findings. Prioritize nulls, anomalies, duplicates, and schema issues.",
                AIResponseType.Recommendation => "List up to four prioritized actions. Include a brief why for each action and mention relevant transformations when applicable.",
                AIResponseType.QuestionAnswer => "Answer first in one short paragraph, then add up to three short evidence bullets if needed.",
                _ => "Keep the answer short and grounded."
            };
        }
    }
}
