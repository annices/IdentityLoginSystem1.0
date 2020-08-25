using Microsoft.AspNetCore.Identity;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class enables you to add properties to the AppUser entity within ASP.NET Identity.
    /// The default properties included in the ASP.NET Identity framework 3.0 are:
    /// Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
    /// SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
    /// LockoutEnd, LockoutEnabled, AccessFailedCount.
    /// </summary>
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Created { get; set; }

        public string Updated { get; set; }

    } // End class.
} // End namespace.