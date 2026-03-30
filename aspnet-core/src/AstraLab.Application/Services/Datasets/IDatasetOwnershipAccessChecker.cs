using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets
{
    /// <summary>
    /// Resolves tenant-owned dataset resources while enforcing dataset owner access rules.
    /// </summary>
    public interface IDatasetOwnershipAccessChecker
    {
        /// <summary>
        /// Gets a dataset for the specified tenant owner.
        /// </summary>
        Task<Dataset> GetDatasetForOwnerAsync(long datasetId, int tenantId, long ownerUserId);

        /// <summary>
        /// Gets a dataset version for the specified tenant owner.
        /// </summary>
        Task<DatasetVersion> GetDatasetVersionForOwnerAsync(long datasetVersionId, int tenantId, long ownerUserId);

        /// <summary>
        /// Gets a dataset column for the specified tenant owner.
        /// </summary>
        Task<DatasetColumn> GetDatasetColumnForOwnerAsync(long datasetColumnId, int tenantId, long ownerUserId);
    }
}
