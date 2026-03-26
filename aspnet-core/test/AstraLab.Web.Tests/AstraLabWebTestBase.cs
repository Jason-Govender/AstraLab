using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.AspNetCore.TestBase;
using Abp.Json;
using Abp.Web.Models;
using AstraLab.Models.TokenAuth;
using AstraLab.Sessions.Dto;
using AstraLab.Web.Host.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace AstraLab.Web.Tests
{
    public abstract class AstraLabWebTestBase : AbpAspNetCoreIntegratedTestBase<Startup>
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            Environment.SetEnvironmentVariable("App__CorsOrigins", "http://localhost:3000,https://app.example.com");

            return base
                .CreateWebHostBuilder()
                .UseEnvironment(Environments.Development)
                .UseContentRoot(GetHostProjectPath())
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(AstraLab.Web.Host.Startup.Startup).Assembly.FullName);
        }

        protected async Task<T> GetResponseAsObjectAsync<T>(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            HttpRequestMessage request = null)
        {
            var strResponse = await GetResponseAsStringAsync(url, expectedStatusCode, request);
            return JsonSerializer.Deserialize<T>(strResponse, JsonSerializerOptions);
        }

        protected async Task<string> GetResponseAsStringAsync(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            HttpRequestMessage request = null)
        {
            var response = await GetResponseAsync(url, expectedStatusCode, request);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(
            string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            HttpRequestMessage request = null)
        {
            var httpRequest = request ?? new HttpRequestMessage(HttpMethod.Get, url);
            if (httpRequest.RequestUri == null)
            {
                httpRequest.RequestUri = new Uri(url, UriKind.Relative);
            }

            var response = await Client.SendAsync(httpRequest);
            if (expectedStatusCode != 0)
            {
                response.StatusCode.ShouldBe(expectedStatusCode);
            }

            return response;
        }

        protected async Task<string> GetAntiForgeryTokenAsync(string origin = "http://localhost:3000")
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/AntiForgery/GetToken");
            request.Headers.Add("Origin", origin);

            var response = await GetResponseAsync("/AntiForgery/GetToken", HttpStatusCode.OK, request);
            return response.Headers.GetValues(AstraLabHostHttpSecurity.AntiForgeryHeaderName).Single();
        }

        protected async Task<AuthenticateResultModel> AuthenticateAsync(
            AuthenticateModel input,
            string origin = "http://localhost:3000",
            int? tenantId = 1)
        {
            var antiForgeryToken = await GetAntiForgeryTokenAsync(origin);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/TokenAuth/Authenticate");
            request.Headers.Add("Origin", origin);
            request.Headers.Add(AstraLabHostHttpSecurity.AntiForgeryHeaderName, antiForgeryToken);

            if (tenantId.HasValue)
            {
                request.Headers.Add("Abp.TenantId", tenantId.Value.ToString());
            }

            request.Content = new StringContent(input.ToJsonString(), Encoding.UTF8, "application/json");

            var response = await GetResponseAsync("/api/TokenAuth/Authenticate", HttpStatusCode.OK, request);
            var payload = JsonSerializer.Deserialize<AjaxResponse<AuthenticateResultModel>>(
                await response.Content.ReadAsStringAsync(),
                JsonSerializerOptions);

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.Result.AccessToken);

            if (tenantId.HasValue)
            {
                Client.DefaultRequestHeaders.Remove("Abp.TenantId");
                Client.DefaultRequestHeaders.Add("Abp.TenantId", tenantId.Value.ToString());
            }

            return payload.Result;
        }

        protected async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync()
        {
            var payload = await GetResponseAsObjectAsync<AjaxResponse<GetCurrentLoginInformationsOutput>>(
                "/api/services/app/Session/GetCurrentLoginInformations");

            return payload.Result;
        }

        private static string GetHostProjectPath()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "..",
                    "src",
                    "AstraLab.Web.Host"));
        }
    }
}
