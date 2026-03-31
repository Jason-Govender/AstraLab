using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes aggregation transformations.
    /// </summary>
    public class AggregateDatasetTransformationStepExecutor : IDatasetTransformationStepExecutor, ITransientDependency
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType => DatasetTransformationType.Aggregate;

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        public DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson)
        {
            var configuration = DatasetTransformationJson.DeserializeConfiguration<AggregateConfiguration>(
                configurationJson,
                TransformationType);

            if (configuration?.Aggregations == null || configuration.Aggregations.Count == 0)
            {
                throw new UserFriendlyException("At least one aggregation is required for the Aggregate transformation.");
            }

            var groupByIndexes = ResolveGroupByIndexes(configuration.GroupByColumns, input);
            var resolvedAggregations = ResolveAggregations(configuration.Aggregations, input, groupByIndexes);
            var dataset = BuildAggregatedDataset(input, groupByIndexes, resolvedAggregations);

            return new DatasetTransformationStepExecutionResult
            {
                TransformationType = TransformationType,
                Dataset = dataset,
                CanonicalConfigurationJson = DatasetTransformationJson.Serialize(configuration),
                SummaryJson = DatasetTransformationJson.Serialize(new
                {
                    groupCount = dataset.Rows.Count,
                    resultingRowCount = dataset.Rows.Count
                })
            };
        }

        private static List<int> ResolveGroupByIndexes(IReadOnlyList<string> groupByColumns, TabularDataset input)
        {
            if (groupByColumns == null || groupByColumns.Count == 0)
            {
                return new List<int>();
            }

            return groupByColumns
                .Select(columnName =>
                {
                    var columnIndex = input.Columns.FindIndex(item => item.Name == columnName);
                    if (columnIndex < 0)
                    {
                        throw new UserFriendlyException($"Column '{columnName}' was not found for aggregation.");
                    }

                    return columnIndex;
                })
                .Distinct()
                .ToList();
        }

        private static List<ResolvedAggregation> ResolveAggregations(
            IReadOnlyList<AggregationDefinition> aggregations,
            TabularDataset input,
            IReadOnlyList<int> groupByIndexes)
        {
            var outputNames = new HashSet<string>(
                groupByIndexes.Select(index => input.Columns[index].Name),
                System.StringComparer.Ordinal);

            var resolvedAggregations = new List<ResolvedAggregation>();
            foreach (var aggregation in aggregations)
            {
                if (string.IsNullOrWhiteSpace(aggregation.OutputColumn))
                {
                    throw new UserFriendlyException("Each aggregation must define an outputColumn name.");
                }

                if (!outputNames.Add(aggregation.OutputColumn))
                {
                    throw new UserFriendlyException($"Aggregation output column '{aggregation.OutputColumn}' is duplicated.");
                }

                var resolvedAggregation = new ResolvedAggregation
                {
                    Function = aggregation.Function,
                    OutputColumn = aggregation.OutputColumn
                };

                if (aggregation.Function != AggregationFunction.Count)
                {
                    if (string.IsNullOrWhiteSpace(aggregation.Column))
                    {
                        throw new UserFriendlyException($"Aggregation '{aggregation.OutputColumn}' requires a source column.");
                    }

                    var columnIndex = input.Columns.FindIndex(item => item.Name == aggregation.Column);
                    if (columnIndex < 0)
                    {
                        throw new UserFriendlyException($"Column '{aggregation.Column}' was not found for aggregation.");
                    }

                    var sourceColumn = input.Columns[columnIndex];
                    var dataType = DatasetTransformationValueUtility.NormalizeDataTypeName(sourceColumn.DataType);
                    if (dataType != DatasetTransformationValueUtility.IntegerDataType &&
                        dataType != DatasetTransformationValueUtility.DecimalDataType)
                    {
                        throw new UserFriendlyException($"Column '{sourceColumn.Name}' must be numeric for the requested aggregation.");
                    }

                    resolvedAggregation.ColumnIndex = columnIndex;
                    resolvedAggregation.SourceDataType = dataType;
                }

                resolvedAggregations.Add(resolvedAggregation);
            }

            return resolvedAggregations;
        }

        private static TabularDataset BuildAggregatedDataset(
            TabularDataset input,
            IReadOnlyList<int> groupByIndexes,
            IReadOnlyList<ResolvedAggregation> aggregations)
        {
            var outputColumns = new List<TabularDatasetColumn>();

            foreach (var groupByIndex in groupByIndexes)
            {
                var sourceColumn = input.Columns[groupByIndex];
                outputColumns.Add(new TabularDatasetColumn
                {
                    Name = sourceColumn.Name,
                    Ordinal = outputColumns.Count + 1,
                    DataType = sourceColumn.DataType,
                    IsDataTypeInferred = sourceColumn.IsDataTypeInferred
                });
            }

            foreach (var aggregation in aggregations)
            {
                outputColumns.Add(new TabularDatasetColumn
                {
                    Name = aggregation.OutputColumn,
                    Ordinal = outputColumns.Count + 1,
                    DataType = GetAggregationDataType(aggregation),
                    IsDataTypeInferred = true
                });
            }

            var outputDataset = new TabularDataset
            {
                JsonRootKind = input.JsonRootKind,
                Columns = outputColumns
            };

            if (input.Rows.Count == 0)
            {
                return outputDataset;
            }

            var groupedRows = input.Rows
                .GroupBy(row => DatasetTransformationValueUtility.BuildRowKey(groupByIndexes.Select(index => row.Values[index])))
                .ToList();

            foreach (var groupedRow in groupedRows)
            {
                var firstRow = groupedRow.First();
                var values = new List<string>();

                foreach (var groupByIndex in groupByIndexes)
                {
                    values.Add(firstRow.Values[groupByIndex]);
                }

                foreach (var aggregation in aggregations)
                {
                    values.Add(ApplyAggregation(groupedRow, aggregation));
                }

                outputDataset.Rows.Add(new TabularDatasetRow
                {
                    Values = values
                });
            }

            return outputDataset;
        }

        private static string ApplyAggregation(IGrouping<string, TabularDatasetRow> groupedRows, ResolvedAggregation aggregation)
        {
            switch (aggregation.Function)
            {
                case AggregationFunction.Count:
                    return groupedRows.LongCount().ToString(CultureInfo.InvariantCulture);
            }

            var numericValues = groupedRows
                .Select(row => row.Values[aggregation.ColumnIndex])
                .Where(value => !DatasetTransformationValueUtility.IsMissing(value))
                .Select(value => DatasetTransformationValueUtility.ParseDecimal(value, aggregation.OutputColumn))
                .ToList();

            if (numericValues.Count == 0)
            {
                return null;
            }

            switch (aggregation.Function)
            {
                case AggregationFunction.Sum:
                    return numericValues.Sum().ToString(CultureInfo.InvariantCulture);
                case AggregationFunction.Average:
                    return numericValues.Average().ToString(CultureInfo.InvariantCulture);
                case AggregationFunction.Min:
                    return numericValues.Min().ToString(CultureInfo.InvariantCulture);
                case AggregationFunction.Max:
                    return numericValues.Max().ToString(CultureInfo.InvariantCulture);
                default:
                    throw new UserFriendlyException("The requested aggregation function is not supported.");
            }
        }

        private static string GetAggregationDataType(ResolvedAggregation aggregation)
        {
            switch (aggregation.Function)
            {
                case AggregationFunction.Count:
                    return DatasetTransformationValueUtility.IntegerDataType;
                case AggregationFunction.Average:
                    return DatasetTransformationValueUtility.DecimalDataType;
                default:
                    return aggregation.SourceDataType ?? DatasetTransformationValueUtility.DecimalDataType;
            }
        }

        private enum AggregationFunction
        {
            Count = 1,
            Sum = 2,
            Average = 3,
            Min = 4,
            Max = 5
        }

        private class AggregateConfiguration
        {
            public List<string> GroupByColumns { get; set; } = new List<string>();

            public List<AggregationDefinition> Aggregations { get; set; } = new List<AggregationDefinition>();
        }

        private class AggregationDefinition
        {
            public AggregationFunction Function { get; set; }

            public string Column { get; set; }

            public string OutputColumn { get; set; }
        }

        private class ResolvedAggregation
        {
            public AggregationFunction Function { get; set; }

            public int ColumnIndex { get; set; }

            public string OutputColumn { get; set; }

            public string SourceDataType { get; set; }
        }
    }
}
