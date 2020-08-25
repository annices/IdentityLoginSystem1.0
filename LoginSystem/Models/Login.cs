using System.ComponentModel.DataAnnotations;

namespace LoginSystem.Models
{
    /// <summary>
    /// This class enables you to customize the Login entity within the ASP.NET Identity framework.
    /// </summary>
    public class Login
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
