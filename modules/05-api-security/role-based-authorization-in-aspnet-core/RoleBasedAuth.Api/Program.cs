using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RoleBasedAuth.Api.Auth;
using RoleBasedAuth.Api.Data;
using RoleBasedAuth.Api.Endpoints;
using RoleBasedAuth.Api.Entities;
using RoleBasedAuth.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Read the "JwtSettings" section into a strongly-typed object.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// 2. EF Core + ASP.NET Core Identity. InMemory keeps this sample runnable with zero database setup.
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("RoleBasedAuthDb"));
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
            // RoleClaimType must match the claim name TokenService writes roles under.
            // If these two disagree, every role check fails with 403.
            RoleClaimType = "role"
        };
    });

// 4. Authorization. The "ManagerOnly" named policy is reused by any endpoint that needs it.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ManagerOnly", policy => policy.RequireRole(Roles.Manager));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<ProductStore>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the roles and the three test users so you can log in right away.
await DbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Order matters: authenticate first (who are you?), then authorize (are you allowed?).
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProductEndpoints();

app.Run();
