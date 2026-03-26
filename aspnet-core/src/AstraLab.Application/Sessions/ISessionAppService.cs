using System.Threading.Tasks;
using Abp.Application.Services;
using AstraLab.Sessions.Dto;

namespace AstraLab.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
