using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class enables you to modify the AppUser entity properties within ASP.NET Identity, e.g. to set your own constraints.
    /// </summary>
    public class User
    {
        [Required]
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "The email is not valid.")]
        public string Email { get; set; }

        [RegularExpression("^[0-9]", ErrorMessage = "The phone number can only be in numbers.")]
        public string Phonenumber { get; set; }
        public string Password { get; set; }

    } // End class.
} // End namespace.