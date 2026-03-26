using System.Threading.Tasks;
using AstraLab.Configuration.Dto;

namespace AstraLab.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
