using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class defines the properties used to display which users that are members/nonmembers of a specific role.
    /// </summary>
    public class RoleMembers
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }
}
