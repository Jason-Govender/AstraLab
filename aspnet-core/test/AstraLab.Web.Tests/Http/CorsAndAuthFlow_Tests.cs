using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AstraLab.Models.TokenAuth;
using Shouldly;
using Xunit;

namespace AstraLab.Web.Tests.Http
{
    public class CorsAndAuthFlow_Tests : AstraLabWebTestBase
    {
        [Theory]
        [InlineData("http://localhost:3000")]
        [InlineData("https://app.example.com")]
        public async Task Preflight_Should_Allow_Configured_Origins(string origin)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/TokenAuth/Authenticate");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type,x-xsrf-token,abp.tenantid");

            var response = await Client.SendAsync(request);

            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            response.Headers.GetValues("Access-Control-Allow-Origin").Single().ShouldBe(origin);
            response.Headers.GetValues("Access-Control-Allow-Credentials").Single().ShouldBe("true");
            response.Headers.GetValues("Access-Control-Allow-Headers").Single()
                .ShouldContain("x-xsrf-token", Case.Insensitive);
            response.Headers.GetValues("Access-Control-Allow-Headers").Single()
                .ShouldContain("abp.tenantid", Case.Insensitive);
            response.Headers.GetValues("Access-Control-Allow-Methods").Single()
                .ShouldContain("POST", Case.Insensitive);
        }

        [Fact]
        public async Task AntiForgery_Bootstrap_Should_Expose_Header_And_Cross_Site_Cookie_Settings()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/AntiForgery/GetToken");
            request.Headers.Add("Origin", "http://localhost:3000");

            var response = await Client.SendAsync(request);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Headers.GetValues("Access-Control-Allow-Origin").Single().ShouldBe("http://localhost:3000");
            response.Headers.GetValues("Access-Control-Expose-Headers").Single()
                .ShouldContain("X-XSRF-TOKEN", Case.Insensitive);
            response.Headers.GetValues("X-XSRF-TOKEN").Single().ShouldNotBeNullOrWhiteSpace();

            var setCookie = response.Headers.GetValues("Set-Cookie").Single();
            setCookie.ShouldContain("XSRF-TOKEN=");
            setCookie.ShouldContain("SameSite=None");
            setCookie.ShouldContain("secure", Case.Insensitive);
        }

        [Fact]
        public async Task Authenticate_And_Call_Protected_Endpoint_Should_Succeed()
        {
            var authResult = await AuthenticateAsync(new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            authResult.AccessToken.ShouldNotBeNullOrWhiteSpace();

            var currentLogin = await GetCurrentLoginInformationsAsync();
            currentLogin.User.ShouldNotBeNull();
            currentLogin.User.Name.ShouldBe("admin");
            currentLogin.Tenant.ShouldNotBeNull();
            currentLogin.Tenant.Name.ShouldBe("Default");
        }
    }
}
