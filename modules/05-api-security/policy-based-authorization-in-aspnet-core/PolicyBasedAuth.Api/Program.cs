using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PolicyBasedAuth.Api.Auth;
using PolicyBasedAuth.Api.Authorization;
using PolicyBasedAuth.Api.Data;
using PolicyBasedAuth.Api.Endpoints;
using PolicyBasedAuth.Api.Entities;
using PolicyBasedAuth.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Read the "JwtSettings" section into a strongly-typed object.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// 2. EF Core + ASP.NET Core Identity. InMemory keeps this sample runnable with zero database setup.
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("PolicyBasedAuthDb"));
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

// 4. Policies. Each policy = one or more requirements. Logic lives in the handlers below.
builder.Services.AddAuthorizationBuilder()
    // RequireAuthenticatedUser is NOT implied by custom requirements - without it,
    // a rule like working hours would pass for anonymous callers too.
    .AddPolicy("ProAccess", policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new ProUserRequirement()))
    .AddPolicy("WorkingHoursOnly", policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new WorkingHoursRequirement(9, 18)))
    // Secure by default: every endpoint requires an authenticated user
    // unless it explicitly opts out with AllowAnonymous.
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// 5. Handlers - registered in DI, discovered by the authorization service.
// TWO handlers for ProUserRequirement = OR semantics.
builder.Services.AddSingleton<IAuthorizationHandler, PremiumSubscriptionHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AdminOverrideHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, WorkingHoursHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MinimumTenureHandler>();
builder.Services.AddSingleton(TimeProvider.System);

// 6. Claims transformation (subscription tier) - carried over from the claims article.
builder.Services.AddTransient<IClaimsTransformation, SubscriptionClaimsTransformation>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<SubscriptionStore>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed the roles, users, claims, and subscription tiers.
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
app.MapDocumentEndpoints();

app.Run();
