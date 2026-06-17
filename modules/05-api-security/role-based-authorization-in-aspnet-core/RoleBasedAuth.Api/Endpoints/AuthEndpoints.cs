using Microsoft.AspNetCore.Identity;
using RoleBasedAuth.Api.Auth;
using RoleBasedAuth.Api.Entities;
using RoleBasedAuth.Api.Models;

namespace RoleBasedAuth.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);

        // Only an existing admin is allowed to hand out roles.
        group.MapPost("/add-role", AddRoleAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager)
    {
        // Stop duplicate sign-ups with the same email.
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
            // Identity tells us exactly why (weak password, etc.).
            return Results.BadRequest(result.Errors.Select(e => e.Description));
        }

        // Every new user starts as a regular "User".
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
        var (token, expiresAt) = tokenService.CreateToken(user, roles);

        return Results.Ok(new AuthResponse(user.Id, user.Email!, roles, token, expiresAt));
    }

    private static async Task<IResult> AddRoleAsync(
        AddRoleRequest request,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Results.NotFound($"No user found with email '{request.Email}'.");
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            return Results.BadRequest($"Role '{request.Role}' does not exist.");
        }

        await userManager.AddToRoleAsync(user, request.Role);

        // Important: the user's CURRENT token still carries the old roles.
        // They need to log in again for the new role to take effect.
        return Results.Ok($"Role '{request.Role}' added to '{request.Email}'. Log in again to refresh the token.");
    }
}
