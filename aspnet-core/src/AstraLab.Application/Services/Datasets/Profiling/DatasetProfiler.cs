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
using Microsoft.VisualBasic.FileIO;

namespace AstraLab.Services.Datasets.Profiling
{
    /// <summary>
    /// Profiles raw dataset content for supported dataset formats.
    /// </summary>
    public class DatasetProfiler : IDatasetProfiler, ITransientDependency
    {
        private const string IntegerDataType = "integer";
        private const string DecimalDataType = "decimal";
        private const string BooleanDataType = "boolean";
        private const string StringDataType = "string";
        private const string MixedDataType = "mixed";
        private const decimal DuplicateRateWeight = 0.3m;
        private const decimal NullRateWeight = 0.4m;
        private const decimal AnomalyRateWeight = 0.3m;
        private const decimal ZScoreThreshold = 3.0m;
        private const int MinimumNumericObservationsForAnomalyDetection = 5;

        /// <summary>
        /// Profiles the supplied dataset content.
        /// </summary>
        public async Task<ProfileDatasetVersionResult> ProfileAsync(ProfileDatasetVersionRequest request)
        {
            ValidateRequest(request);

            switch (request.DatasetFormat)
            {
                case DatasetFormat.Csv:
                    return await ProfileCsvAsync(request);
                case DatasetFormat.Json:
                    return await ProfileJsonAsync(request);
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported for profiling.");
            }
        }

        private static void ValidateRequest(ProfileDatasetVersionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Content == null || !request.Content.CanRead)
            {
                throw new UserFriendlyException("A readable raw dataset stream is required for profiling.");
            }

            if (request.Columns == null)
            {
                throw new UserFriendlyException("Dataset columns are required for profiling.");
            }
        }

        private static async Task<ProfileDatasetVersionResult> ProfileCsvAsync(ProfileDatasetVersionRequest request)
        {
            using (var parser = new TextFieldParser(request.Content, Encoding.UTF8, detectEncoding: true))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                var header = parser.ReadFields();
                if (header == null)
                {
                    throw new UserFriendlyException("The stored CSV dataset must contain a header row for profiling.");
                }

                var orderedColumns = request.Columns.OrderBy(item => item.Ordinal).ToList();
                if (header.Length != orderedColumns.Count)
                {
                    throw new UserFriendlyException("The stored CSV dataset does not match the persisted dataset column structure.");
                }

                var columnStates = orderedColumns
                    .Select(item => new ColumnProfilingState(item.DatasetColumnId))
                    .ToList();

                var duplicateRows = new HashSet<string>(StringComparer.Ordinal);
                var duplicateRowCount = 0L;
                var rowCount = 0L;

                while (!parser.EndOfData)
                {
                    var row = parser.ReadFields();
                    if (row == null)
                    {
                        continue;
                    }

                    if (row.Length != orderedColumns.Count)
                    {
                        throw new UserFriendlyException("The stored CSV dataset is malformed and cannot be profiled.");
                    }

                    rowCount++;
                    if (!duplicateRows.Add(BuildRowKey(row.Select(NormalizeStringValue))))
                    {
                        duplicateRowCount++;
                    }

                    for (var index = 0; index < orderedColumns.Count; index++)
                    {
                        columnStates[index].ObserveStringValue(row[index]);
                    }
                }

                return BuildResult(rowCount, duplicateRowCount, columnStates);
            }
        }

        private static async Task<ProfileDatasetVersionResult> ProfileJsonAsync(ProfileDatasetVersionRequest request)
        {
            using (var document = await JsonDocument.ParseAsync(request.Content))
            {
                var orderedColumns = request.Columns.OrderBy(item => item.Ordinal).ToList();
                var columnStates = orderedColumns
                    .Select(item => new ColumnProfilingState(item.DatasetColumnId))
                    .ToDictionary(item => item.DatasetColumnId, item => item);

                long rowCount;
                long duplicateRowCount;
                var duplicateRows = new HashSet<string>(StringComparer.Ordinal);

                switch (document.RootElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        rowCount = 1;
                        ObserveJsonObject(document.RootElement, orderedColumns, columnStates);
                        duplicateRows.Add(BuildObjectRowKey(document.RootElement, orderedColumns));
                        duplicateRowCount = 0;
                        break;
                    case JsonValueKind.Array:
                        rowCount = 0;
                        duplicateRowCount = 0;

                        foreach (var item in document.RootElement.EnumerateArray())
                        {
                            rowCount++;

                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                ObserveJsonObject(item, orderedColumns, columnStates);
                                if (!duplicateRows.Add(BuildObjectRowKey(item, orderedColumns)))
                                {
                                    duplicateRowCount++;
                                }

                                continue;
                            }

                            if (!duplicateRows.Add(BuildScalarRowKey(item)))
                            {
                                duplicateRowCount++;
                            }
                        }
                        break;
                    default:
                        rowCount = 1;
                        duplicateRows.Add(BuildScalarRowKey(document.RootElement));
                        duplicateRowCount = 0;
                        break;
                }

                return BuildResult(rowCount, duplicateRowCount, orderedColumns.Select(item => columnStates[item.DatasetColumnId]).ToList());
            }
        }

        private static void ObserveJsonObject(
            JsonElement objectElement,
            IReadOnlyList<ProfileDatasetColumnRequest> orderedColumns,
            IReadOnlyDictionary<long, ColumnProfilingState> columnStates)
        {
            var valueLookup = objectElement.EnumerateObject()
                .ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal);

            foreach (var column in orderedColumns)
            {
                if (!valueLookup.TryGetValue(column.Name, out var value))
                {
                    columnStates[column.DatasetColumnId].ObserveJsonNull();
                    continue;
                }

                columnStates[column.DatasetColumnId].ObserveJsonValue(value);
            }
        }

        private static string BuildObjectRowKey(JsonElement objectElement, IReadOnlyList<ProfileDatasetColumnRequest> orderedColumns)
        {
            var valueLookup = objectElement.EnumerateObject()
                .ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal);

            var normalizedValues = orderedColumns
                .Select(column => valueLookup.TryGetValue(column.Name, out var value) ? NormalizeJsonValue(value) : "<NULL>");

            return BuildRowKey(normalizedValues);
        }

        private static string BuildScalarRowKey(JsonElement element)
        {
            return BuildRowKey(new[] { NormalizeJsonValue(element) });
        }

        private static string BuildRowKey(IEnumerable<string> normalizedValues)
        {
            return string.Join("||", normalizedValues);
        }

        private static string NormalizeStringValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<NULL>" : value;
        }

        private static string NormalizeJsonValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "<NULL>";
                case JsonValueKind.String:
                    return value.GetString() ?? "<NULL>";
                default:
                    return value.GetRawText();
            }
        }

        private static ProfileDatasetVersionResult BuildResult(long rowCount, long duplicateRowCount, IReadOnlyList<ColumnProfilingState> columnStates)
        {
            var totalNullCount = columnStates.Sum(item => item.NullCount);
            var totalCells = rowCount > 0 && columnStates.Count > 0 ? rowCount * columnStates.Count : 0;
            var overallNullPercentage = totalCells == 0 ? 0m : RoundPercentage((decimal)totalNullCount / totalCells);
            var totalAnomalyCount = columnStates.Sum(item => item.GetAnomalyCount());
            var totalNumericObservationCount = columnStates.Sum(item => item.GetNumericObservationCount());
            var overallAnomalyPercentage = totalNumericObservationCount == 0
                ? 0m
                : RoundPercentage((decimal)totalAnomalyCount / totalNumericObservationCount);
            var duplicateRate = rowCount == 0 ? 0m : (decimal)duplicateRowCount / rowCount;
            var overallNullRate = totalCells == 0 ? 0m : (decimal)totalNullCount / totalCells;
            var anomalyRate = totalNumericObservationCount == 0 ? 0m : (decimal)totalAnomalyCount / totalNumericObservationCount;
            var weightedRisk = (NullRateWeight * overallNullRate) + (DuplicateRateWeight * duplicateRate) + (AnomalyRateWeight * anomalyRate);
            var dataHealthScore = Math.Max(0m, Math.Round(100m * (1m - weightedRisk), 2, MidpointRounding.AwayFromZero));

            return new ProfileDatasetVersionResult
            {
                RowCount = rowCount,
                DuplicateRowCount = duplicateRowCount,
                DataHealthScore = dataHealthScore,
                SummaryJson = DatasetProfileSerialization.BuildSummaryJson(
                    totalNullCount,
                    overallNullPercentage,
                    totalAnomalyCount,
                    overallAnomalyPercentage),
                Columns = columnStates
                    .Select(item =>
                    {
                        var nullPercentage = rowCount == 0 ? 0m : RoundPercentage((decimal)item.NullCount / rowCount);
                        var anomalyCount = item.GetAnomalyCount();
                        var numericObservationCount = item.GetNumericObservationCount();
                        var anomalyPercentage = numericObservationCount == 0
                            ? 0m
                            : RoundPercentage((decimal)anomalyCount / numericObservationCount);

                        return new ProfiledDatasetColumnResult
                        {
                            DatasetColumnId = item.DatasetColumnId,
                            InferredDataType = item.GetFinalDataType(),
                            NullCount = item.NullCount,
                            NullPercentage = nullPercentage,
                            DistinctCount = item.GetDistinctCount(),
                            StatisticsJson = DatasetProfileSerialization.BuildColumnStatisticsJson(
                                nullPercentage,
                                item.GetMean(),
                                item.GetMin(),
                                item.GetMax(),
                                anomalyCount,
                                anomalyPercentage,
                                anomalyCount > 0)
                        };
                    })
                    .ToList()
            };
        }

        private static decimal RoundPercentage(decimal ratio)
        {
            return Math.Round(ratio * 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static string DetectStringDataType(string value)
        {
            if (bool.TryParse(value, out _))
            {
                return BooleanDataType;
            }

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                return IntegerDataType;
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                return DecimalDataType;
            }

            return StringDataType;
        }

        private static string DetectJsonDataType(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return BooleanDataType;
                case JsonValueKind.Number:
                    return value.TryGetInt64(out _) ? IntegerDataType : DecimalDataType;
                case JsonValueKind.String:
                    return StringDataType;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return MixedDataType;
            }
        }

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

        private class ColumnProfilingState
        {
            private readonly HashSet<string> _distinctValues = new HashSet<string>(StringComparer.Ordinal);
            private readonly List<decimal> _numericValues = new List<decimal>();
            private string _inferredDataType;
            private decimal _numericSum;
            private decimal? _numericMin;
            private decimal? _numericMax;
            private long _numericCount;

            public ColumnProfilingState(long datasetColumnId)
            {
                DatasetColumnId = datasetColumnId;
            }

            public long DatasetColumnId { get; }

            public long NullCount { get; private set; }

            public void ObserveStringValue(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    NullCount++;
                    return;
                }

                _distinctValues.Add(value);

                var detectedType = DetectStringDataType(value);
                _inferredDataType = MergeDataTypes(_inferredDataType, detectedType);

                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var numericValue))
                {
                    ObserveNumericValue(numericValue);
                }
            }

            public void ObserveJsonNull()
            {
                NullCount++;
            }

            public void ObserveJsonValue(JsonElement value)
            {
                if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
                {
                    NullCount++;
                    return;
                }

                _distinctValues.Add(NormalizeJsonValue(value));

                var detectedType = DetectJsonDataType(value);
                _inferredDataType = MergeDataTypes(_inferredDataType, detectedType);

                if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var numericValue))
                {
                    ObserveNumericValue(numericValue);
                }
            }

            public string GetFinalDataType()
            {
                return string.IsNullOrWhiteSpace(_inferredDataType) ? StringDataType : _inferredDataType;
            }

            public long? GetDistinctCount()
            {
                return _distinctValues.Count;
            }

            public long GetNumericObservationCount()
            {
                return IsEligibleForAnomalyDetection() ? _numericValues.Count : 0;
            }

            public long GetAnomalyCount()
            {
                if (!IsEligibleForAnomalyDetection())
                {
                    return 0;
                }

                var mean = _numericSum / _numericCount;
                var variance = _numericValues
                    .Select(item => (item - mean) * (item - mean))
                    .Average();

                if (variance <= 0m)
                {
                    return 0;
                }

                var standardDeviation = (decimal)Math.Sqrt((double)variance);
                if (standardDeviation <= 0m)
                {
                    return 0;
                }

                return _numericValues.LongCount(item =>
                    Math.Abs((item - mean) / standardDeviation) >= ZScoreThreshold);
            }

            public decimal? GetMean()
            {
                if (!IsNumericType())
                {
                    return null;
                }

                return _numericCount == 0
                    ? null
                    : Math.Round(_numericSum / _numericCount, 4, MidpointRounding.AwayFromZero);
            }

            public decimal? GetMin()
            {
                return IsNumericType() ? _numericMin : null;
            }

            public decimal? GetMax()
            {
                return IsNumericType() ? _numericMax : null;
            }

            private bool IsNumericType()
            {
                var finalType = GetFinalDataType();
                return string.Equals(finalType, IntegerDataType, StringComparison.Ordinal) ||
                       string.Equals(finalType, DecimalDataType, StringComparison.Ordinal);
            }

            private bool IsEligibleForAnomalyDetection()
            {
                return IsNumericType() && _numericValues.Count >= MinimumNumericObservationsForAnomalyDetection;
            }

            private void ObserveNumericValue(decimal value)
            {
                _numericCount++;
                _numericSum += value;
                _numericValues.Add(value);
                _numericMin = !_numericMin.HasValue || value < _numericMin.Value ? value : _numericMin.Value;
                _numericMax = !_numericMax.HasValue || value > _numericMax.Value ? value : _numericMax.Value;
            }
        }
    }
}
