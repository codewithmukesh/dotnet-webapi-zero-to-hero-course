using Microsoft.AspNetCore.Identity;
using PolicyBasedAuth.Api.Auth;
using PolicyBasedAuth.Api.Entities;
using PolicyBasedAuth.Api.Models;

namespace PolicyBasedAuth.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // The app has a FALLBACK policy requiring authentication everywhere -
        // so the auth endpoints must explicitly opt OUT.
        var group = app.MapGroup("/api/auth").WithTags("Auth").AllowAnonymous();

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Results.BadRequest($"Email '{request.Email}' is already registered.");
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors.Select(e => e.Description));
        }

        await userManager.AddToRoleAsync(user, Roles.User);

        return Results.Ok($"User '{request.Email}' registered successfully.");
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Same response for "no such user" and "wrong password" so we don't leak which emails exist.
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var userClaims = await userManager.GetClaimsAsync(user);

        var (token, expiresAt) = tokenService.CreateToken(user, roles, userClaims);

        return Results.Ok(new AuthResponse(user.Id, user.Email!, roles, token, expiresAt));
    }
}
