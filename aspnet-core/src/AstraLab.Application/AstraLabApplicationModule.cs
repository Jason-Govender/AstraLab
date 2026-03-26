using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AstraLab.Authorization;

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
