using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class defines the properties used to update permission roles for users.
    /// </summary>
    public class UserRole
    {
        [Required]
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string[] AddIds { get; set; }
        public string[] DeleteIds { get; set; }
    }
}
