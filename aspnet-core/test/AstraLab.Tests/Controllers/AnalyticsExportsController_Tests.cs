using System.IO;
using System.Text;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using AstraLab.Controllers;
using AstraLab.Core.Domains.Analytics;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Analytics;
using AstraLab.Services.Analytics.Storage;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Controllers
{
    public class AnalyticsExportsController_Tests : AstraLabTestBase
    {
        private readonly IAnalyticsExportStorage _analyticsExportStorage;
        private readonly IAbpSession _abpSession;

        public AnalyticsExportsController_Tests()
        {
            _analyticsExportStorage = Substitute.For<IAnalyticsExportStorage>();
            LocalIocManager.IocContainer.Register(
                Component.For<IAnalyticsExportStorage>()
                    .Instance(_analyticsExportStorage)
                    .IsDefault()
                    .LifestyleSingleton());

            _abpSession = Resolve<IAbpSession>();
        }

        [Fact]
        public async Task DownloadAsync_Should_Return_File_For_A_Tenant_Owned_Export()
        {
            var exportId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "analytics-download",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.UserId.Value,
                    OriginalFileName = "analytics-download.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                context.SaveChanges();

                var analyticsExport = context.AnalyticsExports.Add(new AnalyticsExport
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    ExportType = AnalyticsExportType.Document,
                    DisplayName = "stakeholder-report.pdf",
                    StorageProvider = "s3-compatible",
                    StorageKey = "analytics/stakeholder-report.pdf",
                    ContentType = "application/pdf"
                }).Entity;

                context.SaveChanges();
                return analyticsExport.Id;
            });

            _analyticsExportStorage.OpenReadAsync(Arg.Any<OpenReadAnalyticsExportRequest>())
                .Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("pdf-content"))));

            var controller = new AnalyticsExportsController(Resolve<IAnalyticsExportAccessService>(), _abpSession);

            var result = await controller.DownloadAsync(exportId);

            var fileResult = result.ShouldBeOfType<FileStreamResult>();
            fileResult.ContentType.ShouldBe("application/pdf");
            fileResult.FileDownloadName.ShouldBe("stakeholder-report.pdf");

            using (var reader = new StreamReader(fileResult.FileStream, Encoding.UTF8, false, 1024, true))
            {
                (await reader.ReadToEndAsync()).ShouldBe("pdf-content");
            }
        }

        [Fact]
        public async Task DownloadAsync_Should_Return_NotFound_For_A_Foreign_Export()
        {
            var exportId = UsingDbContext(context =>
            {
                var dataset = context.Datasets.Add(new Dataset
                {
                    TenantId = 1,
                    Name = "analytics-foreign-download",
                    SourceFormat = DatasetFormat.Csv,
                    Status = DatasetStatus.Ready,
                    OwnerUserId = AbpSession.UserId.Value + 10,
                    OriginalFileName = "foreign.csv"
                }).Entity;

                context.SaveChanges();

                var datasetVersion = context.DatasetVersions.Add(new DatasetVersion
                {
                    TenantId = 1,
                    DatasetId = dataset.Id,
                    VersionNumber = 1,
                    VersionType = DatasetVersionType.Raw,
                    Status = DatasetVersionStatus.Active,
                    SizeBytes = 512
                }).Entity;

                context.SaveChanges();

                var analyticsExport = context.AnalyticsExports.Add(new AnalyticsExport
                {
                    TenantId = 1,
                    DatasetVersionId = datasetVersion.Id,
                    ExportType = AnalyticsExportType.Document,
                    DisplayName = "foreign-report.pdf",
                    StorageProvider = "s3-compatible",
                    StorageKey = "analytics/foreign-report.pdf",
                    ContentType = "application/pdf"
                }).Entity;

                context.SaveChanges();
                return analyticsExport.Id;
            });

            var controller = new AnalyticsExportsController(Resolve<IAnalyticsExportAccessService>(), _abpSession);

            var result = await controller.DownloadAsync(exportId);

            result.ShouldBeOfType<NotFoundObjectResult>()
                .Value
                .ToString()
                .ShouldContain("could not be found");
        }
    }
}
