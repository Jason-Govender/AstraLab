using Abp.Application.Services;
using AstraLab.MultiTenancy.Dto;

namespace AstraLab.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

