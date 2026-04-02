using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AstraLab.Services.Analytics.Storage;
using AstraLab.Web.Core.Analytics.Storage;
using AstraLab.Web.Core.Storage;
using AstraLab.Services.Storage;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Analytics
{
    public class CompositeAnalyticsExportStorage_Tests
    {
        [Fact]
        public async Task StoreAsync_Should_Route_To_The_Default_Provider_When_None_Is_Specified()
        {
            var provider = CreateProvider(S3CompatibleAnalyticsExportStorage.ProviderName, canStore: true);
            provider.StoreAsync(Arg.Any<StoreAnalyticsExportRequest>())
                .Returns(new StoredAnalyticsExportResult
                {
                    StorageProvider = S3CompatibleAnalyticsExportStorage.ProviderName,
                    StorageKey = "analytics/report.pdf"
                });

            var storage = new CompositeAnalyticsExportStorage(new[] { provider });

            var result = await storage.StoreAsync(new StoreAnalyticsExportRequest
            {
                StorageKey = "analytics/report.pdf",
                Content = new MemoryStream(Encoding.UTF8.GetBytes("report"))
            });

            result.StorageProvider.ShouldBe(S3CompatibleAnalyticsExportStorage.ProviderName);
            await provider.Received(1).StoreAsync(Arg.Is<StoreAnalyticsExportRequest>(item => item.StorageKey == "analytics/report.pdf"));
        }

        [Fact]
        public async Task OpenReadAsync_Should_Route_To_The_Specified_Provider()
        {
            var provider = CreateProvider("custom-provider", canStore: true);
            provider.OpenReadAsync(Arg.Any<OpenReadAnalyticsExportRequest>())
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("payload")));

            var storage = new CompositeAnalyticsExportStorage(new[] { provider });

            using (var stream = await storage.OpenReadAsync(new OpenReadAnalyticsExportRequest
            {
                StorageProvider = "custom-provider",
                StorageKey = "analytics/payload.json"
            }))
            using (var reader = new StreamReader(stream))
            {
                (await reader.ReadToEndAsync()).ShouldBe("payload");
            }
        }

        [Fact]
        public async Task StoreAsync_Should_Reject_A_Provider_That_Cannot_Store()
        {
            var provider = CreateProvider("read-only", canStore: false);
            var storage = new CompositeAnalyticsExportStorage(new[] { provider });

            var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
                storage.StoreAsync(new StoreAnalyticsExportRequest
                {
                    StorageProvider = "read-only",
                    StorageKey = "analytics/export.txt",
                    Content = new MemoryStream(Encoding.UTF8.GetBytes("text"))
                }));

            exception.Message.ShouldContain("cannot accept new writes");
        }

        [Fact]
        public void BuildAnalyticsExportObjectKey_Should_Use_The_Configured_Prefix()
        {
            var result = S3CompatibleStoragePathBuilder.BuildAnalyticsExportObjectKey(new ObjectStorageOptions
            {
                AnalyticsExportKeyPrefix = "analytics-exports"
            }, "/reports/output.pdf");

            result.ShouldBe("analytics-exports/reports/output.pdf");
        }

        private static IAnalyticsExportStorageProvider CreateProvider(string providerName, bool canStore)
        {
            var provider = Substitute.For<IAnalyticsExportStorageProvider>();
            provider.ProviderName.Returns(providerName);
            provider.CanStore.Returns(canStore);
            return provider;
        }
    }
}
