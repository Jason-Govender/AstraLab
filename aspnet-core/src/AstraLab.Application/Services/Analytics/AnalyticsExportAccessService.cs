using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Services.Analytics.Storage;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.Services.Analytics
{
    /// <summary>
    /// Validates ownership and opens persisted analytics exports for download.
    /// </summary>
    public class AnalyticsExportAccessService : IAnalyticsExportAccessService, ITransientDependency
    {
        private readonly IRepository<AnalyticsExport, long> _analyticsExportRepository;
        private readonly IAnalyticsExportStorage _analyticsExportStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsExportAccessService"/> class.
        /// </summary>
        public AnalyticsExportAccessService(
            IRepository<AnalyticsExport, long> analyticsExportRepository,
            IAnalyticsExportStorage analyticsExportStorage)
        {
            _analyticsExportRepository = analyticsExportRepository;
            _analyticsExportStorage = analyticsExportStorage;
        }

        /// <summary>
        /// Opens a tenant-owned analytics export for download.
        /// </summary>
        [UnitOfWork]
        public async Task<AnalyticsExportDownloadResult> OpenDownloadAsync(long analyticsExportId, int tenantId, long ownerUserId)
        {
            var analyticsExport = await GetValidatedExportAsync(analyticsExportId, tenantId, ownerUserId);

            try
            {
                var contentStream = await _analyticsExportStorage.OpenReadAsync(new OpenReadAnalyticsExportRequest
                {
                    StorageProvider = analyticsExport.StorageProvider,
                    StorageKey = analyticsExport.StorageKey
                });

                return new AnalyticsExportDownloadResult
                {
                    Content = contentStream,
                    FileName = analyticsExport.DisplayName,
                    ContentType = analyticsExport.ContentType
                };
            }
            catch (FileNotFoundException)
            {
                throw new UserFriendlyException("The requested analytics export could not be found.");
            }
        }

        /// <summary>
        /// Gets a tenant-owned analytics export scoped to the current dataset owner.
        /// </summary>
        private async Task<AnalyticsExport> GetValidatedExportAsync(long analyticsExportId, int tenantId, long ownerUserId)
        {
            var analyticsExport = await _analyticsExportRepository.GetAll()
                .Where(item =>
                    item.Id == analyticsExportId &&
                    item.TenantId == tenantId &&
                    item.DatasetVersion.TenantId == tenantId &&
                    item.DatasetVersion.Dataset.OwnerUserId == ownerUserId)
                .SingleOrDefaultAsync();

            if (analyticsExport == null)
            {
                throw new UserFriendlyException("The requested analytics export could not be found.");
            }

            return analyticsExport;
        }
    }
}
