using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Ingestion;
using Microsoft.VisualBasic.FileIO;

namespace AstraLab.Services.Datasets.Transformations
{
    /// <summary>
    /// Converts supported dataset files to and from a shared tabular model.
    /// </summary>
    public class DatasetTabularDataCodec : IDatasetTabularDataCodec, ITransientDependency
    {
        private const string CsvRootKind = "tabular";
        private const string JsonArrayRootKind = "array";
        private const string JsonObjectRootKind = "object";

        /// <summary>
        /// Parses the specified stored dataset content into a shared tabular model.
        /// </summary>
        public async Task<TabularDataset> ReadAsync(DatasetFormat datasetFormat, IReadOnlyList<DatasetColumn> columns, Stream content)
        {
            if (content == null || !content.CanRead)
            {
                throw new UserFriendlyException("A readable stored dataset stream is required for transformation.");
            }

            if (columns == null || columns.Count == 0)
            {
                throw new UserFriendlyException("Only tabular dataset versions with persisted columns can be transformed.");
            }

            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return ReadCsv(columns, content);
                case DatasetFormat.Json:
                    return await ReadJsonAsync(columns, content);
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported for transformation.");
            }
        }

        /// <summary>
        /// Serializes the supplied tabular dataset into the requested dataset format.
        /// </summary>
        public byte[] Write(TabularDataset dataset, DatasetFormat datasetFormat)
        {
            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return WriteCsv(dataset);
                case DatasetFormat.Json:
                    return WriteJson(dataset);
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported for transformation.");
            }
        }

        /// <summary>
        /// Builds extracted metadata for the supplied transformed dataset.
        /// </summary>
        public ExtractedRawDatasetMetadata BuildMetadata(TabularDataset dataset, DatasetFormat datasetFormat)
        {
            var columns = dataset.Columns
                .Select(item => new ExtractedDatasetColumn
                {
                    Name = item.Name,
                    DataType = string.IsNullOrWhiteSpace(item.DataType)
                        ? DatasetTransformationValueUtility.DetectDataType(dataset.Rows.Select(row => row.Values[item.Ordinal - 1]))
                        : item.DataType,
                    IsDataTypeInferred = item.IsDataTypeInferred,
                    Ordinal = item.Ordinal
                })
                .ToList();

            var schemaJson = BuildSchemaJson(dataset, datasetFormat, columns);
            return new ExtractedRawDatasetMetadata
            {
                ColumnCount = columns.Count,
                Columns = columns,
                SchemaJson = schemaJson
            };
        }

        /// <summary>
        /// Gets the default content type for the requested dataset format.
        /// </summary>
        public string GetContentType(DatasetFormat datasetFormat)
        {
            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return "text/csv";
                case DatasetFormat.Json:
                    return "application/json";
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported for transformation.");
            }
        }

        /// <summary>
        /// Gets the default file extension for the requested dataset format.
        /// </summary>
        public string GetFileExtension(DatasetFormat datasetFormat)
        {
            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return ".csv";
                case DatasetFormat.Json:
                    return ".json";
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported for transformation.");
            }
        }

        private static TabularDataset ReadCsv(IReadOnlyList<DatasetColumn> columns, Stream content)
        {
            using (var parser = new TextFieldParser(content, Encoding.UTF8, detectEncoding: true))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                var orderedColumns = columns.OrderBy(item => item.Ordinal).ToList();
                var header = parser.ReadFields();
                if (header == null)
                {
                    throw new UserFriendlyException("The stored CSV dataset must contain a header row for transformation.");
                }

                if (header.Length != orderedColumns.Count ||
                    header.Where((value, index) => !string.Equals(value?.Trim(), orderedColumns[index].Name, StringComparison.Ordinal)).Any())
                {
                    throw new UserFriendlyException("The stored CSV dataset does not match the persisted dataset column structure.");
                }

                var dataset = CreateDataset(orderedColumns, TabularJsonRootKind.Array);
                while (!parser.EndOfData)
                {
                    var row = parser.ReadFields();
                    if (row == null)
                    {
                        continue;
                    }

                    if (row.Length != orderedColumns.Count)
                    {
                        throw new UserFriendlyException("The stored CSV dataset is malformed and cannot be transformed.");
                    }

                    dataset.Rows.Add(new TabularDatasetRow
                    {
                        Values = row
                            .Select(NormalizeStringValue)
                            .ToList()
                    });
                }

                return dataset;
            }
        }

        private static async Task<TabularDataset> ReadJsonAsync(IReadOnlyList<DatasetColumn> columns, Stream content)
        {
            using (var document = await JsonDocument.ParseAsync(content))
            {
                switch (document.RootElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        return ReadJsonObject(columns, document.RootElement);
                    case JsonValueKind.Array:
                        return ReadJsonArray(columns, document.RootElement);
                    default:
                        throw new UserFriendlyException("Only tabular JSON object datasets can be transformed.");
                }
            }
        }

        private static TabularDataset ReadJsonObject(IReadOnlyList<DatasetColumn> columns, JsonElement rootElement)
        {
            var dataset = CreateDataset(columns, TabularJsonRootKind.Object);
            dataset.Rows.Add(CreateRow(columns, rootElement));
            return dataset;
        }

        private static TabularDataset ReadJsonArray(IReadOnlyList<DatasetColumn> columns, JsonElement rootElement)
        {
            var dataset = CreateDataset(columns, TabularJsonRootKind.Array);
            foreach (var item in rootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    throw new UserFriendlyException("Only tabular JSON arrays of objects can be transformed.");
                }

                dataset.Rows.Add(CreateRow(columns, item));
            }

            return dataset;
        }

        private static TabularDataset CreateDataset(IReadOnlyList<DatasetColumn> columns, TabularJsonRootKind jsonRootKind)
        {
            return new TabularDataset
            {
                JsonRootKind = jsonRootKind,
                Columns = columns
                    .OrderBy(item => item.Ordinal)
                    .Select(item => new TabularDatasetColumn
                    {
                        Name = item.Name,
                        Ordinal = item.Ordinal,
                        DataType = DatasetTransformationValueUtility.NormalizeDataTypeName(item.DataType),
                        IsDataTypeInferred = item.IsDataTypeInferred
                    })
                    .ToList()
            };
        }

        private static TabularDatasetRow CreateRow(IReadOnlyList<DatasetColumn> columns, JsonElement objectElement)
        {
            var valueLookup = objectElement.EnumerateObject()
                .ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal);

            var knownColumns = new HashSet<string>(columns.Select(item => item.Name), StringComparer.Ordinal);
            var unexpectedProperty = valueLookup.Keys.FirstOrDefault(item => !knownColumns.Contains(item));
            if (unexpectedProperty != null)
            {
                throw new UserFriendlyException("The stored JSON dataset does not match the persisted dataset column structure.");
            }

            return new TabularDatasetRow
            {
                Values = columns
                    .OrderBy(item => item.Ordinal)
                    .Select(item =>
                    {
                        if (!valueLookup.TryGetValue(item.Name, out var value))
                        {
                            return null;
                        }

                        return NormalizeJsonValue(value);
                    })
                    .ToList()
            };
        }

        private static byte[] WriteCsv(TabularDataset dataset)
        {
            var lines = new List<string>
            {
                string.Join(",", dataset.Columns.OrderBy(item => item.Ordinal).Select(item => EscapeCsvValue(item.Name)))
            };

            lines.AddRange(dataset.Rows.Select(row =>
                string.Join(",", row.Values.Select(EscapeCsvValue))));

            return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines) + Environment.NewLine);
        }

        private static byte[] WriteJson(TabularDataset dataset)
        {
            using (var stream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                if (dataset.JsonRootKind == TabularJsonRootKind.Object && dataset.Rows.Count == 1)
                {
                    WriteJsonObject(writer, dataset.Columns, dataset.Rows[0]);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var row in dataset.Rows)
                    {
                        WriteJsonObject(writer, dataset.Columns, row);
                    }

                    writer.WriteEndArray();
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        private static void WriteJsonObject(Utf8JsonWriter writer, IReadOnlyList<TabularDatasetColumn> columns, TabularDatasetRow row)
        {
            writer.WriteStartObject();
            foreach (var column in columns.OrderBy(item => item.Ordinal))
            {
                writer.WritePropertyName(column.Name);
                WriteJsonValue(writer, row.Values[column.Ordinal - 1], column.DataType, column.Name);
            }

            writer.WriteEndObject();
        }

        private static void WriteJsonValue(Utf8JsonWriter writer, string value, string dataType, string columnName)
        {
            if (DatasetTransformationValueUtility.IsMissing(value))
            {
                writer.WriteNullValue();
                return;
            }

            switch (DatasetTransformationValueUtility.NormalizeDataTypeName(dataType))
            {
                case DatasetTransformationValueUtility.IntegerDataType:
                    writer.WriteNumberValue(DatasetTransformationValueUtility.ParseInteger(value, columnName));
                    return;
                case DatasetTransformationValueUtility.DecimalDataType:
                    writer.WriteNumberValue(DatasetTransformationValueUtility.ParseDecimal(value, columnName));
                    return;
                case DatasetTransformationValueUtility.BooleanDataType:
                    writer.WriteBooleanValue(DatasetTransformationValueUtility.ParseBoolean(value, columnName));
                    return;
                case DatasetTransformationValueUtility.ObjectDataType:
                case DatasetTransformationValueUtility.ArrayDataType:
                    using (var document = JsonDocument.Parse(value))
                    {
                        document.RootElement.WriteTo(writer);
                    }

                    return;
                default:
                    writer.WriteStringValue(value);
                    return;
            }
        }

        private static string NormalizeJsonValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return value.GetBoolean() ? "true" : "false";
                default:
                    return value.GetRawText();
            }
        }

        private static string NormalizeStringValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string EscapeCsvValue(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (!value.Contains(",") && !value.Contains("\"") && !value.Contains("\r") && !value.Contains("\n"))
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        private static string BuildSchemaJson(
            TabularDataset dataset,
            DatasetFormat datasetFormat,
            IReadOnlyList<ExtractedDatasetColumn> columns)
        {
            var payload = new DatasetSchemaPayload
            {
                Format = datasetFormat.ToString().ToLowerInvariant(),
                RootKind = datasetFormat == DatasetFormat.Csv
                    ? CsvRootKind
                    : dataset.JsonRootKind == TabularJsonRootKind.Object
                        ? JsonObjectRootKind
                        : JsonArrayRootKind,
                Columns = columns
                    .Select(item => new DatasetSchemaColumnPayload
                    {
                        Name = item.Name,
                        DataType = item.DataType,
                        IsDataTypeInferred = item.IsDataTypeInferred,
                        Ordinal = item.Ordinal
                    })
                    .ToList()
            };

            return DatasetTransformationJson.Serialize(payload);
        }

        private class DatasetSchemaPayload
        {
            public string Format { get; set; }

            public string RootKind { get; set; }

            public List<DatasetSchemaColumnPayload> Columns { get; set; } = new List<DatasetSchemaColumnPayload>();
        }

        private class DatasetSchemaColumnPayload
        {
            public string Name { get; set; }

            public string DataType { get; set; }

            public bool IsDataTypeInferred { get; set; }

            public int Ordinal { get; set; }
        }
    }
}
