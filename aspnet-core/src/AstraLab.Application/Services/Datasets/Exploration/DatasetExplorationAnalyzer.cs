using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using AstraLab.Services.Datasets.Transformations;

namespace AstraLab.Services.Datasets.Exploration
{
    /// <summary>
    /// Builds tabular and chart-ready exploration responses from loaded dataset content.
    /// </summary>
    public class DatasetExplorationAnalyzer : IDatasetExplorationAnalyzer, ITransientDependency
    {
        private const int DefaultPageSize = 10;
        private const int DefaultBucketCount = 10;
        private const int DefaultTopCategoryCount = 10;
        private const int DefaultScatterPointCount = 250;

        /// <summary>
        /// Builds a paged tabular row response.
        /// </summary>
        public PagedResultDto<DatasetRowDto> BuildRows(long datasetVersionId, IReadOnlyList<DatasetColumn> columns, TabularDataset dataset, PagedDatasetRowRequestDto input)
        {
            ValidateRowRequest(datasetVersionId, input);

            var skipCount = Math.Max(0, input.SkipCount);
            var maxResultCount = input.MaxResultCount > 0 ? input.MaxResultCount : DefaultPageSize;
            var items = dataset.Rows
                .Skip(skipCount)
                .Take(maxResultCount)
                .Select((row, index) => new DatasetRowDto
                {
                    RowNumber = skipCount + index + 1,
                    Values = row.Values.ToList()
                })
                .ToList();

            return new PagedResultDto<DatasetRowDto>(dataset.Rows.Count, items);
        }

        /// <summary>
        /// Builds chart configuration metadata for a dataset version.
        /// </summary>
        public DatasetExplorationColumnsDto BuildColumns(long datasetVersionId, IReadOnlyList<DatasetColumn> columns)
        {
            return new DatasetExplorationColumnsDto
            {
                DatasetVersionId = datasetVersionId,
                Columns = columns
                    .OrderBy(item => item.Ordinal)
                    .Select(item =>
                    {
                        var normalizedType = NormalizeDataType(item);
                        return new DatasetExplorationColumnDto
                        {
                            DatasetColumnId = item.Id,
                            Name = item.Name,
                            Ordinal = item.Ordinal,
                            DataType = normalizedType,
                            IsNumeric = IsNumeric(normalizedType),
                            IsCategorical = IsCategorical(normalizedType),
                            IsTemporal = IsTemporal(normalizedType)
                        };
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Builds histogram chart data.
        /// </summary>
        public HistogramChartDto BuildHistogram(LoadedExplorationDataset dataset, GetHistogramChartRequest input)
        {
            ValidateHistogramRequest(input);
            var column = ResolveColumn(dataset, input.DatasetColumnId);
            EnsureNumericColumn(column, "Histograms are only supported for numeric columns.");

            var values = ReadNumericValues(dataset.Dataset, column.Ordinal - 1, column.Name);
            var histogram = BuildHistogramDto(dataset.DatasetVersion.Id, column.Id, column.Name, values, input.BucketCount ?? DefaultBucketCount);
            histogram.NullCount = dataset.Dataset.Rows.LongCount(row => DatasetTransformationValueUtility.IsMissing(row.Values[column.Ordinal - 1]));
            return histogram;
        }

        /// <summary>
        /// Builds bar chart data.
        /// </summary>
        public BarChartDto BuildBarChart(LoadedExplorationDataset dataset, GetBarChartRequest input)
        {
            ValidateBarChartRequest(input);
            var column = ResolveColumn(dataset, input.DatasetColumnId);
            if (!IsCategorical(NormalizeDataType(column)))
            {
                throw new UserFriendlyException("Bar charts are only supported for categorical columns.");
            }

            var categoryResults = BuildCategoryFrequencies(dataset, column, input.TopCategoryCount.GetValueOrDefault(DefaultTopCategoryCount));
            return new BarChartDto
            {
                DatasetVersionId = dataset.DatasetVersion.Id,
                DatasetColumnId = column.Id,
                ColumnName = column.Name,
                DistinctCategoryCount = categoryResults.TotalDistinctCount,
                NullCount = categoryResults.NullCount,
                Categories = categoryResults.Items
            };
        }

        /// <summary>
        /// Builds scatter plot data.
        /// </summary>
        public ScatterPlotDto BuildScatterPlot(LoadedExplorationDataset dataset, GetScatterPlotRequest input)
        {
            ValidateScatterRequest(input);
            var xColumn = ResolveColumn(dataset, input.XDatasetColumnId);
            var yColumn = ResolveColumn(dataset, input.YDatasetColumnId);

            EnsureNumericColumn(xColumn, "Scatter plots require two numeric columns.");
            EnsureNumericColumn(yColumn, "Scatter plots require two numeric columns.");

            var points = dataset.Dataset.Rows
                .Select((row, index) => new { Row = row, RowNumber = index + 1 })
                .Where(item =>
                    !DatasetTransformationValueUtility.IsMissing(item.Row.Values[xColumn.Ordinal - 1]) &&
                    !DatasetTransformationValueUtility.IsMissing(item.Row.Values[yColumn.Ordinal - 1]))
                .Select(item => new ScatterPlotPointDto
                {
                    RowNumber = item.RowNumber,
                    X = DatasetTransformationValueUtility.ParseDecimal(item.Row.Values[xColumn.Ordinal - 1], xColumn.Name),
                    Y = DatasetTransformationValueUtility.ParseDecimal(item.Row.Values[yColumn.Ordinal - 1], yColumn.Name)
                })
                .Take(input.MaxPointCount.GetValueOrDefault(DefaultScatterPointCount))
                .ToList();

            return new ScatterPlotDto
            {
                DatasetVersionId = dataset.DatasetVersion.Id,
                XDatasetColumnId = xColumn.Id,
                YDatasetColumnId = yColumn.Id,
                XColumnName = xColumn.Name,
                YColumnName = yColumn.Name,
                PointCount = points.Count,
                Points = points
            };
        }

        /// <summary>
        /// Builds a distribution analysis payload.
        /// </summary>
        public DistributionAnalysisDto BuildDistribution(LoadedExplorationDataset dataset, GetDistributionAnalysisRequest input)
        {
            ValidateDistributionRequest(input);
            var column = ResolveColumn(dataset, input.DatasetColumnId);
            var normalizedType = NormalizeDataType(column);
            var nullCount = dataset.Dataset.Rows.LongCount(row => DatasetTransformationValueUtility.IsMissing(row.Values[column.Ordinal - 1]));

            if (IsNumeric(normalizedType))
            {
                var values = ReadNumericValues(dataset.Dataset, column.Ordinal - 1, column.Name);
                var histogram = BuildHistogramDto(dataset.DatasetVersion.Id, column.Id, column.Name, values, input.BucketCount ?? DefaultBucketCount);
                return new DistributionAnalysisDto
                {
                    DatasetVersionId = dataset.DatasetVersion.Id,
                    DatasetColumnId = column.Id,
                    ColumnName = column.Name,
                    DataType = normalizedType,
                    ValueCount = values.Count,
                    NullCount = nullCount,
                    Mean = values.Count == 0 ? null : values.Average(),
                    Min = values.Count == 0 ? null : values.Min(),
                    Max = values.Count == 0 ? null : values.Max(),
                    Median = values.Count == 0 ? null : CalculateMedian(values),
                    Buckets = histogram.Buckets
                };
            }

            if (!IsCategorical(normalizedType))
            {
                throw new UserFriendlyException("Distribution analysis is only supported for numeric and categorical columns.");
            }

            var categoryResults = BuildCategoryFrequencies(dataset, column, input.TopCategoryCount.GetValueOrDefault(DefaultTopCategoryCount));
            return new DistributionAnalysisDto
            {
                DatasetVersionId = dataset.DatasetVersion.Id,
                DatasetColumnId = column.Id,
                ColumnName = column.Name,
                DataType = normalizedType,
                ValueCount = categoryResults.TotalValueCount,
                NullCount = categoryResults.NullCount,
                DistinctCount = categoryResults.TotalDistinctCount,
                Categories = categoryResults.Items
            };
        }

        /// <summary>
        /// Builds correlation analysis output.
        /// </summary>
        public CorrelationAnalysisDto BuildCorrelation(LoadedExplorationDataset dataset, GetCorrelationAnalysisRequest input)
        {
            ValidateCorrelationRequest(input);

            var selectedColumns = input.DatasetColumnIds
                .Distinct()
                .Select(columnId => ResolveColumn(dataset, columnId))
                .ToList();

            if (selectedColumns.Count < 2)
            {
                throw new UserFriendlyException("At least two numeric columns are required for correlation analysis.");
            }

            if (selectedColumns.Any(item => !IsNumeric(NormalizeDataType(item))))
            {
                throw new UserFriendlyException("Correlation analysis is only supported for numeric columns.");
            }

            var pairs = new List<CorrelationPairDto>();
            for (var i = 0; i < selectedColumns.Count; i++)
            {
                for (var j = i + 1; j < selectedColumns.Count; j++)
                {
                    var xColumn = selectedColumns[i];
                    var yColumn = selectedColumns[j];
                    var sample = dataset.Dataset.Rows
                        .Select(row => new
                        {
                            X = row.Values[xColumn.Ordinal - 1],
                            Y = row.Values[yColumn.Ordinal - 1]
                        })
                        .Where(item =>
                            !DatasetTransformationValueUtility.IsMissing(item.X) &&
                            !DatasetTransformationValueUtility.IsMissing(item.Y))
                        .Select(item => new
                        {
                            X = (double)DatasetTransformationValueUtility.ParseDecimal(item.X, xColumn.Name),
                            Y = (double)DatasetTransformationValueUtility.ParseDecimal(item.Y, yColumn.Name)
                        })
                        .ToList();

                    pairs.Add(new CorrelationPairDto
                    {
                        XDatasetColumnId = xColumn.Id,
                        YDatasetColumnId = yColumn.Id,
                        XColumnName = xColumn.Name,
                        YColumnName = yColumn.Name,
                        SampleSize = sample.Count,
                        Coefficient = CalculatePearsonCorrelation(sample.Select(item => item.X).ToList(), sample.Select(item => item.Y).ToList())
                    });
                }
            }

            return new CorrelationAnalysisDto
            {
                DatasetVersionId = dataset.DatasetVersion.Id,
                Columns = selectedColumns.Select(item => new CorrelationColumnDto
                {
                    DatasetColumnId = item.Id,
                    Name = item.Name
                }).ToList(),
                Pairs = pairs
            };
        }

        private static void ValidateRowRequest(long datasetVersionId, PagedDatasetRowRequestDto input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.DatasetVersionId != datasetVersionId || input.DatasetVersionId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version is required for dataset row exploration.");
            }
        }

        private static void ValidateHistogramRequest(GetHistogramChartRequest input)
        {
            if (input == null || input.DatasetVersionId <= 0 || input.DatasetColumnId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version and column are required for histogram generation.");
            }
        }

        private static void ValidateBarChartRequest(GetBarChartRequest input)
        {
            if (input == null || input.DatasetVersionId <= 0 || input.DatasetColumnId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version and column are required for bar chart generation.");
            }
        }

        private static void ValidateScatterRequest(GetScatterPlotRequest input)
        {
            if (input == null || input.DatasetVersionId <= 0 || input.XDatasetColumnId <= 0 || input.YDatasetColumnId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version and numeric column pair are required for scatter plot generation.");
            }
        }

        private static void ValidateDistributionRequest(GetDistributionAnalysisRequest input)
        {
            if (input == null || input.DatasetVersionId <= 0 || input.DatasetColumnId <= 0)
            {
                throw new UserFriendlyException("A valid dataset version and column are required for distribution analysis.");
            }
        }

        private static void ValidateCorrelationRequest(GetCorrelationAnalysisRequest input)
        {
            if (input == null || input.DatasetVersionId <= 0 || input.DatasetColumnIds == null || input.DatasetColumnIds.Count == 0)
            {
                throw new UserFriendlyException("At least two numeric columns are required for correlation analysis.");
            }
        }

        private static DatasetColumn ResolveColumn(LoadedExplorationDataset dataset, long datasetColumnId)
        {
            var column = dataset.Columns.SingleOrDefault(item => item.Id == datasetColumnId);
            if (column == null)
            {
                throw new UserFriendlyException("The requested column does not belong to the selected dataset version.");
            }

            return column;
        }

        private static string NormalizeDataType(DatasetColumn column)
        {
            return DatasetTransformationValueUtility.NormalizeDataTypeName(column.DataType);
        }

        private static void EnsureNumericColumn(DatasetColumn column, string message)
        {
            if (!IsNumeric(NormalizeDataType(column)))
            {
                throw new UserFriendlyException(message);
            }
        }

        private static bool IsNumeric(string normalizedType)
        {
            return string.Equals(normalizedType, DatasetTransformationValueUtility.IntegerDataType, StringComparison.Ordinal) ||
                   string.Equals(normalizedType, DatasetTransformationValueUtility.DecimalDataType, StringComparison.Ordinal);
        }

        private static bool IsCategorical(string normalizedType)
        {
            return string.Equals(normalizedType, DatasetTransformationValueUtility.StringDataType, StringComparison.Ordinal) ||
                   string.Equals(normalizedType, DatasetTransformationValueUtility.BooleanDataType, StringComparison.Ordinal);
        }

        private static bool IsTemporal(string normalizedType)
        {
            return string.Equals(normalizedType, DatasetTransformationValueUtility.DateTimeDataType, StringComparison.Ordinal);
        }

        private static List<decimal> ReadNumericValues(TabularDataset dataset, int columnIndex, string columnName)
        {
            return dataset.Rows
                .Select(row => row.Values[columnIndex])
                .Where(value => !DatasetTransformationValueUtility.IsMissing(value))
                .Select(value => DatasetTransformationValueUtility.ParseDecimal(value, columnName))
                .ToList();
        }

        private static HistogramChartDto BuildHistogramDto(long datasetVersionId, long datasetColumnId, string columnName, IReadOnlyList<decimal> values, int requestedBucketCount)
        {
            var bucketCount = requestedBucketCount > 0 ? requestedBucketCount : DefaultBucketCount;
            if (values.Count == 0)
            {
                return new HistogramChartDto
                {
                    DatasetVersionId = datasetVersionId,
                    DatasetColumnId = datasetColumnId,
                    ColumnName = columnName,
                    BucketCount = bucketCount,
                    ValueCount = 0
                };
            }

            var min = values.Min();
            var max = values.Max();
            return new HistogramChartDto
            {
                DatasetVersionId = datasetVersionId,
                DatasetColumnId = datasetColumnId,
                ColumnName = columnName,
                BucketCount = bucketCount,
                ValueCount = values.Count,
                Min = min,
                Max = max,
                Buckets = BuildHistogramBuckets(values, bucketCount, min, max)
            };
        }

        private static List<HistogramBucketDto> BuildHistogramBuckets(IReadOnlyList<decimal> values, int bucketCount, decimal min, decimal max)
        {
            if (min == max)
            {
                return new List<HistogramBucketDto>
                {
                    new HistogramBucketDto
                    {
                        Label = min.ToString(CultureInfo.InvariantCulture),
                        Start = min,
                        End = max,
                        Count = values.Count
                    }
                };
            }

            var bucketSize = (max - min) / bucketCount;
            var buckets = Enumerable.Range(0, bucketCount)
                .Select(index =>
                {
                    var start = min + (bucketSize * index);
                    var end = index == bucketCount - 1 ? max : start + bucketSize;
                    return new HistogramBucketDto
                    {
                        Label = $"{decimal.Round(start, 4)} - {decimal.Round(end, 4)}",
                        Start = decimal.Round(start, 4),
                        End = decimal.Round(end, 4),
                        Count = 0
                    };
                })
                .ToList();

            foreach (var value in values)
            {
                var rawIndex = (int)((value - min) / bucketSize);
                var bucketIndex = rawIndex >= bucketCount ? bucketCount - 1 : rawIndex;
                buckets[bucketIndex].Count++;
            }

            return buckets;
        }

        private static decimal? CalculateMedian(IReadOnlyList<decimal> values)
        {
            if (values.Count == 0)
            {
                return null;
            }

            var ordered = values.OrderBy(item => item).ToList();
            var midpoint = ordered.Count / 2;
            return ordered.Count % 2 == 1 ? ordered[midpoint] : (ordered[midpoint - 1] + ordered[midpoint]) / 2m;
        }

        private static decimal? CalculatePearsonCorrelation(IReadOnlyList<double> xValues, IReadOnlyList<double> yValues)
        {
            if (xValues.Count < 2 || yValues.Count < 2 || xValues.Count != yValues.Count)
            {
                return null;
            }

            var xAverage = xValues.Average();
            var yAverage = yValues.Average();
            var numerator = 0d;
            var xVariance = 0d;
            var yVariance = 0d;

            for (var index = 0; index < xValues.Count; index++)
            {
                var xDelta = xValues[index] - xAverage;
                var yDelta = yValues[index] - yAverage;
                numerator += xDelta * yDelta;
                xVariance += xDelta * xDelta;
                yVariance += yDelta * yDelta;
            }

            if (xVariance == 0d || yVariance == 0d)
            {
                return null;
            }

            return decimal.Round((decimal)(numerator / Math.Sqrt(xVariance * yVariance)), 4);
        }

        private static CategoryFrequencyResult BuildCategoryFrequencies(LoadedExplorationDataset dataset, DatasetColumn column, int topCategoryCount)
        {
            var nullCount = dataset.Dataset.Rows.LongCount(row => DatasetTransformationValueUtility.IsMissing(row.Values[column.Ordinal - 1]));
            var frequencies = dataset.Dataset.Rows
                .Select(row => row.Values[column.Ordinal - 1])
                .Where(value => !DatasetTransformationValueUtility.IsMissing(value))
                .GroupBy(value => value, StringComparer.Ordinal)
                .Select(group => new BarChartCategoryDto
                {
                    Label = group.Key,
                    Count = group.LongCount()
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.Label, StringComparer.Ordinal)
                .ToList();

            return new CategoryFrequencyResult
            {
                NullCount = nullCount,
                TotalValueCount = frequencies.Sum(item => item.Count),
                TotalDistinctCount = frequencies.Count,
                Items = frequencies.Take(topCategoryCount).ToList()
            };
        }

        /// <summary>
        /// Represents derived categorical frequency results.
        /// </summary>
        private class CategoryFrequencyResult
        {
            public long NullCount { get; set; }

            public long TotalValueCount { get; set; }

            public long TotalDistinctCount { get; set; }

            public List<BarChartCategoryDto> Items { get; set; } = new List<BarChartCategoryDto>();
        }
    }
}
