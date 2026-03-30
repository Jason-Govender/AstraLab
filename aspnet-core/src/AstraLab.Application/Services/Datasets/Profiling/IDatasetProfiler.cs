using System.Threading.Tasks;

namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Profiles raw dataset content and returns reusable statistics.
    /// </summary>
    public interface IDatasetProfiler
    {
        /// <summary>
        /// Profiles the supplied dataset content.
        /// </summary>
        Task<ProfileDatasetVersionResult> ProfileAsync(ProfileDatasetVersionRequest request);
    }
}
