using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Abp.Dependency;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Renders analytics report HTML to a stakeholder-facing PDF document using QuestPDF.
    /// </summary>
    public class QuestPdfAnalyticsReportPdfRenderer : IAnalyticsReportPdfRenderer, ITransientDependency
    {
        /// <summary>
        /// Renders the supplied report title and canonical HTML content to a PDF payload.
        /// </summary>
        public byte[] Render(string title, string htmlContent)
        {
            var sections = ExtractSections(htmlContent);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(style => style.FontSize(11).FontColor(Colors.Grey.Darken4));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text(title ?? "Analytics Report").Bold().FontSize(20).FontColor(Colors.Blue.Darken3);
                            column.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });

                    page.Content()
                        .PaddingVertical(16)
                        .Column(column =>
                        {
                            column.Spacing(14);

                            foreach (var section in sections)
                            {
                                column.Item().Text(section.Heading).Bold().FontSize(15).FontColor(Colors.Grey.Darken3);

                                foreach (var paragraph in section.Paragraphs)
                                {
                                    column.Item().Text(paragraph);
                                }

                                foreach (var bullet in section.Bullets)
                                {
                                    column.Item().Row(row =>
                                    {
                                        row.ConstantItem(12).Text("•");
                                        row.RelativeItem().Text(bullet);
                                    });
                                }
                            }
                        });

                    page.Footer()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Extracts canonical sections from the rendered HTML content.
        /// </summary>
        private static IReadOnlyList<PdfSection> ExtractSections(string htmlContent)
        {
            var matches = Regex.Matches(
                htmlContent ?? string.Empty,
                "<section>(.*?)</section>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var sections = new List<PdfSection>();
            foreach (Match match in matches)
            {
                var body = match.Groups[1].Value;
                var heading = ReadFirstMatch(body, "<h2>(.*?)</h2>");
                var paragraphs = Regex.Matches(body, "<p>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(item => Normalize(item.Groups[1].Value))
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();

                var bullets = Regex.Matches(body, "<li>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(item => Normalize(item.Groups[1].Value))
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();

                if (string.IsNullOrWhiteSpace(heading))
                {
                    continue;
                }

                sections.Add(new PdfSection
                {
                    Heading = heading,
                    Paragraphs = paragraphs,
                    Bullets = bullets
                });
            }

            return sections;
        }

        /// <summary>
        /// Reads the first regex match group from the supplied HTML fragment.
        /// </summary>
        private static string ReadFirstMatch(string html, string pattern)
        {
            var match = Regex.Match(html ?? string.Empty, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? Normalize(match.Groups[1].Value) : null;
        }

        /// <summary>
        /// Converts simple HTML fragments to normalized plain text.
        /// </summary>
        private static string Normalize(string value)
        {
            var withoutTags = Regex.Replace(value ?? string.Empty, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(withoutTags).Trim();
        }

        /// <summary>
        /// Represents one extracted PDF report section.
        /// </summary>
        private class PdfSection
        {
            public string Heading { get; set; }

            public IReadOnlyList<string> Paragraphs { get; set; } = new List<string>();

            public IReadOnlyList<string> Bullets { get; set; } = new List<string>();
        }
    }
}
