using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomJwt.Api.Data;

// Same Identity store as the MapIdentityApi project. The only difference is how we
// expose login and what token we hand back - not the user storage underneath.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options);
