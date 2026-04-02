using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Abp.Dependency;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Renders stakeholder-facing analytics reports to canonical HTML content.
    /// </summary>
    public class AnalyticsReportHtmlRenderer : IAnalyticsReportHtmlRenderer, ITransientDependency
    {
        /// <summary>
        /// Renders the supplied analytics summary to canonical HTML.
        /// </summary>
        public string Render(DatasetAnalyticsSummaryDto summary)
        {
            var title = BuildTitle(summary);
            var narrativeSections = ParseNarrativeSections(summary?.Narrative?.Content);

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\" />");
            html.AppendLine("<title>" + Encode(title) + "</title>");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Arial,Helvetica,sans-serif;color:#1f2937;line-height:1.6;margin:0;padding:32px;background:#ffffff;}");
            html.AppendLine("article{max-width:900px;margin:0 auto;}");
            html.AppendLine("h1{font-size:30px;margin-bottom:8px;}");
            html.AppendLine("h2{font-size:22px;margin-top:32px;margin-bottom:12px;padding-bottom:6px;border-bottom:1px solid #e5e7eb;}");
            html.AppendLine("p{margin:0 0 12px;}");
            html.AppendLine("ul{margin:0 0 16px 20px;padding:0;}");
            html.AppendLine("li{margin-bottom:8px;}");
            html.AppendLine(".meta{color:#6b7280;margin-bottom:18px;}");
            html.AppendLine(".summary{background:#f9fafb;border:1px solid #e5e7eb;border-radius:8px;padding:16px;margin:18px 0 24px;}");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<article>");
            html.AppendLine("<h1>" + Encode(title) + "</h1>");
            html.AppendLine("<p class=\"meta\">Dataset version " + summary.VersionNumber + " • " + Encode(summary.VersionType.ToString()) + " • " + Encode(summary.SourceFormat.ToString()) + "</p>");
            html.AppendLine("<div class=\"summary\"><p>" + Encode(BuildReportSummary(summary, narrativeSections)) + "</p></div>");

            AppendSection(html, "Overview", BuildOverviewParagraphs(summary, narrativeSections));
            AppendSection(html, "Dataset quality highlights", BuildQualityHighlights(summary));
            AppendSection(html, "Transformation outcomes", BuildTransformationHighlights(summary));
            AppendSection(html, "AI findings", BuildAiFindings(summary));
            AppendSection(html, "ML highlights", BuildMlHighlights(summary));
            AppendSection(html, "Suggested next steps", BuildSuggestedNextSteps(summary, narrativeSections));

            html.AppendLine("</article>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }

        /// <summary>
        /// Appends a report section with paragraphs and bullet items.
        /// </summary>
        private static void AppendSection(StringBuilder html, string heading, IReadOnlyList<string> lines)
        {
            html.AppendLine("<section>");
            html.AppendLine("<h2>" + Encode(heading) + "</h2>");

            if (lines == null || lines.Count == 0)
            {
                html.AppendLine("<p>No stakeholder-facing information is available for this section yet.</p>");
                html.AppendLine("</section>");
                return;
            }

            var paragraphLines = lines.Where(item => !item.StartsWith("- ", StringComparison.Ordinal)).ToList();
            var bulletLines = lines.Where(item => item.StartsWith("- ", StringComparison.Ordinal)).Select(item => item.Substring(2)).ToList();

            foreach (var paragraph in paragraphLines)
            {
                html.AppendLine("<p>" + Encode(paragraph) + "</p>");
            }

            if (bulletLines.Count > 0)
            {
                html.AppendLine("<ul>");
                foreach (var bullet in bulletLines)
                {
                    html.AppendLine("<li>" + Encode(bullet) + "</li>");
                }

                html.AppendLine("</ul>");
            }

            html.AppendLine("</section>");
        }

        /// <summary>
        /// Builds the stakeholder-facing report title.
        /// </summary>
        private static string BuildTitle(DatasetAnalyticsSummaryDto summary)
        {
            return string.Format("{0} Analytics Report (Version {1})", summary?.DatasetName ?? "Dataset", summary?.VersionNumber ?? 0);
        }

        /// <summary>
        /// Builds the short report summary shown near the title.
        /// </summary>
        private static string BuildReportSummary(DatasetAnalyticsSummaryDto summary, IReadOnlyDictionary<string, List<string>> narrativeSections)
        {
            var overview = ReadSectionText(narrativeSections, "overview");
            if (!string.IsNullOrWhiteSpace(overview))
            {
                return overview;
            }

            var quality = summary?.QualityHighlights;
            if (quality?.HasProfile ?? false)
            {
                return string.Format(
                    "{0} contains {1} columns and {2} rows with a data health score of {3}.",
                    summary.DatasetName,
                    summary.ColumnCount ?? 0,
                    quality.RowCount ?? 0,
                    quality.DataHealthScore?.ToString("0.##") ?? "unavailable");
            }

            return "This report summarizes the latest stored analytical outputs available for the selected dataset version.";
        }

        /// <summary>
        /// Builds the overview section paragraphs.
        /// </summary>
        private static IReadOnlyList<string> BuildOverviewParagraphs(DatasetAnalyticsSummaryDto summary, IReadOnlyDictionary<string, List<string>> narrativeSections)
        {
            var overview = ReadSectionLines(narrativeSections, "overview");
            if (overview.Count > 0)
            {
                return overview;
            }

            return new List<string>
            {
                string.Format(
                    "{0} is currently in the {1} state and this report brings together profiling, transformation, AI, and machine-learning outputs for version {2}.",
                    summary.DatasetName,
                    summary.VersionStatus,
                    summary.VersionNumber)
            };
        }

        /// <summary>
        /// Builds the quality highlights section content.
        /// </summary>
        private static IReadOnlyList<string> BuildQualityHighlights(DatasetAnalyticsSummaryDto summary)
        {
            var quality = summary?.QualityHighlights;
            if (!(quality?.HasProfile ?? false))
            {
                return new List<string>
                {
                    "No persisted profiling snapshot is available for this dataset version yet."
                };
            }

            var lines = new List<string>
            {
                string.Format(
                    "Profiled rows: {0}. Duplicate rows: {1}. Data health score: {2}.",
                    quality.RowCount ?? 0,
                    quality.DuplicateRowCount ?? 0,
                    quality.DataHealthScore?.ToString("0.##") ?? "unavailable"),
                string.Format(
                    "Total null count: {0}. Overall null percentage: {1}%. Total anomaly count: {2}. Overall anomaly percentage: {3}%.",
                    quality.TotalNullCount ?? 0,
                    quality.OverallNullPercentage?.ToString("0.##") ?? "0",
                    quality.TotalAnomalyCount ?? 0,
                    quality.OverallAnomalyPercentage?.ToString("0.##") ?? "0")
            };

            lines.AddRange((quality.HighRiskColumns ?? new List<DatasetQualityColumnHighlightDto>())
                .Select(item => string.Format(
                    "- {0} ({1}): nulls {2}% and anomalies {3}%.",
                    item.Name,
                    item.DataType,
                    item.NullPercentage.ToString("0.##"),
                    item.AnomalyPercentage.ToString("0.##"))));

            return lines;
        }

        /// <summary>
        /// Builds the transformation section content.
        /// </summary>
        private static IReadOnlyList<string> BuildTransformationHighlights(DatasetAnalyticsSummaryDto summary)
        {
            var transformations = summary?.TransformationOutcomes ?? new List<TransformationOutcomeSummaryDto>();
            if (transformations.Count == 0)
            {
                return new List<string>
                {
                    "No persisted transformation outcomes are linked to this dataset version lineage yet."
                };
            }

            return transformations.Select(item => string.Format(
                "- {0} ran at {1:u}. {2}",
                item.TransformationType,
                item.ExecutedAt,
                string.IsNullOrWhiteSpace(item.SummaryPreview) ? "No execution summary was stored." : item.SummaryPreview)).ToList();
        }

        /// <summary>
        /// Builds the persisted AI findings section content.
        /// </summary>
        private static IReadOnlyList<string> BuildAiFindings(DatasetAnalyticsSummaryDto summary)
        {
            var findings = summary?.AiFindings;
            if (findings == null || (findings.StoredAiResponseCount == 0 && findings.StoredInsightRecordCount == 0))
            {
                return new List<string>
                {
                    "No persisted AI findings are available for this dataset version yet."
                };
            }

            var lines = new List<string>
            {
                string.Format(
                    "Stored AI responses: {0}. Stored analytics insight records: {1}.",
                    findings.StoredAiResponseCount,
                    findings.StoredInsightRecordCount)
            };

            if (!string.IsNullOrWhiteSpace(findings.LatestAutomaticInsightPreview))
            {
                lines.Add("- Latest automatic insight: " + findings.LatestAutomaticInsightPreview);
            }

            if (!string.IsNullOrWhiteSpace(findings.LatestManualInsightPreview))
            {
                lines.Add("- Latest manual insight: " + findings.LatestManualInsightPreview);
            }

            if (!string.IsNullOrWhiteSpace(findings.LatestRecommendationPreview))
            {
                lines.Add("- Latest recommendation: " + findings.LatestRecommendationPreview);
            }

            return lines;
        }

        /// <summary>
        /// Builds the machine-learning highlights section content.
        /// </summary>
        private static IReadOnlyList<string> BuildMlHighlights(DatasetAnalyticsSummaryDto summary)
        {
            var ml = summary?.MlExperimentHighlights;
            if (!(ml?.HasCompletedExperiment ?? false))
            {
                return new List<string>
                {
                    "No completed machine-learning experiment is linked to this dataset version yet."
                };
            }

            var lines = new List<string>
            {
                string.Format(
                    "Latest completed experiment used {0} for a {1} task targeting {2}.",
                    ml.AlgorithmKey ?? "an unspecified algorithm",
                    ml.TaskType?.ToString() ?? "machine-learning",
                    ml.TargetColumnName ?? "an unspecified target"),
                string.Format(
                    "Primary metric: {0} = {1}. Selected features: {2}.",
                    ml.PrimaryMetricName ?? "unavailable",
                    ml.PrimaryMetricValue?.ToString("0.####") ?? "unavailable",
                    ml.FeatureCount)
            };

            lines.AddRange((ml.TopFeatureImportances ?? new List<AnalyticsMlFeatureImportanceDto>())
                .Select(item => string.Format("- Feature importance {0}: {1} ({2}).", item.Rank, item.ColumnName, item.ImportanceScore.ToString("0.####"))));

            lines.AddRange((ml.Warnings ?? new List<string>())
                .Select(item => "- Warning: " + item));

            return lines;
        }

        /// <summary>
        /// Builds the suggested-next-steps section content.
        /// </summary>
        private static IReadOnlyList<string> BuildSuggestedNextSteps(DatasetAnalyticsSummaryDto summary, IReadOnlyDictionary<string, List<string>> narrativeSections)
        {
            var narrativeSteps = ReadSectionLines(narrativeSections, "suggested next steps");
            if (narrativeSteps.Count > 0)
            {
                return narrativeSteps;
            }

            var steps = new List<string>();
            if ((summary?.QualityHighlights?.HighRiskColumns?.Count ?? 0) > 0)
            {
                steps.Add("- Start with the columns that currently show the highest null or anomaly percentages.");
            }

            if ((summary?.TransformationOutcomes?.Count ?? 0) == 0)
            {
                steps.Add("- Apply at least one cleaning or shaping transformation and review the next profile snapshot.");
            }

            if (!(summary?.MlExperimentHighlights?.HasCompletedExperiment ?? false))
            {
                steps.Add("- Run a first machine-learning experiment once the highest-risk data quality issues are addressed.");
            }

            if (steps.Count == 0)
            {
                steps.Add("- Use the current highlights to validate assumptions with stakeholders and plan the next round of exploration.");
            }

            return steps;
        }

        /// <summary>
        /// Parses the generated narrative into its titled sections.
        /// </summary>
        private static IReadOnlyDictionary<string, List<string>> ParseNarrativeSections(string content)
        {
            var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(content))
            {
                return sections;
            }

            var currentSection = string.Empty;
            var lines = content
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(item => item?.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();

            foreach (var line in lines)
            {
                if (IsNarrativeHeading(line))
                {
                    currentSection = line.Trim().TrimEnd(':');
                    if (!sections.ContainsKey(currentSection))
                    {
                        sections[currentSection] = new List<string>();
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentSection))
                {
                    continue;
                }

                sections[currentSection].Add(line.StartsWith("- ", StringComparison.Ordinal) ? line : line);
            }

            return sections;
        }

        /// <summary>
        /// Determines whether the supplied line is one of the expected analytics narrative headings.
        /// </summary>
        private static bool IsNarrativeHeading(string line)
        {
            return string.Equals(line, "Overview", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(line, "Key risks", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(line, "Recent changes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(line, "ML highlights", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(line, "Suggested next steps", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads a narrative section as a list of lines.
        /// </summary>
        private static IReadOnlyList<string> ReadSectionLines(IReadOnlyDictionary<string, List<string>> sections, string sectionName)
        {
            if (sections == null)
            {
                return new List<string>();
            }

            return sections.TryGetValue(sectionName, out var lines)
                ? lines
                : new List<string>();
        }

        /// <summary>
        /// Reads the first line of a narrative section when available.
        /// </summary>
        private static string ReadSectionText(IReadOnlyDictionary<string, List<string>> sections, string sectionName)
        {
            return ReadSectionLines(sections, sectionName).FirstOrDefault();
        }

        /// <summary>
        /// HTML-encodes report content.
        /// </summary>
        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
