using System;
using System.Collections.Generic;
using System.Linq;

namespace AstraLab.Services.Datasets.Transformations
{
    public enum TabularJsonRootKind
    {
        Array = 1,
        Object = 2
    }

    public class TabularDataset
    {
        public TabularJsonRootKind JsonRootKind { get; set; } = TabularJsonRootKind.Array;

        public List<TabularDatasetColumn> Columns { get; set; } = new List<TabularDatasetColumn>();

        public List<TabularDatasetRow> Rows { get; set; } = new List<TabularDatasetRow>();

        public TabularDataset Clone()
        {
            return new TabularDataset
            {
                JsonRootKind = JsonRootKind,
                Columns = Columns
                    .Select(item => item.Clone())
                    .ToList(),
                Rows = Rows
                    .Select(item => item.Clone())
                    .ToList()
            };
        }

        public int GetRequiredColumnIndex(string columnName)
        {
            var index = Columns.FindIndex(item => string.Equals(item.Name, columnName, StringComparison.Ordinal));
            if (index < 0)
            {
                throw new InvalidOperationException($"Column '{columnName}' was not found in the tabular dataset.");
            }

            return index;
        }
    }

    public class TabularDatasetColumn
    {
        public string Name { get; set; }

        public int Ordinal { get; set; }

        public string DataType { get; set; }

        public bool IsDataTypeInferred { get; set; } = true;

        public TabularDatasetColumn Clone()
        {
            return new TabularDatasetColumn
            {
                Name = Name,
                Ordinal = Ordinal,
                DataType = DataType,
                IsDataTypeInferred = IsDataTypeInferred
            };
        }
    }

    public class TabularDatasetRow
    {
        public List<string> Values { get; set; } = new List<string>();

        public TabularDatasetRow Clone()
        {
            return new TabularDatasetRow
            {
                Values = Values.ToList()
            };
        }
    }
}
