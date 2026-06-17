using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PolicyBasedAuth.Api.Authorization;

namespace PolicyBasedAuth.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        // No RequireAuthorization() needed here - the FALLBACK policy in Program.cs
        // already demands an authenticated user for every endpoint by default.
        var group = app.MapGroup("/api").WithTags("Documents");

        // Named policy with a custom requirement evaluated by TWO handlers (OR):
        // premium subscribers pass via PremiumSubscriptionHandler,
        // admins pass via AdminOverrideHandler.
        group.MapGet("/insights", (ClaimsPrincipal user) =>
                Results.Ok($"Pro insights for {user.Identity?.Name}."))
            .RequireAuthorization("ProAccess");

        // Named policy whose requirement carries data (start/end hours) and
        // whose handler takes a DI dependency (TimeProvider).
        group.MapPost("/documents", () =>
                Results.Ok("Document published."))
            .RequireAuthorization("WorkingHoursOnly");

        // IAuthorizationRequirementData: the attribute IS the policy.
        // No registration in Program.cs - the months value travels on the attribute.
        group.MapGet("/veterans-lounge", () =>
                Results.Ok("Welcome, long-time member."))
            .RequireAuthorization(new MinimumTenureAttribute(6));

        // Imperative authorization: evaluate a policy mid-endpoint with
        // IAuthorizationService when you need to control the failure response.
        group.MapDelete("/documents/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            IAuthorizationService authorizationService) =>
        {
            var result = await authorizationService.AuthorizeAsync(user, "WorkingHoursOnly");

            if (!result.Succeeded)
            {
                // A plain policy failure would be a bare 403 - this one explains itself.
                return Results.Problem(
                    title: "Outside working hours",
                    detail: "Documents can only be deleted between 09:00 and 18:00 UTC.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            return Results.Ok($"Document {id} deleted.");
        });
    }
}
