using System.Security.Claims;

namespace EfCoreInterceptors.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        // Demo convenience: this sample has no authentication wired up, so it
        // falls back to an X-User-Id request header. That lets you watch the
        // audit fields populate. In a real app the claim above is enough.
        ?? accessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
}
