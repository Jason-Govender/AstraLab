using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abp.UI;
using AstraLab.Services.AI;
using Shouldly;
using Xunit;

namespace AstraLab.Tests.Services.AI
{
    public class GroqAiTextGenerationClient_Tests
    {
        [Fact]
        public async Task GenerateTextAsync_Should_Send_A_Responses_Request_And_Map_The_Result()
        {
            var handler = new CapturingMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"resp_123\",\"model\":\"llama-test\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Dataset looks healthy.\"}]}],\"usage\":{\"output_tokens\":12}}",
                    Encoding.UTF8,
                    "application/json")
            });
            var client = new GroqAiTextGenerationClient(
                new FakeHttpClientFactory(new HttpClient(handler)),
                new GroqAiOptions
                {
                    BaseUrl = "https://api.groq.com/openai/v1",
                    ApiKey = "groq-key",
                    Model = "llama-test",
                    TimeoutSeconds = 30,
                    MaxOutputTokens = 321,
                    ReasoningEffort = "medium"
                });

            var result = await client.GenerateTextAsync(new AiTextGenerationRequest
            {
                SystemInstructions = "System prompt",
                ConversationHistory = new[]
                {
                    new AiConversationHistoryMessage
                    {
                        Role = "assistant",
                        Content = "Previous answer"
                    }
                },
                UserMessage = "Current question"
            });

            handler.RequestUri.AbsoluteUri.ShouldBe("https://api.groq.com/openai/v1/responses");
            handler.AuthorizationHeader.ShouldBe("Bearer groq-key");
            handler.RequestBody.ShouldContain("\"model\":\"llama-test\"");
            handler.RequestBody.ShouldContain("\"max_output_tokens\":321");
            handler.RequestBody.ShouldContain("\"effort\":\"medium\"");
            handler.RequestBody.ShouldContain("System prompt");
            handler.RequestBody.ShouldContain("Previous answer");
            handler.RequestBody.ShouldContain("Current question");

            result.Provider.ShouldBe("groq");
            result.Model.ShouldBe("llama-test");
            result.ProviderResponseId.ShouldBe("resp_123");
            result.Text.ShouldBe("Dataset looks healthy.");
            result.UsageJson.ShouldContain("output_tokens");
        }

        [Fact]
        public async Task GenerateTextAsync_Should_Not_Treat_Reasoning_Text_As_Final_Output_When_OutputText_Is_Missing()
        {
            var handler = new CapturingMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"id\":\"resp_456\",\"model\":\"llama-test\",\"output\":[{\"type\":\"reasoning\",\"content\":[{\"type\":\"reasoning_text\",\"text\":\"Internal reasoning.\"}]},{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Final answer.\"}]}]}",
                    Encoding.UTF8,
                    "application/json")
            });
            var client = new GroqAiTextGenerationClient(
                new FakeHttpClientFactory(new HttpClient(handler)),
                new GroqAiOptions
                {
                    BaseUrl = "https://api.groq.com/openai/v1",
                    ApiKey = "groq-key",
                    Model = "llama-test",
                    TimeoutSeconds = 30,
                    MaxOutputTokens = 321
                });

            var result = await client.GenerateTextAsync(new AiTextGenerationRequest
            {
                SystemInstructions = "System prompt",
                UserMessage = "Current question"
            });

            result.Text.ShouldBe("Final answer.");
        }

        [Fact]
        public async Task GenerateTextAsync_Should_Throw_A_Clear_Exception_When_The_Provider_Fails()
        {
            var handler = new CapturingMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"bad request\"}", Encoding.UTF8, "application/json")
            });
            var client = new GroqAiTextGenerationClient(
                new FakeHttpClientFactory(new HttpClient(handler)),
                new GroqAiOptions
                {
                    BaseUrl = "https://api.groq.com/openai/v1/responses",
                    ApiKey = "groq-key",
                    Model = "llama-test",
                    TimeoutSeconds = 30,
                    MaxOutputTokens = 321
                });

            var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
                client.GenerateTextAsync(new AiTextGenerationRequest
                {
                    SystemInstructions = "System prompt",
                    UserMessage = "Current question"
                }));

            exception.Message.ShouldContain("HTTP 400");
        }

        private class FakeHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _httpClient;

            public FakeHttpClientFactory(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public HttpClient CreateClient(string name)
            {
                return _httpClient;
            }
        }

        private class CapturingMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

            public CapturingMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            public Uri RequestUri { get; private set; }

            public string AuthorizationHeader { get; private set; }

            public string RequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestUri = request.RequestUri;
                AuthorizationHeader = request.Headers.Authorization == null
                    ? null
                    : request.Headers.Authorization.Scheme + " " + request.Headers.Authorization.Parameter;
                RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync();
                return _responseFactory(request);
            }
        }
    }
}
