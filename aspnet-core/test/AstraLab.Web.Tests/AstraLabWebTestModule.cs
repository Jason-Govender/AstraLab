using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using AstraLab.EntityFrameworkCore;
using AstraLab.Web.Host.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AstraLab.Web.Tests
{
    [DependsOn(
        typeof(AstraLabWebHostModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class AstraLabWebTestModule : AbpModule
    {
        public AstraLabWebTestModule(AstraLabEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; // EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AstraLabWebTestModule).Assembly);
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(AstraLabWebHostModule).Assembly);
        }
    }
}
