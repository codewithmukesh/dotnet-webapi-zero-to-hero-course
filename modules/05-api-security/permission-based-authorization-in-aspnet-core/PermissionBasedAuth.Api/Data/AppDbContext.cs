using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PermissionBasedAuth.Api.Entities;

namespace PermissionBasedAuth.Api.Data;

// IdentityDbContext brings in all the Identity tables, including AspNetRoleClaims -
// which is where the permissions actually live.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);
