using System.Collections.Generic;
using Abp.Application.Services.Dto;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Transformations;

namespace AstraLab.Services.Datasets.Exploration
{
    /// <summary>
    /// Derives chart-ready and tabular exploration responses from loaded dataset content.
    /// </summary>
    public interface IDatasetExplorationAnalyzer
    {
        /// <summary>
        /// Builds a paged tabular row response.
        /// </summary>
        PagedResultDto<DatasetRowDto> BuildRows(long datasetVersionId, IReadOnlyList<DatasetColumn> columns, TabularDataset dataset, PagedDatasetRowRequestDto input);

        /// <summary>
        /// Builds chart configuration metadata for a dataset version.
        /// </summary>
        DatasetExplorationColumnsDto BuildColumns(long datasetVersionId, IReadOnlyList<DatasetColumn> columns);

        /// <summary>
        /// Builds histogram chart data.
        /// </summary>
        HistogramChartDto BuildHistogram(LoadedExplorationDataset dataset, GetHistogramChartRequest input);

        /// <summary>
        /// Builds bar chart data.
        /// </summary>
        BarChartDto BuildBarChart(LoadedExplorationDataset dataset, GetBarChartRequest input);

        /// <summary>
        /// Builds scatter plot data.
        /// </summary>
        ScatterPlotDto BuildScatterPlot(LoadedExplorationDataset dataset, GetScatterPlotRequest input);

        /// <summary>
        /// Builds a distribution analysis payload.
        /// </summary>
        DistributionAnalysisDto BuildDistribution(LoadedExplorationDataset dataset, GetDistributionAnalysisRequest input);

        /// <summary>
        /// Builds correlation analysis output.
        /// </summary>
        CorrelationAnalysisDto BuildCorrelation(LoadedExplorationDataset dataset, GetCorrelationAnalysisRequest input);
    }
}
