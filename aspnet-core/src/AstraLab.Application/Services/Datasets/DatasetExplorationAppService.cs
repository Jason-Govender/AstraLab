using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.Authorization;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Exploration;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Exposes read-only dataset exploration workflows for tabular viewing and charting.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Datasets)]
    public class DatasetExplorationAppService : AstraLabAppServiceBase, IDatasetExplorationAppService
    {
        private readonly IDatasetExplorationAnalyzer _datasetExplorationAnalyzer;
        private readonly IDatasetExplorationReader _datasetExplorationReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetExplorationAppService"/> class.
        /// </summary>
        public DatasetExplorationAppService(
            IDatasetExplorationAnalyzer datasetExplorationAnalyzer,
            IDatasetExplorationReader datasetExplorationReader)
        {
            _datasetExplorationAnalyzer = datasetExplorationAnalyzer;
            _datasetExplorationReader = datasetExplorationReader;
        }

        /// <summary>
        /// Gets a paged row view for the requested dataset version.
        /// </summary>
        public async Task<PagedResultDto<DatasetRowDto>> GetRowsAsync(PagedDatasetRowRequestDto input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildRows(explorationDataset.DatasetVersion.Id, explorationDataset.Columns, explorationDataset.Dataset, input);
        }

        /// <summary>
        /// Gets the chart configuration metadata for the requested dataset version.
        /// </summary>
        public async Task<DatasetExplorationColumnsDto> GetColumnsAsync(EntityDto<long> input)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            var columns = await _datasetExplorationReader.GetColumnsAsync(input.Id, tenantId, ownerUserId);
            return _datasetExplorationAnalyzer.BuildColumns(input.Id, columns);
        }

        /// <summary>
        /// Gets histogram data for the requested numeric dataset column.
        /// </summary>
        public async Task<HistogramChartDto> GetHistogramAsync(GetHistogramChartRequest input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildHistogram(explorationDataset, input);
        }

        /// <summary>
        /// Gets bar chart data for the requested categorical dataset column.
        /// </summary>
        public async Task<BarChartDto> GetBarChartAsync(GetBarChartRequest input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildBarChart(explorationDataset, input);
        }

        /// <summary>
        /// Gets scatter plot data for the requested numeric dataset column pair.
        /// </summary>
        public async Task<ScatterPlotDto> GetScatterPlotAsync(GetScatterPlotRequest input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildScatterPlot(explorationDataset, input);
        }

        /// <summary>
        /// Gets a distribution analysis for the requested dataset column.
        /// </summary>
        public async Task<DistributionAnalysisDto> GetDistributionAsync(GetDistributionAnalysisRequest input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildDistribution(explorationDataset, input);
        }

        /// <summary>
        /// Gets correlation analysis data for the requested numeric dataset columns.
        /// </summary>
        public async Task<CorrelationAnalysisDto> GetCorrelationAsync(GetCorrelationAnalysisRequest input)
        {
            var explorationDataset = await LoadDatasetAsync(input.DatasetVersionId);
            return _datasetExplorationAnalyzer.BuildCorrelation(explorationDataset, input);
        }

        private async Task<LoadedExplorationDataset> LoadDatasetAsync(long datasetVersionId)
        {
            var tenantId = GetRequiredTenantId();
            var ownerUserId = AbpSession.GetUserId();
            return await _datasetExplorationReader.LoadAsync(datasetVersionId, tenantId, ownerUserId);
        }

        private int GetRequiredTenantId()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new UserFriendlyException("Tenant context is required for dataset exploration operations.");
            }

            return AbpSession.TenantId.Value;
        }
    }
}
