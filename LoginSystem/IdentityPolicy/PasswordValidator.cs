using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace LoginSystem.IdentityPolicy
{
    /// <summary>
    /// This class enables you to customize the password policy for users, which is registered in Startup.cs.
    /// You can modify this class further to suit your own needs.
    /// </summary>
    public class PasswordPolicy : PasswordValidator<AppUser>
    {
        public override async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            IdentityResult result = await base.ValidateAsync(manager, user, password);
            List<IdentityError> errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            if (password.Contains("123"))
            {
                errors.Add(new IdentityError
                {
                    Description = "The password cannot contain a numeric sequence like '123'."
                });
            }
            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }

    } // End class.
} // End namespace.
