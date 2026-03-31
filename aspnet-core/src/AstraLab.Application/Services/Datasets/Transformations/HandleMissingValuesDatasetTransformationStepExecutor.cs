using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes missing-value handling transformations.
    /// </summary>
    public class HandleMissingValuesDatasetTransformationStepExecutor : IDatasetTransformationStepExecutor, ITransientDependency
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType => DatasetTransformationType.HandleMissingValues;

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        public DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson)
        {
            var configuration = DatasetTransformationJson.DeserializeConfiguration<HandleMissingValuesConfiguration>(
                configurationJson,
                TransformationType);

            if (configuration?.Columns == null || configuration.Columns.Count == 0)
            {
                throw new UserFriendlyException("At least one column is required for missing value handling.");
            }

            var dataset = input.Clone();
            var selectedColumns = ResolveColumnIndexes(configuration.Columns, dataset);

            switch (configuration.Strategy)
            {
                case MissingValueStrategy.DropRows:
                    return DropRows(dataset, configuration, selectedColumns);
                case MissingValueStrategy.FillConstant:
                    return FillConstant(dataset, configuration, selectedColumns);
                case MissingValueStrategy.FillMean:
                    return FillMean(dataset, configuration, selectedColumns);
                case MissingValueStrategy.FillMode:
                    return FillMode(dataset, configuration, selectedColumns);
                default:
                    throw new UserFriendlyException("The missing value handling strategy is not supported.");
            }
        }

        private static DatasetTransformationStepExecutionResult DropRows(
            TabularDataset dataset,
            HandleMissingValuesConfiguration configuration,
            IReadOnlyList<int> selectedColumns)
        {
            var originalRowCount = dataset.Rows.Count;
            dataset.Rows = dataset.Rows
                .Where(row => selectedColumns.All(index => !DatasetTransformationValueUtility.IsMissing(row.Values[index])))
                .Select(row => row.Clone())
                .ToList();

            return BuildResult(dataset, configuration, new
            {
                strategy = configuration.Strategy,
                droppedRowCount = originalRowCount - dataset.Rows.Count,
                remainingRowCount = dataset.Rows.Count
            });
        }

        private static DatasetTransformationStepExecutionResult FillConstant(
            TabularDataset dataset,
            HandleMissingValuesConfiguration configuration,
            IReadOnlyList<int> selectedColumns)
        {
            if (string.IsNullOrWhiteSpace(configuration.ConstantValue))
            {
                throw new UserFriendlyException("A constant value is required for the FillConstant missing value strategy.");
            }

            var filledCellCount = 0;
            foreach (var columnIndex in selectedColumns)
            {
                var column = dataset.Columns[columnIndex];
                var normalizedConstant = DatasetTransformationValueUtility.NormalizeValueForDataType(
                    configuration.ConstantValue,
                    column.DataType,
                    column.Name);

                foreach (var row in dataset.Rows)
                {
                    if (!DatasetTransformationValueUtility.IsMissing(row.Values[columnIndex]))
                    {
                        continue;
                    }

                    row.Values[columnIndex] = normalizedConstant;
                    filledCellCount++;
                }
            }

            return BuildResult(dataset, configuration, new
            {
                strategy = configuration.Strategy,
                filledCellCount
            });
        }

        private static DatasetTransformationStepExecutionResult FillMean(
            TabularDataset dataset,
            HandleMissingValuesConfiguration configuration,
            IReadOnlyList<int> selectedColumns)
        {
            var filledCellCount = 0;
            foreach (var columnIndex in selectedColumns)
            {
                var column = dataset.Columns[columnIndex];
                var columnDataType = DatasetTransformationValueUtility.NormalizeDataTypeName(column.DataType);
                if (columnDataType != DatasetTransformationValueUtility.IntegerDataType &&
                    columnDataType != DatasetTransformationValueUtility.DecimalDataType)
                {
                    throw new UserFriendlyException($"Column '{column.Name}' must be numeric to use the FillMean missing value strategy.");
                }

                var numericValues = dataset.Rows
                    .Select(row => row.Values[columnIndex])
                    .Where(value => !DatasetTransformationValueUtility.IsMissing(value))
                    .Select(value => DatasetTransformationValueUtility.ParseDecimal(value, column.Name))
                    .ToList();

                if (numericValues.Count == 0)
                {
                    throw new UserFriendlyException($"Column '{column.Name}' does not contain any non-missing numeric values for mean imputation.");
                }

                var mean = numericValues.Average();
                var normalizedMean = mean.ToString(CultureInfo.InvariantCulture);
                column.DataType = DatasetTransformationValueUtility.DecimalDataType;

                foreach (var row in dataset.Rows)
                {
                    if (!DatasetTransformationValueUtility.IsMissing(row.Values[columnIndex]))
                    {
                        continue;
                    }

                    row.Values[columnIndex] = normalizedMean;
                    filledCellCount++;
                }
            }

            return BuildResult(dataset, configuration, new
            {
                strategy = configuration.Strategy,
                filledCellCount
            });
        }

        private static DatasetTransformationStepExecutionResult FillMode(
            TabularDataset dataset,
            HandleMissingValuesConfiguration configuration,
            IReadOnlyList<int> selectedColumns)
        {
            var filledCellCount = 0;
            foreach (var columnIndex in selectedColumns)
            {
                var column = dataset.Columns[columnIndex];
                var columnDataType = DatasetTransformationValueUtility.NormalizeDataTypeName(column.DataType);
                if (columnDataType != DatasetTransformationValueUtility.StringDataType &&
                    columnDataType != DatasetTransformationValueUtility.BooleanDataType)
                {
                    throw new UserFriendlyException($"Column '{column.Name}' must be string or boolean to use the FillMode missing value strategy.");
                }

                var modeValue = GetModeValue(dataset, columnIndex, column.Name);
                foreach (var row in dataset.Rows)
                {
                    if (!DatasetTransformationValueUtility.IsMissing(row.Values[columnIndex]))
                    {
                        continue;
                    }

                    row.Values[columnIndex] = modeValue;
                    filledCellCount++;
                }
            }

            return BuildResult(dataset, configuration, new
            {
                strategy = configuration.Strategy,
                filledCellCount
            });
        }

        private static DatasetTransformationStepExecutionResult BuildResult(
            TabularDataset dataset,
            HandleMissingValuesConfiguration configuration,
            object summary)
        {
            return new DatasetTransformationStepExecutionResult
            {
                TransformationType = DatasetTransformationType.HandleMissingValues,
                Dataset = dataset,
                CanonicalConfigurationJson = DatasetTransformationJson.Serialize(configuration),
                SummaryJson = DatasetTransformationJson.Serialize(summary)
            };
        }

        private static string GetModeValue(TabularDataset dataset, int columnIndex, string columnName)
        {
            var rankedValues = dataset.Rows
                .Select((row, index) => new
                {
                    Index = index,
                    Value = row.Values[columnIndex]
                })
                .Where(item => !DatasetTransformationValueUtility.IsMissing(item.Value))
                .GroupBy(item => item.Value)
                .Select(group => new
                {
                    Value = group.Key,
                    Count = group.Count(),
                    FirstIndex = group.Min(item => item.Index)
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.FirstIndex)
                .FirstOrDefault();

            if (rankedValues == null)
            {
                throw new UserFriendlyException($"Column '{columnName}' does not contain any non-missing values for mode imputation.");
            }

            return rankedValues.Value;
        }

        private static List<int> ResolveColumnIndexes(IReadOnlyList<string> selectedColumnNames, TabularDataset dataset)
        {
            var columnLookup = dataset.Columns
                .Select((item, index) => new { item.Name, Index = index })
                .ToDictionary(item => item.Name, item => item.Index, System.StringComparer.Ordinal);

            var selectedColumns = new List<int>();
            foreach (var selectedColumnName in selectedColumnNames)
            {
                if (!columnLookup.TryGetValue(selectedColumnName, out var columnIndex))
                {
                    throw new UserFriendlyException($"Column '{selectedColumnName}' was not found for missing value handling.");
                }

                if (!selectedColumns.Contains(columnIndex))
                {
                    selectedColumns.Add(columnIndex);
                }
            }

            return selectedColumns;
        }

        private enum MissingValueStrategy
        {
            DropRows = 1,
            FillConstant = 2,
            FillMean = 3,
            FillMode = 4
        }

        private class HandleMissingValuesConfiguration
        {
            public List<string> Columns { get; set; } = new List<string>();

            public MissingValueStrategy Strategy { get; set; }

            public string ConstantValue { get; set; }
        }
    }
}
