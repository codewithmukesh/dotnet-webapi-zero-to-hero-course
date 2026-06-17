using IdentityEndpoints.Api.Data;
using IdentityEndpoints.Api.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core Identity store. InMemory keeps this sample F5-ready with zero database setup.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("IdentityEndpointsDb"));

builder.Services.AddAuthorization();

// THE one call. AddIdentityApiEndpoints (NOT the older AddIdentity) registers the
// Identity API endpoint services and the bearer-token handling at the same time.
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Maps the ten endpoints: /register, /login, /refresh, /confirmEmail,
// /resendConfirmationEmail, /forgotPassword, /resetPassword,
// /manage/2fa, GET /manage/info, POST /manage/info.
app.MapIdentityApi<IdentityUser>();

// A protected resource to prove the token works. RequireAuthorization() here accepts
// either the auth cookie (login with ?useCookies=true) or the opaque bearer token.
app.MapProductEndpoints();

app.Run();
