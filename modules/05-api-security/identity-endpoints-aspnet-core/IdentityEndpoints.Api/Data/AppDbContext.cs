using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityEndpoints.Api.Data;

// IdentityDbContext brings in all the Identity tables (users, roles, claims, tokens).
// IdentityUser is the default user - email, password hash, security stamp, and the rest.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options);
