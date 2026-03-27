namespace AstraLab.Authorization.Accounts
{
    public class ResolvedTenantInfo
    {
        public static ResolvedTenantInfo Host { get; } = new ResolvedTenantInfo(null, null);

        public int? TenantId { get; }

        public string TenancyName { get; }

        public bool IsHost => !TenantId.HasValue;

        public ResolvedTenantInfo(int? tenantId, string tenancyName)
        {
            TenantId = tenantId;
            TenancyName = tenancyName;
        }
    }
}
