using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AstraLab.Configuration;

namespace AstraLab.Web.Host.Startup
{
    [DependsOn(
       typeof(AstraLabWebCoreModule))]
    public class AstraLabWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AstraLabWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AstraLabWebHostModule).GetAssembly());
        }
    }
}
