using CustomUserManagement.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomUserManagement.Api.Data;

// IdentityDbContext<ApplicationUser> wires our custom user into the Identity tables.
// Because ApplicationUser inherits IdentityUser, the extra columns (FirstName, IsActive, ...)
// land in the AspNetUsers table automatically when you add a migration.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);
