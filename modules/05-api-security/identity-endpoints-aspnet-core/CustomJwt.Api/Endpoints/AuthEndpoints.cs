using CustomJwt.Api.Auth;
using CustomJwt.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace CustomJwt.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // The escape hatch: keep ASP.NET Core Identity for the user store, password
        // hashing, and lockout, but issue your OWN signed JWT instead of the opaque
        // token MapIdentityApi would hand back.
        app.MapPost("/auth/login", async (
            LoginRequest request,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            TokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            // CheckPasswordSignInAsync verifies the hash and applies lockout for you.
            var result = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var (token, expiresAt) = tokenService.CreateToken(user, roles);

            return Results.Ok(new LoginResponse(token, expiresAt));
        })
        .WithTags("Auth");
    }
}
