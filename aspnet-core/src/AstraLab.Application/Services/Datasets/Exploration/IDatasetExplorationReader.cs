using System.Collections.Generic;
using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Exploration
{
    /// <summary>
    /// Loads persisted dataset-version content for read-only exploration workflows.
    /// </summary>
    public interface IDatasetExplorationReader
    {
        /// <summary>
        /// Gets the ordered persisted columns for an owner-scoped dataset version.
        /// </summary>
        Task<List<DatasetColumn>> GetColumnsAsync(long datasetVersionId, int tenantId, long ownerUserId);

        /// <summary>
        /// Loads and parses a tenant-owned dataset version into the shared tabular model.
        /// </summary>
        Task<LoadedExplorationDataset> LoadAsync(long datasetVersionId, int tenantId, long ownerUserId);
    }
}
