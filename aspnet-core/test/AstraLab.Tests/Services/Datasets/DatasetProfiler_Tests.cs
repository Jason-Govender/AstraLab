using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Profiling;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.Datasets
{
    public class DatasetProfiler_Tests
    {
        private readonly IDatasetProfiler _datasetProfiler = new DatasetProfiler();

        [Fact]
        public async Task ProfileAsync_Should_Compute_Csv_Profile_Metrics()
        {
            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("id,amount,flag,name\n1,10.5,true,Alice\n2,,false,\n1,10.5,true,Alice\n")))
            {
                var result = await _datasetProfiler.ProfileAsync(new ProfileDatasetVersionRequest
                {
                    DatasetFormat = DatasetFormat.Csv,
                    Content = content,
                    Columns = new[]
                    {
                        new ProfileDatasetColumnRequest { DatasetColumnId = 1, Name = "id", Ordinal = 1 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 2, Name = "amount", Ordinal = 2 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 3, Name = "flag", Ordinal = 3 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 4, Name = "name", Ordinal = 4 }
                    }
                });

                result.RowCount.ShouldBe(3);
                result.DuplicateRowCount.ShouldBe(1);
                result.DataHealthScore.ShouldBeLessThan(100m);
                result.SummaryJson.ShouldContain("\"totalAnomalyCount\":0");
                result.SummaryJson.ShouldContain("\"overallAnomalyPercentage\":0");

                var amountColumn = result.Columns.Single(item => item.DatasetColumnId == 2);
                amountColumn.InferredDataType.ShouldBe("decimal");
                amountColumn.NullCount.ShouldBe(1);
                amountColumn.NullPercentage.ShouldBe(33.33m);
                amountColumn.DistinctCount.ShouldBe(1);
                amountColumn.StatisticsJson.ShouldContain("\"anomalyCount\":0");
                amountColumn.StatisticsJson.ShouldContain("\"hasAnomalies\":false");
                amountColumn.StatisticsJson.ShouldContain("\"mean\":10.5");
                amountColumn.StatisticsJson.ShouldContain("\"min\":10.5");
                amountColumn.StatisticsJson.ShouldContain("\"max\":10.5");

                var flagColumn = result.Columns.Single(item => item.DatasetColumnId == 3);
                flagColumn.InferredDataType.ShouldBe("boolean");
                flagColumn.NullCount.ShouldBe(0);

                var nameColumn = result.Columns.Single(item => item.DatasetColumnId == 4);
                nameColumn.InferredDataType.ShouldBe("string");
                nameColumn.NullCount.ShouldBe(1);
                nameColumn.StatisticsJson.ShouldContain("\"nullPercentage\":33.33");
            }
        }

        [Fact]
        public async Task ProfileAsync_Should_Compute_Json_Profile_Metrics()
        {
            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("[{\"id\":1,\"amount\":10.0,\"name\":\"Alice\",\"isActive\":true},{\"id\":2,\"amount\":null,\"name\":null,\"isActive\":false},{\"id\":1,\"amount\":10.0,\"name\":\"Alice\",\"isActive\":true}]")))
            {
                var result = await _datasetProfiler.ProfileAsync(new ProfileDatasetVersionRequest
                {
                    DatasetFormat = DatasetFormat.Json,
                    Content = content,
                    Columns = new[]
                    {
                        new ProfileDatasetColumnRequest { DatasetColumnId = 11, Name = "id", Ordinal = 1 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 12, Name = "amount", Ordinal = 2 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 13, Name = "name", Ordinal = 3 },
                        new ProfileDatasetColumnRequest { DatasetColumnId = 14, Name = "isActive", Ordinal = 4 }
                    }
                });

                result.RowCount.ShouldBe(3);
                result.DuplicateRowCount.ShouldBe(1);

                var idColumn = result.Columns.Single(item => item.DatasetColumnId == 11);
                idColumn.InferredDataType.ShouldBe("integer");

                var amountColumn = result.Columns.Single(item => item.DatasetColumnId == 12);
                amountColumn.InferredDataType.ShouldBe("decimal");
                amountColumn.NullCount.ShouldBe(1);
                amountColumn.StatisticsJson.ShouldContain("\"mean\":10.0");

                var nameColumn = result.Columns.Single(item => item.DatasetColumnId == 13);
                nameColumn.InferredDataType.ShouldBe("string");
                nameColumn.NullCount.ShouldBe(1);

                var activeColumn = result.Columns.Single(item => item.DatasetColumnId == 14);
                activeColumn.InferredDataType.ShouldBe("boolean");
                activeColumn.NullCount.ShouldBe(0);
                activeColumn.StatisticsJson.ShouldContain("\"anomalyCount\":0");
                activeColumn.StatisticsJson.ShouldContain("\"hasAnomalies\":false");
            }
        }

        [Fact]
        public async Task ProfileAsync_Should_Flag_Csv_Numeric_Outliers_And_Lower_Data_Health_Score()
        {
            var baselineContent = "value\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n";
            var outlierContent = "value\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n1\n100\n";

            var baselineResult = await ProfileSingleColumnCsvAsync(baselineContent);
            var outlierResult = await ProfileSingleColumnCsvAsync(outlierContent);

            var baselineColumn = baselineResult.Columns.Single();
            baselineColumn.StatisticsJson.ShouldContain("\"anomalyCount\":0");
            baselineColumn.StatisticsJson.ShouldContain("\"anomalyPercentage\":0");
            baselineColumn.StatisticsJson.ShouldContain("\"hasAnomalies\":false");

            outlierResult.SummaryJson.ShouldContain("\"totalAnomalyCount\":1");
            outlierResult.SummaryJson.ShouldContain("\"overallAnomalyPercentage\":4.76");
            outlierResult.DataHealthScore.ShouldBeLessThan(baselineResult.DataHealthScore);

            var outlierColumn = outlierResult.Columns.Single();
            outlierColumn.InferredDataType.ShouldBe("integer");
            outlierColumn.StatisticsJson.ShouldContain("\"anomalyCount\":1");
            outlierColumn.StatisticsJson.ShouldContain("\"anomalyPercentage\":4.76");
            outlierColumn.StatisticsJson.ShouldContain("\"hasAnomalies\":true");
        }

        [Fact]
        public async Task ProfileAsync_Should_Flag_Json_Numeric_Outliers()
        {
            var values = string.Join(",", Enumerable.Repeat("{\"amount\":1}", 20).Concat(new[] { "{\"amount\":100}" }));
            using (var content = new MemoryStream(Encoding.UTF8.GetBytes("[" + values + "]")))
            {
                var result = await _datasetProfiler.ProfileAsync(new ProfileDatasetVersionRequest
                {
                    DatasetFormat = DatasetFormat.Json,
                    Content = content,
                    Columns = new[]
                    {
                        new ProfileDatasetColumnRequest { DatasetColumnId = 21, Name = "amount", Ordinal = 1 }
                    }
                });

                result.RowCount.ShouldBe(21);
                result.SummaryJson.ShouldContain("\"totalAnomalyCount\":1");

                var amountColumn = result.Columns.Single();
                amountColumn.InferredDataType.ShouldBe("integer");
                amountColumn.StatisticsJson.ShouldContain("\"anomalyCount\":1");
                amountColumn.StatisticsJson.ShouldContain("\"hasAnomalies\":true");
            }
        }

        [Fact]
        public async Task ProfileAsync_Should_Not_Flag_Anomalies_For_Insufficient_Or_ZeroVariance_Numeric_Data()
        {
            var insufficientResult = await ProfileSingleColumnCsvAsync("value\n1\n1\n1\n100\n");
            insufficientResult.Columns.Single().StatisticsJson.ShouldContain("\"anomalyCount\":0");
            insufficientResult.Columns.Single().StatisticsJson.ShouldContain("\"hasAnomalies\":false");

            var zeroVarianceResult = await ProfileSingleColumnCsvAsync("value\n5\n5\n5\n5\n5\n5\n");
            zeroVarianceResult.Columns.Single().StatisticsJson.ShouldContain("\"anomalyCount\":0");
            zeroVarianceResult.Columns.Single().StatisticsJson.ShouldContain("\"hasAnomalies\":false");
        }

        private async Task<ProfileDatasetVersionResult> ProfileSingleColumnCsvAsync(string csvContent)
        {
            using (var content = new MemoryStream(Encoding.UTF8.GetBytes(csvContent)))
            {
                return await _datasetProfiler.ProfileAsync(new ProfileDatasetVersionRequest
                {
                    DatasetFormat = DatasetFormat.Csv,
                    Content = content,
                    Columns = new[]
                    {
                        new ProfileDatasetColumnRequest { DatasetColumnId = 100, Name = "value", Ordinal = 1 }
                    }
                });
            }
        }
    }
}
