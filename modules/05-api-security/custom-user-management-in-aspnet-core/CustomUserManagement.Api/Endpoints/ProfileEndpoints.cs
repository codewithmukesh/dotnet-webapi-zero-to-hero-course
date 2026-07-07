using System.Security.Claims;
using CustomUserManagement.Api.Auth;
using CustomUserManagement.Api.Contracts;
using CustomUserManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CustomUserManagement.Api.Endpoints;

// The self-service side. Any logged-in user can read and edit their OWN profile - no admin
// role needed. The user id comes from the JWT, never from the request body, so a user can
// only ever touch their own account.
public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me")
            .WithTags("My Profile")
            .RequireAuthorization();

        // GET /api/me
        group.MapGet("/", async (ClaimsPrincipal principal, UserManager<ApplicationUser> userManager) =>
        {
            var user = await GetCurrentUserAsync(principal, userManager);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            return Results.Ok(user.ToResponse(roles));
        });

        // PUT /api/me - update my own name and avatar.
        group.MapPut("/", async (
            UpdateProfileRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await GetCurrentUserAsync(principal, userManager);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            return Results.Ok(user.ToResponse(roles));
        });

        // POST /api/me/change-password - ChangePasswordAsync checks the current password for you.
        group.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await GetCurrentUserAsync(principal, userManager);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var result = await userManager.ChangePasswordAsync(
                user, request.CurrentPassword, request.NewPassword);

            return result.Succeeded
                ? Results.NoContent()
                : Results.ValidationProblem(result.ToProblemDictionary());
        });
    }

    // The JWT stores the user id in the "sub" claim. Read it and load the user - this is how
    // an endpoint knows "who is calling" without trusting anything in the request body.
    private static async Task<ApplicationUser?> GetCurrentUserAsync(
        ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
    {
        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return userId is null ? null : await userManager.FindByIdAsync(userId);
    }
}
