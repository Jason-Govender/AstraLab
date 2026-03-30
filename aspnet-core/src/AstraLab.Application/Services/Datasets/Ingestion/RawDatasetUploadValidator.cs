using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;
using AstraLab.Services.Datasets.Dto;
using Microsoft.VisualBasic.FileIO;

namespace AstraLab.Services.Datasets.Ingestion
{
    /// <summary>
    /// Validates supported raw dataset uploads before ingestion records are created.
    /// </summary>
    public class RawDatasetUploadValidator : IRawDatasetUploadValidator, ITransientDependency
    {
        private static readonly ISet<string> CsvContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "text/csv",
            "application/csv",
            "application/vnd.ms-excel"
        };

        private static readonly ISet<string> JsonContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/json",
            "text/json"
        };

        /// <summary>
        /// Validates the request and infers the dataset format from the file extension.
        /// </summary>
        public async Task<DatasetFormat> ValidateAsync(UploadRawDatasetRequest request)
        {
            ValidateRequestMetadata(request);

            var extension = Path.GetExtension(request.OriginalFileName.Trim()).ToLowerInvariant();
            var datasetFormat = GetDatasetFormat(extension);
            ValidateContentType(datasetFormat, request.ContentType);

            switch (datasetFormat)
            {
                case DatasetFormat.Csv:
                    ValidateCsv(request.Content);
                    break;
                case DatasetFormat.Json:
                    await ValidateJsonAsync(request.Content);
                    break;
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported.");
            }

            return datasetFormat;
        }

        private static void ValidateRequestMetadata(UploadRawDatasetRequest request)
        {
            if (request == null)
            {
                throw new UserFriendlyException("A dataset upload request is required.");
            }

            if (request.Name.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Dataset name is required.");
            }

            if (request.OriginalFileName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("A dataset file is required.");
            }

            if (request.Content == null || request.Content.Length == 0)
            {
                throw new UserFriendlyException("Uploaded dataset files cannot be empty.");
            }
        }

        private static DatasetFormat GetDatasetFormat(string extension)
        {
            switch (extension)
            {
                case ".csv":
                    return DatasetFormat.Csv;
                case ".json":
                    return DatasetFormat.Json;
                default:
                    throw new UserFriendlyException("Only CSV and JSON dataset files are supported.");
            }
        }

        private static void ValidateContentType(DatasetFormat datasetFormat, string contentType)
        {
            if (contentType.IsNullOrWhiteSpace())
            {
                return;
            }

            var normalizedContentType = contentType.Trim();
            var isAllowed = datasetFormat == DatasetFormat.Csv
                ? CsvContentTypes.Contains(normalizedContentType)
                : JsonContentTypes.Contains(normalizedContentType);

            if (!isAllowed)
            {
                throw new UserFriendlyException("The uploaded file content type does not match the supported dataset format.");
            }
        }

        private static async Task ValidateJsonAsync(byte[] content)
        {
            try
            {
                using (var stream = new MemoryStream(content, writable: false))
                {
                    await JsonDocument.ParseAsync(stream);
                }
            }
            catch (JsonException)
            {
                throw new UserFriendlyException("The uploaded JSON file is malformed.");
            }
        }

        private static void ValidateCsv(byte[] content)
        {
            try
            {
                using (var stream = new MemoryStream(content, writable: false))
                using (var parser = new TextFieldParser(stream, Encoding.UTF8, detectEncoding: true))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    var header = parser.ReadFields();
                    if (header == null || header.Length == 0 || header.All(string.IsNullOrWhiteSpace))
                    {
                        throw new UserFriendlyException("The uploaded CSV file must contain a header row.");
                    }

                    var expectedFieldCount = header.Length;
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        if (fields == null || fields.Length == 0)
                        {
                            continue;
                        }

                        if (fields.Length != expectedFieldCount)
                        {
                            throw new UserFriendlyException("The uploaded CSV file is malformed.");
                        }
                    }
                }
            }
            catch (MalformedLineException)
            {
                throw new UserFriendlyException("The uploaded CSV file is malformed.");
            }
        }
    }
}
