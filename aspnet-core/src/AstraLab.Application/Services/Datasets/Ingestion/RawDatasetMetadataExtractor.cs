using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Abp.Dependency;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using Microsoft.VisualBasic.FileIO;

namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Extracts initial schema metadata from validated CSV and JSON dataset uploads.
    /// </summary>
    public class RawDatasetMetadataExtractor : IRawDatasetMetadataExtractor, ITransientDependency
    {
        private const string CsvRootKind = "tabular";
        private const string JsonArrayRootKind = "array";
        private const string JsonObjectRootKind = "object";
        private const string JsonScalarRootKind = "scalar";
        private const string StringDataType = "string";
        private const string IntegerDataType = "integer";
        private const string DecimalDataType = "decimal";
        private const string BooleanDataType = "boolean";
        private const string ObjectDataType = "object";
        private const string ArrayDataType = "array";
        private const string MixedDataType = "mixed";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Extracts schema metadata for the specified dataset content.
        /// </summary>
        public ExtractedRawDatasetMetadata Extract(byte[] content, DatasetFormat datasetFormat)
        {
            if (content == null || content.Length == 0)
            {
                throw new UserFriendlyException("Uploaded dataset files cannot be empty.");
            }

            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    return ExtractCsvMetadata(content);
                case DatasetFormat.Json:
                    return ExtractJsonMetadata(content);
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported.");
            }
        }

        /// <summary>
        /// Extracts column metadata from a CSV header row.
        /// </summary>
        private static ExtractedRawDatasetMetadata ExtractCsvMetadata(byte[] content)
        {
            using (var stream = new MemoryStream(content, writable: false))
            using (var parser = new TextFieldParser(stream, Encoding.UTF8, detectEncoding: true))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                var header = parser.ReadFields();
                if (header == null || header.Length == 0)
                {
                    throw new UserFriendlyException("The uploaded CSV file must contain a header row.");
                }

                var columns = header
                    .Select((name, index) => CreateCsvColumn(name, index + 1))
                    .ToList();

                return BuildMetadata(DatasetFormat.Csv, CsvRootKind, columns);
            }
        }

        /// <summary>
        /// Extracts column metadata from supported JSON root structures.
        /// </summary>
        private static ExtractedRawDatasetMetadata ExtractJsonMetadata(byte[] content)
        {
            using (var document = JsonDocument.Parse(content))
            {
                switch (document.RootElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        return BuildMetadata(
                            DatasetFormat.Json,
                            JsonObjectRootKind,
                            BuildColumns(BuildJsonColumnTypes(document.RootElement)));
                    case JsonValueKind.Array:
                        return ExtractJsonArrayMetadata(document.RootElement);
                    default:
                        return BuildMetadata(DatasetFormat.Json, JsonScalarRootKind, new List<ExtractedDatasetColumn>());
                }
            }
        }

        /// <summary>
        /// Extracts metadata from a supported JSON array shape.
        /// </summary>
        private static ExtractedRawDatasetMetadata ExtractJsonArrayMetadata(JsonElement rootElement)
        {
            var columnTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            var sawObject = false;

            foreach (var item in rootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                if (item.ValueKind != JsonValueKind.Object)
                {
                    return BuildMetadata(DatasetFormat.Json, JsonArrayRootKind, new List<ExtractedDatasetColumn>());
                }

                sawObject = true;
                MergeJsonColumnTypes(columnTypes, item);
            }

            if (!sawObject)
            {
                return BuildMetadata(DatasetFormat.Json, JsonArrayRootKind, new List<ExtractedDatasetColumn>());
            }

            return BuildMetadata(DatasetFormat.Json, JsonArrayRootKind, BuildColumns(columnTypes));
        }

        /// <summary>
        /// Builds an extracted CSV column and validates its header name.
        /// </summary>
        private static ExtractedDatasetColumn CreateCsvColumn(string name, int ordinal)
        {
            var trimmedName = name?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                throw new UserFriendlyException("The uploaded CSV file must contain named header columns.");
            }

            return new ExtractedDatasetColumn
            {
                Name = trimmedName,
                DataType = StringDataType,
                IsDataTypeInferred = true,
                Ordinal = ordinal
            };
        }

        /// <summary>
        /// Builds extracted columns from ordered JSON column type metadata.
        /// </summary>
        private static List<ExtractedDatasetColumn> BuildColumns(IReadOnlyDictionary<string, string> columnTypes)
        {
            return columnTypes
                .Select((item, index) => new ExtractedDatasetColumn
                {
                    Name = item.Key,
                    DataType = item.Value ?? StringDataType,
                    IsDataTypeInferred = true,
                    Ordinal = index + 1
                })
                .ToList();
        }

        /// <summary>
        /// Builds ordered type metadata for a JSON object.
        /// </summary>
        private static Dictionary<string, string> BuildJsonColumnTypes(JsonElement objectElement)
        {
            var columnTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            MergeJsonColumnTypes(columnTypes, objectElement);
            return columnTypes;
        }

        /// <summary>
        /// Merges JSON object properties into the ordered type map.
        /// </summary>
        private static void MergeJsonColumnTypes(IDictionary<string, string> columnTypes, JsonElement objectElement)
        {
            foreach (var property in objectElement.EnumerateObject())
            {
                var detectedType = DetectJsonDataType(property.Value);
                if (!columnTypes.TryGetValue(property.Name, out var existingType))
                {
                    columnTypes[property.Name] = detectedType;
                    continue;
                }

                columnTypes[property.Name] = MergeDataTypes(existingType, detectedType);
            }
        }

        /// <summary>
        /// Detects a coarse schema data type from a JSON value.
        /// </summary>
        private static string DetectJsonDataType(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    return value.TryGetInt64(out _) ? IntegerDataType : DecimalDataType;
                case JsonValueKind.String:
                    return StringDataType;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return BooleanDataType;
                case JsonValueKind.Object:
                    return ObjectDataType;
                case JsonValueKind.Array:
                    return ArrayDataType;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return MixedDataType;
            }
        }

        /// <summary>
        /// Merges two inferred data types into a single coarse type.
        /// </summary>
        private static string MergeDataTypes(string existingType, string newType)
        {
            if (string.IsNullOrWhiteSpace(existingType))
            {
                return newType;
            }

            if (string.IsNullOrWhiteSpace(newType) || string.Equals(existingType, newType, StringComparison.Ordinal))
            {
                return existingType;
            }

            if ((string.Equals(existingType, IntegerDataType, StringComparison.Ordinal) &&
                 string.Equals(newType, DecimalDataType, StringComparison.Ordinal)) ||
                (string.Equals(existingType, DecimalDataType, StringComparison.Ordinal) &&
                 string.Equals(newType, IntegerDataType, StringComparison.Ordinal)))
            {
                return DecimalDataType;
            }

            return MixedDataType;
        }

        /// <summary>
        /// Builds the extracted metadata response and serialized schema payload.
        /// </summary>
        private static ExtractedRawDatasetMetadata BuildMetadata(
            DatasetFormat datasetFormat,
            string rootKind,
            List<ExtractedDatasetColumn> columns)
        {
            var schemaPayload = new DatasetSchemaPayload
            {
                Format = datasetFormat.ToString().ToLowerInvariant(),
                RootKind = rootKind,
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

            return new ExtractedRawDatasetMetadata
            {
                ColumnCount = columns.Count,
                Columns = columns,
                SchemaJson = JsonSerializer.Serialize(schemaPayload, SerializerOptions)
            };
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
