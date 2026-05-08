using ApiKeyAuth.Api.Authentication;
using ApiKeyAuth.Api.Data;
using ApiKeyAuth.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiKeyAuth.Api.Endpoints;

public sealed record IssueApiKeyRequest(string Name, string OwnerId, string[] Scopes, int? TtlDays);

public sealed record IssueApiKeyResponse(
    Guid Id, string Name, string PlaintextKey, string Prefix, DateTime? ExpiresAt);

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/keys").WithTags("Admin");

        group.MapPost("/", IssueAsync);
        group.MapGet("/", ListAsync).RequireAuthorization("keys:read");
        group.MapDelete("/{id:guid}", RevokeAsync).RequireAuthorization("keys:admin");
        group.MapPost("/{id:guid}/rotate", RotateAsync).RequireAuthorization("keys:admin");

        return app;
    }

    private static async Task<IResult> IssueAsync(
        IssueApiKeyRequest request,
        AppDbContext db,
        TimeProvider time,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (string.IsNullOrWhiteSpace(request.OwnerId))
            return Results.BadRequest(new { error = "OwnerId is required." });

        var plaintext = ApiKeyGenerator.Generate("sk_live_");

        var entity = new ApiKey
        {
            Prefix = plaintext[..12],
            KeyHash = ApiKeyHasher.Hash(plaintext),
            Name = request.Name,
            OwnerId = request.OwnerId,
            Scopes = string.Join(',', request.Scopes ?? []),
            CreatedAt = time.GetUtcNow().UtcDateTime,
            ExpiresAt = request.TtlDays is { } days
                ? time.GetUtcNow().AddDays(days).UtcDateTime
                : null
        };

        db.ApiKeys.Add(entity);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/admin/keys/{entity.Id}",
            new IssueApiKeyResponse(entity.Id, entity.Name, plaintext, entity.Prefix, entity.ExpiresAt));
    }

    private static async Task<IResult> ListAsync(AppDbContext db, CancellationToken ct)
        => Results.Ok(await db.ApiKeys.AsNoTracking()
            .Select(k => new
            {
                k.Id, k.Prefix, k.Name, k.OwnerId, k.Scopes,
                k.CreatedAt, k.ExpiresAt, k.RevokedAt, k.LastUsedAt
            })
            .ToListAsync(ct));

    private static async Task<IResult> RevokeAsync(
        Guid id, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var rows = await db.ApiKeys
            .Where(k => k.Id == id && k.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(
                k => k.RevokedAt, time.GetUtcNow().UtcDateTime), ct);

        return rows == 0 ? Results.NotFound() : Results.NoContent();
    }

    private static async Task<IResult> RotateAsync(
        Guid id, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var existing = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (existing is null) return Results.NotFound();

        var plaintext = ApiKeyGenerator.Generate("sk_live_");
        var newKey = new ApiKey
        {
            Prefix = plaintext[..12],
            KeyHash = ApiKeyHasher.Hash(plaintext),
            Name = $"{existing.Name} (rotated {time.GetUtcNow():yyyy-MM-dd})",
            OwnerId = existing.OwnerId,
            Scopes = existing.Scopes,
            CreatedAt = time.GetUtcNow().UtcDateTime,
            ExpiresAt = existing.ExpiresAt
        };
        db.ApiKeys.Add(newKey);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/admin/keys/{newKey.Id}",
            new IssueApiKeyResponse(newKey.Id, newKey.Name, plaintext, newKey.Prefix, newKey.ExpiresAt));
    }
}
