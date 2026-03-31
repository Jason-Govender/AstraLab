using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes row-filtering transformations.
    /// </summary>
    public class FilterRowsDatasetTransformationStepExecutor : IDatasetTransformationStepExecutor, ITransientDependency
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType => DatasetTransformationType.FilterRows;

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        public DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson)
        {
            var configuration = DatasetTransformationJson.DeserializeConfiguration<FilterRowsConfiguration>(
                configurationJson,
                TransformationType);

            if (configuration?.Conditions == null || configuration.Conditions.Count == 0)
            {
                throw new UserFriendlyException("At least one filter condition is required for row filtering.");
            }

            configuration.Match = configuration.Match == FilterMatchMode.Any
                ? FilterMatchMode.Any
                : FilterMatchMode.All;

            var dataset = input.Clone();
            var evaluatedConditions = configuration.Conditions
                .Select(condition => BuildCondition(dataset, condition))
                .ToList();

            var originalRowCount = dataset.Rows.Count;
            dataset.Rows = dataset.Rows
                .Where(row => Matches(row, evaluatedConditions, configuration.Match))
                .Select(row => row.Clone())
                .ToList();

            return new DatasetTransformationStepExecutionResult
            {
                TransformationType = TransformationType,
                Dataset = dataset,
                CanonicalConfigurationJson = DatasetTransformationJson.Serialize(configuration),
                SummaryJson = DatasetTransformationJson.Serialize(new
                {
                    keptRowCount = dataset.Rows.Count,
                    removedRowCount = originalRowCount - dataset.Rows.Count
                })
            };
        }

        private static bool Matches(
            TabularDatasetRow row,
            IReadOnlyList<EvaluatedCondition> conditions,
            FilterMatchMode matchMode)
        {
            var evaluations = conditions.Select(condition => EvaluateCondition(row.Values[condition.ColumnIndex], condition)).ToList();
            return matchMode == FilterMatchMode.Any ? evaluations.Any(item => item) : evaluations.All(item => item);
        }

        private static EvaluatedCondition BuildCondition(TabularDataset dataset, FilterCondition condition)
        {
            if (string.IsNullOrWhiteSpace(condition?.Column))
            {
                throw new UserFriendlyException("Each filter condition must specify a column.");
            }

            var columnIndex = dataset.Columns.FindIndex(item => item.Name == condition.Column);
            if (columnIndex < 0)
            {
                throw new UserFriendlyException($"Column '{condition.Column}' was not found for row filtering.");
            }

            var column = dataset.Columns[columnIndex];
            var normalizedDataType = DatasetTransformationValueUtility.NormalizeDataTypeName(column.DataType);

            ValidateOperator(normalizedDataType, condition.Operator, column.Name);
            return new EvaluatedCondition
            {
                ColumnIndex = columnIndex,
                ColumnName = column.Name,
                DataType = normalizedDataType,
                Operator = condition.Operator,
                Value = condition.Value
            };
        }

        private static void ValidateOperator(string dataType, FilterOperator filterOperator, string columnName)
        {
            if ((filterOperator == FilterOperator.GreaterThan ||
                 filterOperator == FilterOperator.GreaterThanOrEqual ||
                 filterOperator == FilterOperator.LessThan ||
                 filterOperator == FilterOperator.LessThanOrEqual) &&
                dataType != DatasetTransformationValueUtility.IntegerDataType &&
                dataType != DatasetTransformationValueUtility.DecimalDataType &&
                dataType != DatasetTransformationValueUtility.DateTimeDataType)
            {
                throw new UserFriendlyException($"Column '{columnName}' does not support the requested comparison operator.");
            }
        }

        private static bool EvaluateCondition(string cellValue, EvaluatedCondition condition)
        {
            switch (condition.Operator)
            {
                case FilterOperator.IsNull:
                    return DatasetTransformationValueUtility.IsMissing(cellValue);
                case FilterOperator.IsNotNull:
                    return !DatasetTransformationValueUtility.IsMissing(cellValue);
                case FilterOperator.Contains:
                    return !DatasetTransformationValueUtility.IsMissing(cellValue) &&
                           cellValue.IndexOf(condition.Value ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) >= 0;
                case FilterOperator.StartsWith:
                    return !DatasetTransformationValueUtility.IsMissing(cellValue) &&
                           cellValue.StartsWith(condition.Value ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
                case FilterOperator.EndsWith:
                    return !DatasetTransformationValueUtility.IsMissing(cellValue) &&
                           cellValue.EndsWith(condition.Value ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
            }

            if (DatasetTransformationValueUtility.IsMissing(cellValue))
            {
                return false;
            }

            switch (condition.DataType)
            {
                case DatasetTransformationValueUtility.IntegerDataType:
                case DatasetTransformationValueUtility.DecimalDataType:
                    return EvaluateNumericCondition(cellValue, condition);
                case DatasetTransformationValueUtility.BooleanDataType:
                    return EvaluateBooleanCondition(cellValue, condition);
                case DatasetTransformationValueUtility.DateTimeDataType:
                    return EvaluateDateTimeCondition(cellValue, condition);
                default:
                    return EvaluateStringCondition(cellValue, condition);
            }
        }

        private static bool EvaluateNumericCondition(string cellValue, EvaluatedCondition condition)
        {
            var left = DatasetTransformationValueUtility.ParseDecimal(cellValue, condition.ColumnName);
            var right = DatasetTransformationValueUtility.ParseDecimal(condition.Value, condition.ColumnName);

            switch (condition.Operator)
            {
                case FilterOperator.Equals:
                    return left == right;
                case FilterOperator.NotEquals:
                    return left != right;
                case FilterOperator.GreaterThan:
                    return left > right;
                case FilterOperator.GreaterThanOrEqual:
                    return left >= right;
                case FilterOperator.LessThan:
                    return left < right;
                case FilterOperator.LessThanOrEqual:
                    return left <= right;
                default:
                    throw new UserFriendlyException("The requested numeric filter operator is not supported.");
            }
        }

        private static bool EvaluateBooleanCondition(string cellValue, EvaluatedCondition condition)
        {
            var left = DatasetTransformationValueUtility.ParseBoolean(cellValue, condition.ColumnName);
            var right = DatasetTransformationValueUtility.ParseBoolean(condition.Value, condition.ColumnName);

            switch (condition.Operator)
            {
                case FilterOperator.Equals:
                    return left == right;
                case FilterOperator.NotEquals:
                    return left != right;
                default:
                    throw new UserFriendlyException("Boolean filters only support Equals and NotEquals.");
            }
        }

        private static bool EvaluateDateTimeCondition(string cellValue, EvaluatedCondition condition)
        {
            var left = DatasetTransformationValueUtility.ParseDateTime(cellValue, condition.ColumnName);
            var right = DatasetTransformationValueUtility.ParseDateTime(condition.Value, condition.ColumnName);

            switch (condition.Operator)
            {
                case FilterOperator.Equals:
                    return left == right;
                case FilterOperator.NotEquals:
                    return left != right;
                case FilterOperator.GreaterThan:
                    return left > right;
                case FilterOperator.GreaterThanOrEqual:
                    return left >= right;
                case FilterOperator.LessThan:
                    return left < right;
                case FilterOperator.LessThanOrEqual:
                    return left <= right;
                default:
                    throw new UserFriendlyException("The requested datetime filter operator is not supported.");
            }
        }

        private static bool EvaluateStringCondition(string cellValue, EvaluatedCondition condition)
        {
            var comparisonValue = condition.Value ?? string.Empty;
            switch (condition.Operator)
            {
                case FilterOperator.Equals:
                    return string.Equals(cellValue, comparisonValue, System.StringComparison.OrdinalIgnoreCase);
                case FilterOperator.NotEquals:
                    return !string.Equals(cellValue, comparisonValue, System.StringComparison.OrdinalIgnoreCase);
                default:
                    throw new UserFriendlyException("The requested string filter operator is not supported.");
            }
        }

        private enum FilterMatchMode
        {
            All = 1,
            Any = 2
        }

        private enum FilterOperator
        {
            Equals = 1,
            NotEquals = 2,
            GreaterThan = 3,
            GreaterThanOrEqual = 4,
            LessThan = 5,
            LessThanOrEqual = 6,
            Contains = 7,
            StartsWith = 8,
            EndsWith = 9,
            IsNull = 10,
            IsNotNull = 11
        }

        private class FilterRowsConfiguration
        {
            public FilterMatchMode Match { get; set; } = FilterMatchMode.All;

            public List<FilterCondition> Conditions { get; set; } = new List<FilterCondition>();
        }

        private class FilterCondition
        {
            public string Column { get; set; }

            public FilterOperator Operator { get; set; }

            public string Value { get; set; }
        }

        private class EvaluatedCondition
        {
            public int ColumnIndex { get; set; }

            public string ColumnName { get; set; }

            public string DataType { get; set; }

            public FilterOperator Operator { get; set; }

            public string Value { get; set; }
        }
    }
}
