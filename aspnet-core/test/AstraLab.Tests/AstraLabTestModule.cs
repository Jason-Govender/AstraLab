using System;
using System.IO;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Modules;
using Abp.Configuration.Startup;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using AstraLab.EntityFrameworkCore;
using AstraLab.Services.Datasets.Storage;
using AstraLab.Services.ML;
using AstraLab.Services.ML.Storage;
using AstraLab.Services.Storage;
using AstraLab.Tests.DependencyInjection;
using AstraLab.Web.Core.Datasets.Storage;
using AstraLab.Web.Core.ML.Storage;

namespace AstraLab.Tests
{
    [DependsOn(
        typeof(AstraLabApplicationModule),
        typeof(AstraLabEntityFrameworkModule),
        typeof(AbpTestBaseModule)
        )]
    public class AstraLabTestModule : AbpModule
    {
        public AstraLabTestModule(AstraLabEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
            Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator<AstraLabDbContext>>();
            RegisterFakeService<IMLJobDispatcher>();

            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            RegisterDatasetStorage();
            RegisterMlExecutionOptions();
        }

        public override void Initialize()
        {
            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>() where TService : class
        {
            IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton()
            );
        }

        private void RegisterDatasetStorage()
        {
            var rawRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "RawStorage", Guid.NewGuid().ToString("N"));

            IocManager.IocContainer.Register(
                Component.For<DatasetStorageOptions>()
                    .Instance(new DatasetStorageOptions
                    {
                        DefaultProvider = LocalFileSystemRawDatasetStorage.ProviderName,
                        RawRootPath = rawRootPath
                    })
                    .LifestyleSingleton(),
                Component.For<ObjectStorageOptions>()
                    .Instance(new ObjectStorageOptions
                    {
                        ServiceUrl = "http://localhost:9000",
                        Region = "us-east-1",
                        AccessKey = "test-access-key",
                        SecretKey = "test-secret-key",
                        DatasetBucketName = "datasets",
                        MlArtifactBucketName = "ml-artifacts",
                        PresignedUrlTtlSeconds = 900
                    })
                    .LifestyleSingleton(),
                Component.For<IRawDatasetStorageProvider>()
                    .ImplementedBy<LocalFileSystemRawDatasetStorage>()
                    .LifestyleTransient(),
                Component.For<IRawDatasetStorage>()
                    .ImplementedBy<CompositeRawDatasetStorage>()
                    .LifestyleTransient());
        }

        private void RegisterMlExecutionOptions()
        {
            var artifactRootPath = Path.Combine(Path.GetTempPath(), "AstraLab.Tests", "MlArtifacts", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(artifactRootPath);

            IocManager.IocContainer.Register(
                Component.For<MLExecutionOptions>()
                    .Instance(new MLExecutionOptions
                    {
                        ExecutorBaseUrl = "http://localhost:8010",
                        CallbackBaseUrl = "http://localhost:44311",
                        SharedSecret = "test-ml-shared-secret",
                        DefaultArtifactStorageProvider = LocalFileSystemMlArtifactStorage.ProviderName,
                        ArtifactRootPath = artifactRootPath
                    })
                    .LifestyleSingleton(),
                Component.For<IMLArtifactStorageProvider>()
                    .ImplementedBy<LocalFileSystemMlArtifactStorage>()
                    .LifestyleTransient(),
                Component.For<IMLArtifactStorage>()
                    .ImplementedBy<CompositeMlArtifactStorage>()
                    .LifestyleTransient());
        }
    }
}
