using System.Threading.Tasks;
using Abp.Application.Services;
using AstraLab.Authorization.Accounts.Dto;

namespace AstraLab.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
