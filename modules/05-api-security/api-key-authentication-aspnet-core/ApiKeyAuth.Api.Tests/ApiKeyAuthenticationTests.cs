using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiKeyAuth.Api.Authentication;
using ApiKeyAuth.Api.Data;
using ApiKeyAuth.Api.Endpoints;
using ApiKeyAuth.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApiKeyAuth.Api.Tests;

public sealed class ApiKeyAuthenticationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ApiKeyAuthenticationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static HttpRequestMessage RequestWithKey(HttpMethod method, string path, string? key)
    {
        var request = new HttpRequestMessage(method, path);
        if (key is not null)
            request.Headers.Add(ApiKeyAuthenticationOptions.HeaderName, key);
        return request;
    }

    private async Task<string> SeedKeyAsync(
        string ownerId = "test-client",
        string scopes = "keys:read,keys:admin",
        DateTime? expiresAt = null,
        DateTime? revokedAt = null)
    {
        var plaintext = ApiKeyGenerator.Generate("sk_live_");
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ApiKeys.Add(new ApiKey
        {
            Prefix = plaintext[..12],
            KeyHash = ApiKeyHasher.Hash(plaintext),
            Name = "Test Key",
            OwnerId = ownerId,
            Scopes = scopes,
            CreatedAt = _factory.TimeProvider.GetUtcNow().UtcDateTime,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt
        });
        await db.SaveChangesAsync();
        return plaintext;
    }

    [Fact]
    public async Task Public_endpoint_does_not_require_a_key()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Secure_endpoint_returns_401_when_header_missing()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Secure_endpoint_returns_401_when_key_invalid()
    {
        var client = _factory.CreateClient();
        var response = await client.SendAsync(RequestWithKey(HttpMethod.Get, "/secure", "sk_live_definitely_not_real"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Secure_endpoint_returns_200_with_valid_key()
    {
        var key = await SeedKeyAsync();
        var client = _factory.CreateClient();
        var response = await client.SendAsync(RequestWithKey(HttpMethod.Get, "/secure", key));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Revoked_key_is_rejected()
    {
        var key = await SeedKeyAsync(
            revokedAt: _factory.TimeProvider.GetUtcNow().UtcDateTime);

        var client = _factory.CreateClient();
        var response = await client.SendAsync(RequestWithKey(HttpMethod.Get, "/secure", key));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Expired_key_is_rejected()
    {
        var pastExpiry = _factory.TimeProvider.GetUtcNow().AddDays(-1).UtcDateTime;
        var key = await SeedKeyAsync(expiresAt: pastExpiry);

        var client = _factory.CreateClient();
        var response = await client.SendAsync(RequestWithKey(HttpMethod.Get, "/secure", key));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Issue_endpoint_returns_plaintext_key_once()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/admin/keys/", new IssueApiKeyRequest(
            Name: "Webhook receiver",
            OwnerId: "partner-acme",
            Scopes: ["keys:read"],
            TtlDays: 30));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IssueApiKeyResponse>();
        Assert.NotNull(body);
        Assert.StartsWith("sk_live_", body!.PlaintextKey);
        Assert.Equal(12, body.Prefix.Length);
        Assert.NotNull(body.ExpiresAt);
    }

    [Fact]
    public async Task Stored_key_is_a_hash_not_plaintext()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/admin/keys/", new IssueApiKeyRequest(
            Name: "Hash check key",
            OwnerId: "partner-test",
            Scopes: [],
            TtlDays: null));

        var body = await response.Content.ReadFromJsonAsync<IssueApiKeyResponse>();
        Assert.NotNull(body);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.ApiKeys.SingleAsync(k => k.Id == body!.Id);

        Assert.NotEqual(body!.PlaintextKey, stored.KeyHash);
        Assert.Equal(64, stored.KeyHash.Length);
        Assert.Equal(ApiKeyHasher.Hash(body.PlaintextKey), stored.KeyHash);
    }

    [Fact]
    public async Task List_endpoint_requires_keys_read_scope()
    {
        var keyWithoutScope = await SeedKeyAsync(scopes: string.Empty);
        var client = _factory.CreateClient();

        var response = await client.SendAsync(
            RequestWithKey(HttpMethod.Get, "/admin/keys/", keyWithoutScope));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_endpoint_succeeds_with_keys_read_scope()
    {
        var key = await SeedKeyAsync(scopes: "keys:read");
        var client = _factory.CreateClient();

        var response = await client.SendAsync(
            RequestWithKey(HttpMethod.Get, "/admin/keys/", key));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
