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
using AstraLab.Services.AI;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;
using AstraLab.Services.Storage;
using AstraLab.Web.Core.Datasets.Storage;
using AstraLab.Web.Core.ML;
using AstraLab.Web.Core.ML.Storage;
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
            RegisterMlExecution();
            RegisterAiGeneration();
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

            Directory.CreateDirectory(resolvedRawRootPath);

            IocManager.IocContainer.Register(
                Component.For<DatasetStorageOptions>()
                    .Instance(new DatasetStorageOptions
                    {
                        DefaultProvider = _appConfiguration["DatasetStorage:DefaultProvider"] ?? LocalFileSystemRawDatasetStorage.ProviderName,
                        RawRootPath = resolvedRawRootPath
                    })
                    .LifestyleSingleton(),
                Component.For<ObjectStorageOptions>()
                    .Instance(new ObjectStorageOptions
                    {
                        ServiceUrl = _appConfiguration["ObjectStorage:ServiceUrl"],
                        Region = _appConfiguration["ObjectStorage:Region"],
                        AccessKey = _appConfiguration["ObjectStorage:AccessKey"],
                        SecretKey = _appConfiguration["ObjectStorage:SecretKey"],
                        ForcePathStyle = ReadBooleanSetting("ObjectStorage:ForcePathStyle", true),
                        DatasetBucketName = _appConfiguration["ObjectStorage:DatasetBucketName"],
                        DatasetKeyPrefix = _appConfiguration["ObjectStorage:DatasetKeyPrefix"] ?? "datasets",
                        MlArtifactBucketName = _appConfiguration["ObjectStorage:MlArtifactBucketName"],
                        MlArtifactKeyPrefix = _appConfiguration["ObjectStorage:MlArtifactKeyPrefix"] ?? "ml-artifacts",
                        PresignedUrlTtlSeconds = ReadIntegerSetting("ObjectStorage:PresignedUrlTtlSeconds", 900)
                    })
                    .LifestyleSingleton(),
                Component.For<IRawDatasetStorageProvider>()
                    .ImplementedBy<LocalFileSystemRawDatasetStorage>()
                    .LifestyleTransient(),
                Component.For<IRawDatasetStorageProvider>()
                    .ImplementedBy<S3CompatibleRawDatasetStorage>()
                    .LifestyleTransient(),
                Component.For<IRawDatasetStorage>()
                    .ImplementedBy<CompositeRawDatasetStorage>()
                    .LifestyleTransient());
        }

        private void RegisterMlExecution()
        {
            var configuredArtifactRootPath = _appConfiguration["MLExecution:ArtifactRootPath"];
            var resolvedArtifactRootPath = Path.IsPathRooted(configuredArtifactRootPath)
                ? configuredArtifactRootPath
                : Path.GetFullPath(Path.Combine(_env.ContentRootPath, configuredArtifactRootPath));

            Directory.CreateDirectory(resolvedArtifactRootPath);

            var executorBaseUrl = _appConfiguration["MLExecution:ExecutorBaseUrl"];
            var callbackBaseUrl = _appConfiguration["MLExecution:CallbackBaseUrl"];

            if (string.IsNullOrWhiteSpace(callbackBaseUrl))
            {
                callbackBaseUrl = _appConfiguration["App:ServerRootAddress"];
            }

            IocManager.IocContainer.Register(
                Component.For<MLExecutionOptions>()
                    .Instance(new MLExecutionOptions
                    {
                        ExecutorBaseUrl = executorBaseUrl,
                        CallbackBaseUrl = callbackBaseUrl,
                        SharedSecret = _appConfiguration["MLExecution:SharedSecret"],
                        DefaultArtifactStorageProvider = _appConfiguration["MLExecution:DefaultArtifactStorageProvider"] ?? LocalFileSystemMlArtifactStorage.ProviderName,
                        ArtifactRootPath = resolvedArtifactRootPath
                    })
                    .LifestyleSingleton(),
                Component.For<IMLArtifactStorageProvider>()
                    .ImplementedBy<LocalFileSystemMlArtifactStorage>()
                    .LifestyleTransient(),
                Component.For<IMLArtifactStorageProvider>()
                    .ImplementedBy<S3CompatibleMlArtifactStorage>()
                    .LifestyleTransient(),
                Component.For<IMLArtifactStorage>()
                    .ImplementedBy<CompositeMlArtifactStorage>()
                    .LifestyleTransient(),
                Component.For<IMLJobDispatcher>()
                    .ImplementedBy<MLHttpJobDispatcher>()
                    .LifestyleTransient());
        }

        private void RegisterAiGeneration()
        {
            IocManager.IocContainer.Register(
                Component.For<GroqAiOptions>()
                    .Instance(new GroqAiOptions
                    {
                        BaseUrl = _appConfiguration["AI:Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/responses",
                        ApiKey = _appConfiguration["AI:Groq:ApiKey"],
                        Model = _appConfiguration["AI:Groq:Model"],
                        TimeoutSeconds = ReadIntegerSetting("AI:Groq:TimeoutSeconds", 60),
                        MaxOutputTokens = ReadIntegerSetting("AI:Groq:MaxOutputTokens", 800),
                        ReasoningEffort = _appConfiguration["AI:Groq:ReasoningEffort"]
                    })
                    .LifestyleSingleton());
        }

        private bool ReadBooleanSetting(string key, bool defaultValue)
        {
            var configuredValue = _appConfiguration[key];
            return bool.TryParse(configuredValue, out var parsedValue)
                ? parsedValue
                : defaultValue;
        }

        private int ReadIntegerSetting(string key, int defaultValue)
        {
            var configuredValue = _appConfiguration[key];
            return int.TryParse(configuredValue, out var parsedValue)
                ? parsedValue
                : defaultValue;
        }
    }
}
