using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes column data-type conversion transformations.
    /// </summary>
    public class ConvertDataTypeDatasetTransformationStepExecutor : IDatasetTransformationStepExecutor, ITransientDependency
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType => DatasetTransformationType.ConvertDataType;

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        public DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson)
        {
            var configuration = DatasetTransformationJson.DeserializeConfiguration<ConvertDataTypeConfiguration>(
                configurationJson,
                TransformationType);

            if (string.IsNullOrWhiteSpace(configuration?.Column))
            {
                throw new UserFriendlyException("A target column is required for data type conversion.");
            }

            if (string.IsNullOrWhiteSpace(configuration.TargetType))
            {
                throw new UserFriendlyException("A target data type is required for data type conversion.");
            }

            var dataset = input.Clone();
            var columnIndex = GetColumnIndex(dataset, configuration.Column);
            var column = dataset.Columns[columnIndex];
            var targetType = DatasetTransformationValueUtility.NormalizeDataTypeName(configuration.TargetType);
            ValidateTargetType(targetType);

            var convertedValueCount = 0;
            foreach (var row in dataset.Rows)
            {
                if (DatasetTransformationValueUtility.IsMissing(row.Values[columnIndex]))
                {
                    continue;
                }

                row.Values[columnIndex] = DatasetTransformationValueUtility.NormalizeValueForDataType(
                    row.Values[columnIndex],
                    targetType,
                    column.Name);
                convertedValueCount++;
            }

            column.DataType = targetType;
            column.IsDataTypeInferred = false;

            configuration.TargetType = targetType;
            return new DatasetTransformationStepExecutionResult
            {
                TransformationType = TransformationType,
                Dataset = dataset,
                CanonicalConfigurationJson = DatasetTransformationJson.Serialize(configuration),
                SummaryJson = DatasetTransformationJson.Serialize(new
                {
                    column = column.Name,
                    targetType,
                    convertedValueCount
                })
            };
        }

        private static int GetColumnIndex(TabularDataset dataset, string columnName)
        {
            var columnIndex = dataset.Columns.FindIndex(item => item.Name == columnName);
            if (columnIndex < 0)
            {
                throw new UserFriendlyException($"Column '{columnName}' was not found for data type conversion.");
            }

            return columnIndex;
        }

        private static void ValidateTargetType(string targetType)
        {
            switch (targetType)
            {
                case DatasetTransformationValueUtility.IntegerDataType:
                case DatasetTransformationValueUtility.DecimalDataType:
                case DatasetTransformationValueUtility.BooleanDataType:
                case DatasetTransformationValueUtility.DateTimeDataType:
                case DatasetTransformationValueUtility.StringDataType:
                    return;
                default:
                    throw new UserFriendlyException("The target data type is not supported for data type conversion.");
            }
        }

        private class ConvertDataTypeConfiguration
        {
            public string Column { get; set; }

            public string TargetType { get; set; }
        }
    }
}
