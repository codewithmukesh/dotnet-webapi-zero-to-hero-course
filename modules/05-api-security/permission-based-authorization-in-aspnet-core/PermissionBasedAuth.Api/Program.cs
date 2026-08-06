using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PermissionBasedAuth.Api.Auth;
using PermissionBasedAuth.Api.Authorization;
using PermissionBasedAuth.Api.Data;
using PermissionBasedAuth.Api.Endpoints;
using PermissionBasedAuth.Api.Entities;
using PermissionBasedAuth.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Strongly-typed settings.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<PermissionSettings>(builder.Configuration.GetSection("Permissions"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// 2. EF Core + ASP.NET Core Identity. InMemory keeps this sample runnable with zero database setup.
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("PermissionBasedAuthDb"));
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options => options.User.RequireUniqueEmail = true)
    .AddEntityFrameworkStores<AppDbContext>();

// 3. Authenticate requests using JWT bearer tokens.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Keep the claim names exactly as they appear in the token (no surprise remapping).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };
    });

// 4. Authorization. Notice there is NOT a single AddPolicy call for permissions -
// PermissionPolicyProvider manufactures those on demand.
builder.Services.AddAuthorizationBuilder()
    // Secure by default: every endpoint requires an authenticated user
    // unless it explicitly opts out with AllowAnonymous.
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// The policy provider MUST be a singleton - ASP.NET Core resolves exactly one.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<PermissionStore>();

// TokenService is registered concretely as well so the diagnostics probe can
// reuse the exact same signing path a real login takes.
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ITokenService>(sp => sp.GetRequiredService<TokenService>());

builder.Services.AddSingleton<ProductStore>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the roles, the role permissions, and the three demo users.
await DbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    // The fallback policy applies to ALL endpoints - the docs UI must opt out too,
    // or the browser gets a 401 instead of Scalar.
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

// Order matters: authenticate first (who are you?), then authorize (are you allowed?).
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapAdminPermissionEndpoints();
app.MapDiagnosticsEndpoints();

app.Run();
