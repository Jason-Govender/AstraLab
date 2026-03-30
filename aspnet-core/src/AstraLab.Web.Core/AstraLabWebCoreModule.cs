using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Castle.MicroKernel.Registration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.Configuration;
using AstraLab.Authentication.JwtBearer;
using AstraLab.Configuration;
using AstraLab.EntityFrameworkCore;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Web.Core.Datasets.Storage;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.IO;

namespace AstraLab
{
    [DependsOn(
         typeof(AstraLabApplicationModule),
         typeof(AstraLabEntityFrameworkModule),
         typeof(AbpAspNetCoreModule)
        ,typeof(AbpAspNetCoreSignalRModule)
     )]
    public class AstraLabWebCoreModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public AstraLabWebCoreModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                AstraLabConsts.ConnectionStringName
            );

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(AstraLabApplicationModule).GetAssembly()
                 );

            RegisterDatasetStorage();
            ConfigureTokenAuth();
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials = new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.Expiration = TimeSpan.FromDays(1);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AstraLabWebCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(AstraLabWebCoreModule).Assembly);
        }

        private void RegisterDatasetStorage()
        {
            var configuredRawRootPath = _appConfiguration["DatasetStorage:RawRootPath"];
            var resolvedRawRootPath = Path.IsPathRooted(configuredRawRootPath)
                ? configuredRawRootPath
                : Path.GetFullPath(Path.Combine(_env.ContentRootPath, configuredRawRootPath));

            IocManager.IocContainer.Register(
                Component.For<DatasetStorageOptions>()
                    .Instance(new DatasetStorageOptions
                    {
                        RawRootPath = resolvedRawRootPath
                    })
                    .LifestyleSingleton(),
                Component.For<IRawDatasetStorage>()
                    .ImplementedBy<LocalFileSystemRawDatasetStorage>()
                    .LifestyleTransient());
        }
    }
}
