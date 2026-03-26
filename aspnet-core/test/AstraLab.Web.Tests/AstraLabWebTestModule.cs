using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using AstraLab.EntityFrameworkCore;
using AstraLab.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace AstraLab.Web.Tests
{
    [DependsOn(
        typeof(AstraLabWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class AstraLabWebTestModule : AbpModule
    {
        public AstraLabWebTestModule(AstraLabEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AstraLabWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(AstraLabWebMvcModule).Assembly);
        }
    }
}