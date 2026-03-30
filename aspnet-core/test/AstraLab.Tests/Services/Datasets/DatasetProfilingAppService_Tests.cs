using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets;
using AstraLab.Services.Datasets.Dto;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetProfilingAppService_Tests : AstraLabTestBase
    {
        private readonly IDatasetIngestionAppService _datasetIngestionAppService;
        private readonly IDatasetProfilingAppService _datasetProfilingAppService;

        public DatasetProfilingAppService_Tests()
        {
            _datasetIngestionAppService = Resolve<IDatasetIngestionAppService>();
            _datasetProfilingAppService = Resolve<IDatasetProfilingAppService>();
        }

        [Fact]
        public async Task ProfileAsync_Should_Reprofile_DatasetVersion_And_Replace_Current_Profile_Snapshot()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Profile CSV",
                OriginalFileName = "profile.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,amount,name\n1,10.5,Alice\n2,,\n1,10.5,Alice\n")
            });

            var output = await _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(upload.DatasetVersionId));

            output.RowCount.ShouldBe(3);
            output.DuplicateRowCount.ShouldBe(1);
            output.ColumnProfiles.Count.ShouldBe(3);
            output.ColumnProfiles.Single(item => item.InferredDataType == "decimal").NullCount.ShouldBe(1);

            await UsingDbContextAsync(async context =>
            {
                context.DatasetProfiles.Count(item => item.DatasetVersionId == upload.DatasetVersionId).ShouldBe(1);
                context.DatasetColumnProfiles.Count().ShouldBe(3);

                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                var datasetVersion = context.DatasetVersions.Single(item => item.Id == upload.DatasetVersionId);
                var datasetColumns = context.DatasetColumns
                    .Where(item => item.DatasetVersionId == upload.DatasetVersionId)
                    .OrderBy(item => item.Ordinal)
                    .ToList();

                dataset.Status.ShouldBe(DatasetStatus.Ready);
                datasetVersion.Status.ShouldBe(DatasetVersionStatus.Active);
                datasetVersion.RowCount.ShouldBe(3);
                datasetColumns[1].DataType.ShouldBe("decimal");
                datasetColumns[1].NullCount.ShouldBe(1);

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ProfileAsync_Should_Hide_Dataset_Version_From_Other_Owner_In_Same_Tenant()
        {
            var upload = await _datasetIngestionAppService.UploadRawAsync(new UploadRawDatasetRequest
            {
                Name = "Owner CSV",
                OriginalFileName = "owner.csv",
                ContentType = "text/csv",
                Content = Encoding.UTF8.GetBytes("id,name\n1,Alice\n")
            });

            UsingDbContext(context =>
            {
                var dataset = context.Datasets.Single(item => item.Id == upload.Dataset.Id);
                dataset.OwnerUserId = dataset.OwnerUserId + 999;
                context.SaveChanges();
            });

            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(upload.DatasetVersionId)));
        }

        [Fact]
        public async Task Host_Context_Should_Be_Rejected_For_Dataset_Profiling_Operations()
        {
            LoginAsHostAdmin();

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                _datasetProfilingAppService.ProfileAsync(new EntityDto<long>(1)));

            exception.Message.ShouldBe("Tenant context is required for dataset profiling operations.");
        }
    }
}
