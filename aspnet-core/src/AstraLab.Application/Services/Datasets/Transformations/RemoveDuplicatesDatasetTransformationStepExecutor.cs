using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Executes duplicate-removal transformations.
    /// </summary>
    public class RemoveDuplicatesDatasetTransformationStepExecutor : IDatasetTransformationStepExecutor, ITransientDependency
    {
        /// <summary>
        /// Gets the supported transformation type.
        /// </summary>
        public DatasetTransformationType TransformationType => DatasetTransformationType.RemoveDuplicates;

        /// <summary>
        /// Executes the transformation step against the supplied dataset.
        /// </summary>
        public DatasetTransformationStepExecutionResult Execute(TabularDataset input, string configurationJson)
        {
            var configuration = DatasetTransformationJson.DeserializeConfiguration<RemoveDuplicatesConfiguration>(
                configurationJson,
                TransformationType) ?? new RemoveDuplicatesConfiguration();

            var dataset = input.Clone();
            var selectedColumns = NormalizeSelectedColumns(configuration.Columns, dataset);
            var seenRows = new HashSet<string>(System.StringComparer.Ordinal);
            var deduplicatedRows = new List<TabularDatasetRow>();
            var removedRowCount = 0;

            foreach (var row in dataset.Rows)
            {
                var rowKey = DatasetTransformationValueUtility.BuildRowKey(selectedColumns.Select(index => row.Values[index]));
                if (!seenRows.Add(rowKey))
                {
                    removedRowCount++;
                    continue;
                }

                deduplicatedRows.Add(row.Clone());
            }

            dataset.Rows = deduplicatedRows;
            return new DatasetTransformationStepExecutionResult
            {
                TransformationType = TransformationType,
                Dataset = dataset,
                CanonicalConfigurationJson = DatasetTransformationJson.Serialize(configuration),
                SummaryJson = DatasetTransformationJson.Serialize(new
                {
                    removedRowCount,
                    remainingRowCount = dataset.Rows.Count
                })
            };
        }

        private static List<int> NormalizeSelectedColumns(IReadOnlyList<string> selectedColumnNames, TabularDataset dataset)
        {
            if (selectedColumnNames == null || selectedColumnNames.Count == 0)
            {
                return Enumerable.Range(0, dataset.Columns.Count).ToList();
            }

            var columnLookup = dataset.Columns
                .Select((item, index) => new { item.Name, Index = index })
                .ToDictionary(item => item.Name, item => item.Index, System.StringComparer.Ordinal);

            var selectedColumns = new List<int>();
            foreach (var selectedColumnName in selectedColumnNames)
            {
                if (!columnLookup.TryGetValue(selectedColumnName, out var columnIndex))
                {
                    throw new UserFriendlyException($"Column '{selectedColumnName}' was not found for duplicate removal.");
                }

                if (!selectedColumns.Contains(columnIndex))
                {
                    selectedColumns.Add(columnIndex);
                }
            }

            if (selectedColumns.Count == 0)
            {
                throw new UserFriendlyException("At least one valid column is required for duplicate removal.");
            }

            return selectedColumns;
        }

        private class RemoveDuplicatesConfiguration
        {
            public List<string> Columns { get; set; } = new List<string>();
        }
    }
}
