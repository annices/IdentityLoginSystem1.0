using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class handles the base database context for the application.
    /// </summary>
    public class LoginSystemDbContext : IdentityDbContext<AppUser>
    {
        public LoginSystemDbContext(DbContextOptions<LoginSystemDbContext> options) : base(options) { }
    }
}
