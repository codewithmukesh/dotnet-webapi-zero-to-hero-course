using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolicyBasedAuth.Api.Entities;

namespace PolicyBasedAuth.Api.Data;

// IdentityDbContext brings in all the Identity tables (users, roles, claims, etc.).
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);
