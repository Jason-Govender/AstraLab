using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AstraLab.Services.Datasets.Dto;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Provides read-only dataset exploration workflows for dataset versions.
    /// </summary>
    public interface IDatasetExplorationAppService : IApplicationService
    {
        /// <summary>
        /// Gets a paged row view for the requested dataset version.
        /// </summary>
        Task<PagedResultDto<DatasetRowDto>> GetRowsAsync(PagedDatasetRowRequestDto input);

        /// <summary>
        /// Gets the chart configuration metadata for the requested dataset version.
        /// </summary>
        Task<DatasetExplorationColumnsDto> GetColumnsAsync(EntityDto<long> input);

        /// <summary>
        /// Gets histogram data for the requested numeric dataset column.
        /// </summary>
        Task<HistogramChartDto> GetHistogramAsync(GetHistogramChartRequest input);

        /// <summary>
        /// Gets bar chart data for the requested categorical dataset column.
        /// </summary>
        Task<BarChartDto> GetBarChartAsync(GetBarChartRequest input);

        /// <summary>
        /// Gets scatter plot data for the requested numeric dataset column pair.
        /// </summary>
        Task<ScatterPlotDto> GetScatterPlotAsync(GetScatterPlotRequest input);

        /// <summary>
        /// Gets a distribution analysis for the requested dataset column.
        /// </summary>
        Task<DistributionAnalysisDto> GetDistributionAsync(GetDistributionAnalysisRequest input);

        /// <summary>
        /// Gets correlation analysis data for the requested numeric dataset columns.
        /// </summary>
        Task<CorrelationAnalysisDto> GetCorrelationAsync(GetCorrelationAnalysisRequest input);
    }
}
