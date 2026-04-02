using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AstraLab.Authorization;
using QuestPDF.Infrastructure;

namespace AstraLab
{
    [DependsOn(
        typeof(AstraLabCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class AstraLabApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<AstraLabAuthorizationProvider>();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(AstraLabApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
