using System.Text.Json;
using System.Text.Json.Serialization;
using Abp.UI;
using AstraLab.Core.Domains.Datasets;

namespace AstraLab.Services.Datasets.Transformations
{
    internal static class DatasetTransformationJson
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public static T DeserializeConfiguration<T>(string configurationJson, DatasetTransformationType transformationType)
        {
            if (string.IsNullOrWhiteSpace(configurationJson))
            {
                throw new UserFriendlyException($"A configuration payload is required for the {transformationType} transformation.");
            }

            try
            {
                return JsonSerializer.Deserialize<T>(configurationJson, SerializerOptions);
            }
            catch (JsonException)
            {
                throw new UserFriendlyException($"The configuration for the {transformationType} transformation is malformed.");
            }
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }
    }
}
