using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;

namespace AstraLab.Services.AI
{
    /// <summary>
    /// Calls Groq's OpenAI-compatible Responses API to generate dataset-grounded text.
    /// </summary>
    public class GroqAiTextGenerationClient : IAiTextGenerationClient, ITransientDependency
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GroqAiOptions _groqAiOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroqAiTextGenerationClient"/> class.
        /// </summary>
        public GroqAiTextGenerationClient(IHttpClientFactory httpClientFactory, GroqAiOptions groqAiOptions)
        {
            _httpClientFactory = httpClientFactory;
            _groqAiOptions = groqAiOptions;
        }

        /// <summary>
        /// Generates text for the supplied request.
        /// </summary>
        public async Task<AiTextGenerationResult> GenerateTextAsync(AiTextGenerationRequest request)
        {
            ValidateConfiguration();

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_groqAiOptions.TimeoutSeconds > 0 ? _groqAiOptions.TimeoutSeconds : 60);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _groqAiOptions.ApiKey);

            var payload = new GroqResponsesRequest
            {
                Model = _groqAiOptions.Model,
                MaxOutputTokens = _groqAiOptions.MaxOutputTokens > 0 ? _groqAiOptions.MaxOutputTokens : 800,
                Input = BuildInputMessages(request)
            };

            if (!string.IsNullOrWhiteSpace(_groqAiOptions.ReasoningEffort))
            {
                payload.Reasoning = new GroqReasoningRequest
                {
                    Effort = _groqAiOptions.ReasoningEffort
                };
            }

            var requestJson = JsonSerializer.Serialize(payload, SerializerOptions);
            var response = await client.PostAsync(
                BuildResponsesEndpoint(),
                new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("The AI provider request failed: " + BuildProviderErrorMessage(responseBody, (int)response.StatusCode));
            }

            return ParseSuccessResponse(responseBody);
        }

        /// <summary>
        /// Builds the provider request message list in system-history-user order.
        /// </summary>
        private static IReadOnlyList<GroqInputMessage> BuildInputMessages(AiTextGenerationRequest request)
        {
            var messages = new List<GroqInputMessage>
            {
                BuildMessage("system", request.SystemInstructions)
            };

            if (request.ConversationHistory != null)
            {
                messages.AddRange(request.ConversationHistory.Select(item => BuildMessage(item.Role, item.Content)));
            }

            messages.Add(BuildMessage("user", request.UserMessage));
            return messages;
        }

        /// <summary>
        /// Builds a single provider input message.
        /// </summary>
        private static GroqInputMessage BuildMessage(string role, string text)
        {
            return new GroqInputMessage
            {
                Role = role,
                Content = new[]
                {
                    new GroqInputContent
                    {
                        Type = "input_text",
                        Text = text
                    }
                }
            };
        }

        /// <summary>
        /// Parses the successful provider response into the neutral result shape.
        /// </summary>
        private AiTextGenerationResult ParseSuccessResponse(string responseBody)
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                var root = document.RootElement;
                var text = TryReadOutputText(root);

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new UserFriendlyException("The AI provider returned a response without generated text.");
                }

                return new AiTextGenerationResult
                {
                    Text = text.Trim(),
                    Provider = "groq",
                    Model = TryReadString(root, "model") ?? _groqAiOptions.Model,
                    ProviderResponseId = TryReadString(root, "id"),
                    UsageJson = root.TryGetProperty("usage", out var usageElement) ? usageElement.GetRawText() : null
                };
            }
            catch (JsonException exception)
            {
                throw new UserFriendlyException("The AI provider returned an unexpected response payload.", exception);
            }
        }

        /// <summary>
        /// Tries to read the generated output text from the provider payload.
        /// </summary>
        private static string TryReadOutputText(JsonElement root)
        {
            var directOutputText = TryReadString(root, "output_text");
            if (!string.IsNullOrWhiteSpace(directOutputText))
            {
                return directOutputText;
            }

            if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var textBuilder = new StringBuilder();

            foreach (var outputItem in outputElement.EnumerateArray())
            {
                if (!outputItem.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var contentItem in contentElement.EnumerateArray())
                {
                    var text = TryReadString(contentItem, "text");
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textBuilder.AppendLine(text);
                    }
                }
            }

            return textBuilder.Length == 0 ? null : textBuilder.ToString().Trim();
        }

        /// <summary>
        /// Tries to read a string property safely.
        /// </summary>
        private static string TryReadString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var propertyElement) || propertyElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return propertyElement.GetString();
        }

        /// <summary>
        /// Builds a compact provider error message from the failed response body.
        /// </summary>
        private static string BuildProviderErrorMessage(string responseBody, int statusCode)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return "HTTP " + statusCode + ".";
            }

            var compactBody = responseBody.Length <= 300 ? responseBody : responseBody.Substring(0, 300);
            return "HTTP " + statusCode + ": " + compactBody;
        }

        /// <summary>
        /// Validates the required provider configuration before making a request.
        /// </summary>
        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_groqAiOptions.ApiKey))
            {
                throw new UserFriendlyException("Groq AI is not configured. Set AI:Groq:ApiKey before using dataset AI generation.");
            }

            if (string.IsNullOrWhiteSpace(_groqAiOptions.Model))
            {
                throw new UserFriendlyException("Groq AI is not configured. Set AI:Groq:Model before using dataset AI generation.");
            }
        }

        /// <summary>
        /// Builds the configured Groq responses endpoint URL.
        /// </summary>
        private string BuildResponsesEndpoint()
        {
            var baseUrl = string.IsNullOrWhiteSpace(_groqAiOptions.BaseUrl)
                ? "https://api.groq.com/openai/v1/responses"
                : _groqAiOptions.BaseUrl.TrimEnd('/');

            return baseUrl.EndsWith("/responses", StringComparison.OrdinalIgnoreCase)
                ? baseUrl
                : baseUrl + "/responses";
        }

        /// <summary>
        /// Represents the provider request payload.
        /// </summary>
        private class GroqResponsesRequest
        {
            public string Model { get; set; }

            public IReadOnlyList<GroqInputMessage> Input { get; set; }

            public int MaxOutputTokens { get; set; }

            public GroqReasoningRequest Reasoning { get; set; }
        }

        /// <summary>
        /// Represents a provider input message.
        /// </summary>
        private class GroqInputMessage
        {
            public string Role { get; set; }

            public IReadOnlyList<GroqInputContent> Content { get; set; }
        }

        /// <summary>
        /// Represents provider text content.
        /// </summary>
        private class GroqInputContent
        {
            public string Type { get; set; }

            public string Text { get; set; }
        }

        /// <summary>
        /// Represents the optional provider reasoning settings.
        /// </summary>
        private class GroqReasoningRequest
        {
            public string Effort { get; set; }
        }
    }
}
