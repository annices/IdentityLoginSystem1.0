using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

/*
Copyright © 2020 Annice Strömberg – Annice.se
This script is MIT (Massachusetts Institute of Technology) licensed, which means that
permission is granted, free of charge, to any person obtaining a copy of this software
and associated documentation files to deal in the software without restriction. This
includes, without limitation, the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the software, and to permit persons to whom the software
is furnished to do so subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the software.
*/
namespace LoginSystem.Controllers
{
    /// <summary>
    /// This controller handles different user actions such as to login/logout and to reset an account password.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        /// <summary>
        /// This constructor injects some ASP.NET Identity dependencies to enable some
        /// of their functions once this controller is called.
        /// </summary>
        /// <param name="userMgr">UserManager class.</param>
        /// <param name="signinMgr">SignInManager class.</param>
        /// <param name="emailSndr">IEmailSender interface.</param>
        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr, IEmailSender emailSndr, IConfiguration config)
        {
            _userManager = userMgr;
            _signInManager = signinMgr;
            _emailSender = emailSndr;
            _config = config;
        }

        /// <summary>
        /// This action renders the page to be able to login a user.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>The login page.</returns>
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            Login login = new Login();
            login.ReturnUrl = returnUrl;
            return View(login);
        }

        /// <summary>
        /// This task handles the action when a user has been requested to be logged in,
        /// i.e. when the login form has been submitted.
        /// </summary>
        /// <param name="login"></param>
        /// <returns>The page on a successful login, or error feedback on login failure.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            // Prepare feedback message on failure:
            string errormsg = "Login failed: Invalid email or password.";

            if (ModelState.IsValid && login != null)
            {
                AppUser appUser = await _userManager.FindByEmailAsync(login.Email);

                if (appUser != null && login.Password != null)
                {
                    await _signInManager.SignOutAsync();

                    // Set 3rd and 4th PasswordSignInAsync param to true if you want the user to stay logged in after closed browser:
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, login.Password, false, false);

                    if (result.Succeeded)
                    {
                        HttpContext.Session.SetString("userid", appUser.Id);
                        IList<string> loggedInUserRoles = await _userManager.GetRolesAsync(appUser);
                        TempData["hasRoles"] = loggedInUserRoles.Any();

                        return Redirect(login.ReturnUrl ?? "/");
                    }
                    else
                        TempData["error"] = errormsg;
                }
                else
                    TempData["error"] = errormsg;
            }
            else
                ModelState.AddModelError(nameof(login.Email), errormsg);

            return View(login);
        }

        /// <summary>
        /// This task handles the action when a user has been requested to be logged out,
        /// i.e. when the logout link has been clicked.
        /// </summary>
        /// <returns>The page when a user has been logged out.</returns>
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Remove("hasRoles");
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// This action renders the page to be able to send a reset password URL to a user.
        /// </summary>
        /// <returns>The page to send a reset password URL.</returns>
        [AllowAnonymous]
        public IActionResult ResetPasswordURL()
        {
            return View();
        }

        /// <summary>
        /// This task handles the action when a user has requested to send a reset password URL.
        /// </summary>
        /// <param name="input">The user email where the reset password link will be sent.</param>
        /// <returns>The page when a user has sent a reset password URL, or error feedback on failure.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordURL(IFormCollection input)
        {
            AppUser user = await _userManager.FindByEmailAsync(input["email"]);

            if (user != null)
            {
                // Utilize the ASP.NET Identity token generator:
                var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;

                // Generate the URL to reset the user password:
                var resetLink = Url.Action("ResetPassword",
                                "Account", new { token },
                                 protocol: HttpContext.Request.Scheme);

                // Send the generated URL to the user email:
                await _emailSender.SendEmailAsync(user.Email, "Reset Password", "To reset your account password, click on the following link: " + resetLink);

                TempData["success"] = "A password reset link has been sent to your email address!";
                return View(nameof(Login));
            }
            else
                TempData["error"] = "Invalid user email.";

            return View();
        }

        /// <summary>
        /// This action renders the page to be able to set a new password to a user email/account.
        /// </summary>
        /// <param name="token">A token is required to enable the password recovery for a specific user.</param>
        /// <returns>The page to reset a user password.</returns>
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (token != null)
                HttpContext.Session.SetString("token", token);

            return View();
        }

        /// <summary>
        /// This task handles the action when a user password has been requested to be recovered,
        /// i.e. when the form to reset the password has been submitted.
        /// </summary>
        /// <param name="input">The password reset object.</param>
        /// <returns>The page when the user password has been updated, or error feedback on failure.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(PasswordReset input)
        {
            // Ensure the user token is catched in a new post request when input errors have occured on submit:
            input.Token = HttpContext.Session.GetString("token");

            AppUser user = new AppUser();

            if (!string.IsNullOrEmpty(input.Email))
                user = await _userManager.FindByEmailAsync(input.Email);

            if ((input.Password != null || input.ConfirmPassword != null) && !input.Password.Equals(input.ConfirmPassword))
            {
                ModelState.AddModelError("", "The passwords did not match.");
                return View(input);
            }
                
            // If success:
            if (user != null && !string.IsNullOrEmpty(input.Password))
            {
                IdentityResult result = _userManager.ResetPasswordAsync(user, input.Token, input.Password).Result;
                if (result.Succeeded)
                {
                    TempData["success"] = "Your password was reset successfully.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    Errors(result);
                    return View(input);
                }
            }
            else if (user == null)
            {
                ModelState.AddModelError("", "Invalid user email.");
                return View(input);
            }
            return View();
        }

        /// <summary>
        /// This action renders an access denied page for users missing permissions to perform certain actions.
        /// </summary>
        /// <returns>An access denied page.</returns>
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// This function collects all error messages that might occur on input failures.
        /// </summary>
        /// <param name="result">Feedback message.</param>
        public void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

    } // End class.
} // End namespace.
