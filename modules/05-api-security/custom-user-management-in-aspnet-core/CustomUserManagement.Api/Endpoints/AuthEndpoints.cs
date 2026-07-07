using CustomUserManagement.Api.Auth;
using CustomUserManagement.Api.Contracts;
using CustomUserManagement.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace CustomUserManagement.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        // Custom registration. This is what MapIdentityApi cannot do: capture FirstName and
        // LastName at sign-up and drop the new user into a default role.
        group.MapPost("/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender) =>
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                // Identity returns exactly which rule failed (password, duplicate email, ...).
                return Results.ValidationProblem(result.ToProblemDictionary());
            }

            // Every new user gets the "User" role by default. Admins are promoted later.
            await userManager.AddToRoleAsync(user, "User");

            // Generate an email confirmation token and "send" it. In production this link goes
            // to the user's inbox; here it is logged so you can confirm the flow.
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await emailSender.SendAsync(user.Email, "Confirm your email",
                $"Confirm with this token: {token}");

            var roles = await userManager.GetRolesAsync(user);
            return Results.Created($"/api/admin/users/{user.Id}", user.ToResponse(roles));
        });

        // Login. Same "keep Identity, issue your own JWT" pattern from the JWT article, with one
        // addition: a disabled (IsActive = false) account is refused even with the right password.
        group.MapPost("/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
            {
                return Results.Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var (token, expiresAt) = tokenService.CreateToken(user, roles);
            return Results.Ok(new LoginResponse(token, expiresAt));
        });

        // Confirm the email using the token from the registration email.
        group.MapPost("/confirm-email", async (
            ConfirmEmailRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.NotFound();
            }

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest("Invalid token.");
        });

        // Start a password reset. Always return 204 so the endpoint cannot be used to probe
        // which emails exist. If the user is real, a reset token is generated and emailed.
        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is not null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await emailSender.SendAsync(user.Email!, "Reset your password",
                    $"Reset with this token: {token}");
            }

            return Results.NoContent();
        });

        // Complete the reset with the token + a new password.
        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.NotFound();
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            return result.Succeeded
                ? Results.NoContent()
                : Results.ValidationProblem(result.ToProblemDictionary());
        });
    }
}
