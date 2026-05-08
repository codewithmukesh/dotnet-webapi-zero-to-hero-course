using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiKeyAuth.Api.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace ApiKeyAuth.Api.Authentication;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    IApiKeyValidator validator)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, loggerFactory, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                ApiKeyAuthenticationOptions.HeaderName, out var providedKey))
        {
            return AuthenticateResult.NoResult();
        }

        var key = providedKey.ToString();
        var result = await validator.ValidateAsync(key, Context.RequestAborted);

        if (!result.IsValid)
        {
            Logger.LogWarning(
                "API key authentication failed for prefix {Prefix}: {Reason}",
                result.Prefix ?? "(none)", result.Reason);
            return AuthenticateResult.Fail(result.Reason ?? "Invalid API key.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.KeyId!.Value.ToString()),
            new(ClaimTypes.Name, result.Name!),
            new("client_id", result.OwnerId!),
            new("api_key_prefix", result.Prefix!)
        };
        claims.AddRange(result.Scopes.Select(s => new Claim("scope", s)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = $"A valid API key is required. Send it in the {ApiKeyAuthenticationOptions.HeaderName} header.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
        };

        await Results.Problem(problemDetails).ExecuteAsync(Context);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "The API key is valid, but it does not have permission for this resource.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4"
        };

        await Results.Problem(problemDetails).ExecuteAsync(Context);
    }
}
