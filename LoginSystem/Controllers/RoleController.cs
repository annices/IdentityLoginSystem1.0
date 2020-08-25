using LoginSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LoginSystem.Controllers
{
    /// <summary>
    /// This controller handles the permission roles in the application.
    /// </summary>
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        /// <summary>
        /// This constructor injects some ASP.NET Identity dependencies to enable some
        /// of their functions once this controller is called.
        /// </summary>
        /// <param name="roleMgr">RoleManager class.</param>
        /// <param name="userMgr">UserManager class.</param>
        public RoleController(RoleManager<IdentityRole> roleMgr, UserManager<AppUser> userMgr, IConfiguration config)
        {
            _roleManager = roleMgr;
            _userManager = userMgr;
            _config = config;
        }

        /// <summary>
        /// This method renders the start page for user roles, i.e. lists the available application permissions. The policy
        /// "SA&A&LA" registered in Startup.cs is used to only allow SuperAdmins, Admins and LimitedAdmins to access this page.
        /// </summary>
        /// <returns>The user role start page.</returns>
        [Authorize(Policy = "SA&A&LA")]
        public ViewResult Index() => View(_roleManager.Roles);

        /// <summary>
        /// This task renders the page to list all available users that can be assigned/unassigned a permission role.
        /// The policy "SA&A" registered in Startup.cs is used to only allow SuperAdmins and Admins to access this page.
        /// </summary>
        /// <param name="id">Role ID.</param>
        /// <returns>The available users to allocate to a role.</returns>
        [Authorize(Policy = "SA&A")]
        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();
            AppUser loggedInUser = await _userManager.FindByIdAsync(HttpContext.Session.GetString("userid"));
            TempData["IsNotSuperAdmin"] = (!_userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result) ? true : false;

            foreach (AppUser user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }

            RoleMembers item = new RoleMembers
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            };

            // Ensure that SuperAdmins have full access and that Admins can only set LimitedAdmin roles:
            if (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result
                || (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().A]).Result && role.Name.Equals(_config[new Role().LA])))
            {
                return View(item);
            }
            else
                return RedirectToAction("AccessDenied", "Account");
        }

        /// <summary>
        /// This task handles the action when users are assigned or unassigned a permission role. The policy "SA&A"
        /// registered in Startup.cs is used to only allow Admins and SuperAdmins to change roles for users.
        /// </summary>
        /// <param name="input">User role object.</param>
        /// <returns>Added or removed permission for users.</returns>
        [HttpPost]
        [Authorize(Policy = "SA&A")]
        public async Task<IActionResult> Update(UserRole input)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                // Apply the role to selected users:
                foreach (string userId in input.AddIds ?? new string[] { })
                {
                    AppUser user = await _userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        result = await _userManager.AddToRoleAsync(user, input.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                // Remove the role from selected users:
                foreach (string userId in input.DeleteIds ?? new string[] { })
                {
                    AppUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await _userManager.RemoveFromRoleAsync(user, input.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }
            if (ModelState.IsValid)
                return RedirectToAction(nameof(Update), "Role", new { id = input.RoleId });
            else
                return await Update(input.RoleId);
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