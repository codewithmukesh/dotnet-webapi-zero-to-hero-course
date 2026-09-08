using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cors.Api.Tests;

/// <summary>
/// Pins the environment to Production so the tests read the deployed origin list
/// from appsettings.json (app.acme.com + admin.acme.com) rather than the localhost
/// list in appsettings.Development.json.
/// </summary>
public sealed class CorsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Production");
}

public sealed class CorsPolicyTests(CorsApiFactory factory) : IClassFixture<CorsApiFactory>
{
    private const string AllowedOrigin = "https://app.acme.com";
    private const string SecondAllowedOrigin = "https://admin.acme.com";
    private const string UnknownOrigin = "https://evil.example";

    private static HttpRequestMessage Preflight(string path, string origin, string method = "POST")
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        request.Headers.Add("Access-Control-Request-Headers", "authorization, content-type");
        return request;
    }

    [Fact]
    public async Task Preflight_FromAllowedOrigin_Returns204WithAllowOrigin()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("/api/products", AllowedOrigin), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AllowedOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task Preflight_FromUnknownOrigin_OmitsAllowOriginHeader()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("/api/products", UnknownOrigin), TestContext.Current.CancellationToken);

        // The middleware still short-circuits the OPTIONS request, but writes no CORS
        // headers - which is what the browser turns into "No 'Access-Control-Allow-Origin'
        // header is present on the requested resource."
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_EchoesConfiguredMaxAge()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("/api/products", AllowedOrigin), TestContext.Current.CancellationToken);

        Assert.Equal("600", response.Headers.GetValues("Access-Control-Max-Age").Single());
    }

    [Fact]
    public async Task Preflight_WithCredentialedPolicy_AllowsCredentials()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("/api/products", AllowedOrigin), TestContext.Current.CancellationToken);

        Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
    }

    [Theory]
    [InlineData(AllowedOrigin)]
    [InlineData(SecondAllowedOrigin)]
    public async Task Response_IncludesVaryOrigin_WhenPolicyListsMoreThanOneOrigin(string origin)
    {
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        request.Headers.Add("Origin", origin);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // AcmeFrontend lists two origins, so CorsService sets VaryByOrigin and the
        // middleware emits Vary: Origin. Without this header a shared cache or CDN
        // would serve one origin's Access-Control-Allow-Origin value to the other.
        Assert.Contains("Origin", response.Headers.Vary);
        Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task Get_ExposesCustomResponseHeadersToTheBrowser()
    {
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        request.Headers.Add("Origin", AllowedOrigin);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        var exposed = response.Headers.GetValues("Access-Control-Expose-Headers").Single();
        Assert.Contains("X-Total-Count", exposed);
        Assert.True(response.Headers.Contains("X-Total-Count"));
    }

    [Fact]
    public async Task WebhookEndpoint_AllowsItsOwnOrigin_AndRejectsTheFrontendOrigin()
    {
        using var client = factory.CreateClient();

        var allowed = await client.SendAsync(Preflight("/webhooks/stripe", "https://stripe.com"), TestContext.Current.CancellationToken);
        var rejected = await client.SendAsync(Preflight("/webhooks/stripe", AllowedOrigin), TestContext.Current.CancellationToken);

        // The endpoint-specific PublicWebhooks policy wins over the global one.
        Assert.Equal("https://stripe.com", allowed.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(rejected.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
