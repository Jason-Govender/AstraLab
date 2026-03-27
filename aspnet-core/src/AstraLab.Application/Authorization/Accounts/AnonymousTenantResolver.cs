using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Runtime.Session;
using Abp.UI;
using AstraLab.MultiTenancy;

namespace AstraLab.Authorization.Accounts
{
    public class AnonymousTenantResolver : AbpServiceBase, ITransientDependency
    {
        private readonly IAbpSession _abpSession;
        private readonly TenantManager _tenantManager;

        public AnonymousTenantResolver(
            IAbpSession abpSession,
            TenantManager tenantManager)
        {
            _abpSession = abpSession;
            _tenantManager = tenantManager;

            LocalizationSourceName = AstraLabConsts.LocalizationSourceName;
        }

        public async Task<ResolvedTenantInfo> ResolveAsync(string tenancyName, bool allowHost)
        {
            var ambientTenant = await GetAmbientTenantOrNullAsync();

            if (!string.IsNullOrWhiteSpace(tenancyName))
            {
                var requestedTenant = await GetActiveTenantByTenancyNameAsync(tenancyName);
                if (ambientTenant != null && ambientTenant.Id != requestedTenant.Id)
                {
                    throw new UserFriendlyException(L("TenantContextMismatch"));
                }

                return new ResolvedTenantInfo(requestedTenant.Id, requestedTenant.TenancyName);
            }

            if (ambientTenant != null)
            {
                return new ResolvedTenantInfo(ambientTenant.Id, ambientTenant.TenancyName);
            }

            if (allowHost)
            {
                return ResolvedTenantInfo.Host;
            }

            throw new UserFriendlyException(L("TenantIsRequiredForRegistration"));
        }

        private async Task<Tenant> GetAmbientTenantOrNullAsync()
        {
            if (!_abpSession.TenantId.HasValue)
            {
                return null;
            }

            return await GetActiveTenantByIdAsync(_abpSession.TenantId.Value);
        }

        private async Task<Tenant> GetActiveTenantByTenancyNameAsync(string tenancyName)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIsNotActive", tenancyName));
            }

            return tenant;
        }

        private async Task<Tenant> GetActiveTenantByIdAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }
    }
}
