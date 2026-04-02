using System.Threading.Tasks;
using AstraLab.Services.Analytics.Dto;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Generates optional stakeholder-facing analytics narratives from aggregated deterministic summary data.
    /// </summary>
    public interface IAnalyticsNarrativeGenerator
    {
        /// <summary>
        /// Generates an optional narrative for the supplied deterministic analytics summary.
        /// </summary>
        Task<AnalyticsNarrativeDto> GenerateAsync(DatasetAnalyticsSummaryDto summary);
    }
}
