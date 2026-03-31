using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abp.UI;

namespace AstraLab.Services.Datasets.Transformations
{
    internal static class DatasetTransformationValueUtility
    {
        public const string IntegerDataType = "integer";
        public const string DecimalDataType = "decimal";
        public const string BooleanDataType = "boolean";
        public const string DateTimeDataType = "datetime";
        public const string StringDataType = "string";
        public const string MixedDataType = "mixed";
        public const string ObjectDataType = "object";
        public const string ArrayDataType = "array";

        public static bool IsMissing(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string BuildRowKey(IEnumerable<string> values)
        {
            return string.Join("||", values.Select(value => value ?? "<NULL>"));
        }

        public static string NormalizeValueForDataType(string value, string dataType, string columnName)
        {
            if (IsMissing(value))
            {
                return null;
            }

            switch (NormalizeDataTypeName(dataType))
            {
                case IntegerDataType:
                    return ParseInteger(value, columnName).ToString(CultureInfo.InvariantCulture);
                case DecimalDataType:
                    return ParseDecimal(value, columnName).ToString(CultureInfo.InvariantCulture);
                case BooleanDataType:
                    return ParseBoolean(value, columnName) ? "true" : "false";
                case DateTimeDataType:
                    return ParseDateTime(value, columnName).ToString("O", CultureInfo.InvariantCulture);
                default:
                    return value;
            }
        }

        public static string DetectDataType(IEnumerable<string> values)
        {
            string detectedType = null;

            foreach (var value in values.Where(item => !IsMissing(item)))
            {
                detectedType = MergeDataTypes(detectedType, DetectScalarDataType(value));
            }

            return string.IsNullOrWhiteSpace(detectedType) ? StringDataType : detectedType;
        }

        public static string NormalizeDataTypeName(string dataType)
        {
            return string.IsNullOrWhiteSpace(dataType) ? StringDataType : dataType.Trim().ToLowerInvariant();
        }

        public static long ParseInteger(string value, string columnName)
        {
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
            {
                throw new UserFriendlyException($"Column '{columnName}' contains a value that cannot be converted to integer.");
            }

            return parsedValue;
        }

        public static decimal ParseDecimal(string value, string columnName)
        {
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
            {
                throw new UserFriendlyException($"Column '{columnName}' contains a value that cannot be converted to decimal.");
            }

            return parsedValue;
        }

        public static bool ParseBoolean(string value, string columnName)
        {
            if (!bool.TryParse(value, out var parsedValue))
            {
                throw new UserFriendlyException($"Column '{columnName}' contains a value that cannot be converted to boolean.");
            }

            return parsedValue;
        }

        public static DateTime ParseDateTime(string value, string columnName)
        {
            if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var parsedValue))
            {
                throw new UserFriendlyException($"Column '{columnName}' contains a value that cannot be converted to datetime.");
            }

            return parsedValue;
        }

        private static string DetectScalarDataType(string value)
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

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out _))
            {
                return DateTimeDataType;
            }

            return StringDataType;
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
    }
}
