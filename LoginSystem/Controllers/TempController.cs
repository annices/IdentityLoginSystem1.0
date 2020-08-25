using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LoginSystem.Models;
using System.IO;
using System;
using System.Threading.Tasks;

namespace LoginSystem.Controllers
{
    /// <summary>
    /// The purpose with this temporary controller is to seed the application with default permission roles and create the first super admin.
    /// Since the page to create the first super admin is a public page, the files/code to perform this will be removed once this action is done.
    /// </summary>
    public class TempController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TempController(UserManager<AppUser> usrMgr, RoleManager<IdentityRole> roleMgr)
        {
            _userManager = usrMgr;
            _roleManager = roleMgr;
        }

        /// <summary>
        /// This action renders the page to create the first super admin.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateSuperAdmin()
        {
            string path = "./Views/Temp/CreateSuperAdmin.cshtml";
            FileInfo file = new FileInfo(path);

            if (file.Exists)
                return View();
            else
                return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// This action creates a first super admin along with permission roles for the application. Note! The role names reflect the specified
        /// role names in appsettings.json. In turn, the appsettings role names have to reflect the ones stored in the database!
        /// </summary>
        /// <param name="item">User object.</param>
        /// <returns>Redirection to start page on success, or error feedback on failure.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSuperAdmin(User input)
        {
            // Seed the application with the default permission roles: "SuperAdmin", "Admin", and "LimitedAdmin":
            if (!_roleManager.RoleExistsAsync("SuperAdmin").Result)
            {
                string name = "SuperAdmin";
                IdentityResult result = _roleManager.CreateAsync(new IdentityRole(name)).Result;
            }
            if (!_roleManager.RoleExistsAsync("Admin").Result)
            {
                string name = "Admin";
                IdentityResult result = _roleManager.CreateAsync(new IdentityRole(name)).Result;
            }
            if (!_roleManager.RoleExistsAsync("LimitedAdmin").Result)
            {
                string name = "LimitedAdmin";
                IdentityResult result = _roleManager.CreateAsync(new IdentityRole(name)).Result;
            }

            // Then create the first user with the super admin role:
            if (_userManager.FindByEmailAsync(input.Email).Result == null)
            {
                AppUser user = new AppUser
                {
                    UserName = input.Username,
                    Email = input.Email,
                    Created = DateTime.Now.ToString("yyyy-MM-dd hh:mm"), // Format DateTime to skip seconds.
                    Updated = DateTime.Now.ToString("yyyy-MM-dd hh:mm")
                };

                if (ModelState.IsValid)
                {
                    IdentityResult result = await _userManager.CreateAsync(user, input.Password);

                    if (result.Succeeded)
                        await _userManager.AddToRoleAsync(user, "SuperAdmin");
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                            ModelState.AddModelError("", error.Description);

                        return View(input);
                    }
                }

                // For security reasons, delete the create super admin page along with this controller after the first user has been created:
                string folderPath = "./Views/Temp/";
                var directory = new DirectoryInfo(folderPath);
                directory.Attributes &= ~FileAttributes.ReadOnly;

                string controllerPath = "./Controllers/TempController.cs";
                FileInfo controllerFile = new FileInfo(controllerPath);

                if (controllerFile.Exists || directory.Exists)
                {
                    directory.Delete(true);
                    controllerFile.Delete();

                    return RedirectToAction("Index", "Home");
                }

            }
            return View(input);
        }

    } // End controller.
} // End namespace.