using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class defines the password reset properties.
    /// </summary>
    public class PasswordReset
    {

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; }

    }
}
