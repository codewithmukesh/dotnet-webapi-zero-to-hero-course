using System.Text;
using ClaimsBasedAuth.Api.Auth;
using ClaimsBasedAuth.Api.Data;
using ClaimsBasedAuth.Api.Endpoints;
using ClaimsBasedAuth.Api.Entities;
using ClaimsBasedAuth.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Read the "JwtSettings" section into a strongly-typed object.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// 2. EF Core + ASP.NET Core Identity. InMemory keeps this sample runnable with zero database setup.
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ClaimsBasedAuthDb"));
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

// 4. Authorization + claims transformation (enriches the principal on every request).
builder.Services.AddAuthorization();
builder.Services.AddTransient<IClaimsTransformation, SubscriptionClaimsTransformation>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<SubscriptionStore>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the roles, users, department claims, and subscription tiers.
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
app.MapClaimsEndpoints();

app.Run();
