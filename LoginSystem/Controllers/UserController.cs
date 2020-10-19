using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReflectionIT.Mvc.Paging;

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
    /// This controller handles the CRUD operations of user profiles.
    /// </summary>
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly IUserValidator<AppUser> _userValidator;
        private readonly IPasswordValidator<AppUser> _passwordValidator;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IConfiguration _config;

        /// <summary>
        /// This constructor injects different dependencies to enable some functions
        /// to use once this controller is called.
        /// </summary>
        /// <param name="usrMgr">UserManager class.</param>
        /// <param name="usrVal">IUserValidation interface.</param>
        /// <param name="passwordHash">IPasswordHasher interface.</param>
        /// <param name="usrPass">IPasswordValidator interface.</param>
        public UserController(UserManager<AppUser> usrMgr, RoleManager<IdentityRole> roleMgr, IUserValidator<AppUser> usrVal, IPasswordHasher<AppUser> passwordHash, IPasswordValidator<AppUser> usrPass, IConfiguration config)
        {
            _userManager = usrMgr;
            _roleManager = roleMgr;
            _userValidator = usrVal;
            _passwordValidator = usrPass;
            _passwordHasher = passwordHash;
            _config = config;
        }

        /// <summary>
        /// This task returns all application users and handles search, sort and filter functions for these users.
        /// Also, the page requires authorization.
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="searchString"></param>
        /// <param name="filterByRoles"></param>
        /// <param name="page"></param>
        /// <returns>The application users.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string[] filterByRoles, int page = 1)
        {
            // Prepare view variables to catch search and filter values:
            TempData["UserNameSort"] = sortOrder == "Username" ? "username_desc" : "Username";
            TempData["CreatedSort"] = sortOrder == "Created" ? "created_desc" : "Created";
            TempData["NameSort"] = sortOrder == "Name" ? "name_desc" : "Name";
            TempData["EmailSort"] = sortOrder == "Email" ? "email_desc" : "Email";
            TempData["SearchWord"] = searchString;

            ViewBag.AllRoles = _roleManager.Roles;

            // Create a default user list (note! Ordered list must be used for paging list):
            var users = _userManager.Users.OrderBy(u => u.UserName);

            // Find users by search words:
            if (!string.IsNullOrEmpty(searchString))
            {
                users = (IOrderedQueryable<AppUser>)users
                        .Where(u => u.UserName.Contains(searchString) || u.FirstName.Contains(searchString)
                        || u.LastName.Contains(searchString) || u.Email.Contains(searchString));
            }

            // Filter users by roles:
            if (filterByRoles.Length > 0)
            {
                ViewBag.SelectedRoles = filterByRoles;
                var items = new List<AppUser>();
                foreach (AppUser user in users)
                {
                    foreach (string role in filterByRoles)
                    {
                        if (_userManager.IsInRoleAsync(user, role).Result)
                            items.Add(user);
                    }
                }
                IOrderedQueryable<AppUser> orderedUsersByRole = (from user in items select user).AsQueryable().OrderBy(user => user.UserName);
                users = orderedUsersByRole;
            }

            // Sort users by column values:
            switch (sortOrder)
            {
                case "username_desc":
                    users = users.OrderByDescending(u => u.UserName);
                    break;
                case "Name":
                    users = users.OrderBy(u => u.FirstName);
                    break;
                case "name_desc":
                    users = users.OrderByDescending(u => u.UserName);
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "Email":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "Created":
                    users = users.OrderBy(u => u.Created);
                    break;
                case "created_desc":
                    users = users.OrderByDescending(u => u.Created);
                    break;
                default:
                    users = users.OrderBy(u => u.UserName);
                    break;
            }

            // Return the result list with applied pagination:
            var model = PagingList.Create(users, Convert.ToInt32(_config["Paging:ItemsPerPage"]), page);
            return View(model);
        }

        /// <summary>
        /// This action renders the page to be able to create a new user.
        /// The policy "SA&A" registered in Startup.cs is used to only
        /// allow SuperAdmins and Admins to access this page.
        /// </summary>
        /// <returns>The page to create a new user.</returns>
        [HttpGet]
        [Authorize(Policy = "SA&A")]
        public async Task<IActionResult> Create()
        {
            AppUser loggedInUser = await _userManager.FindByIdAsync(HttpContext.Session.GetString("userid"));
            // Create some view bag variables to be able to call them in view:
            ViewBag.AllRoles = _roleManager.Roles;
            // Create view bags to handle SuperAdmin vs Admin conditions to set user roles in view:
            ViewBag.SA_Editable = (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result) ? true : false;
            ViewBag.A_Editable = (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().A]).Result) ? true : false;
            ViewBag.LA = _config[new Role().LA];

            // Prevent loss of ViewBag values on a post request and stick to DRY principle by handling input failures here:
            if (TempData["input"] != null && TempData["result"] != null)
            {
                User input = JsonConvert.DeserializeObject<User>(TempData["input"].ToString());
                IdentityResult result = JsonConvert.DeserializeObject<IdentityResult>(TempData["result"].ToString());
                Errors(result);

                return View(input);
            }
            return View();
        }

        /// <summary>
        /// This task handles the action when a new user has been requested to be created,
        /// i.e. when the create form has been submitted. The policy "SA&A" registered in
        /// Startup.cs is used to only allow SuperAdmins and Admins to access this page.
        /// </summary>
        /// <param name="input">The user to be created.</param>
        /// <returns>The page when a user has been created, or error feedback on input failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "SA&A")]
        public async Task<IActionResult> Create(User input, string[] selectedRoles)
        {
            if (string.IsNullOrEmpty(input.Password))
            {
                ViewBag.Error = "The password field is required.";
                return RedirectToAction(nameof(Create), "User");
            }

            AppUser user = new AppUser
            {
                UserName = input.Username,
                FirstName = input.Firstname,
                LastName = input.Lastname,
                Email = input.Email,
                PhoneNumber = input.Phonenumber,
                Created = DateTime.Now.ToString("yyyy-MM-dd hh:mm"), // Format DateTime to skip seconds.
                Updated = DateTime.Now.ToString("yyyy-MM-dd hh:mm")
            };

            if (ModelState.IsValid)
            {
                // Create input result feedback and store in TempData to be catched in HttpGet action:
                TempData["input"] = JsonConvert.SerializeObject(input);
                IdentityResult result = await _userManager.CreateAsync(user, input.Password);
                TempData["result"] = JsonConvert.SerializeObject(result);

                if (result.Succeeded)
                {
                    TempData.Remove("input");

                    // If roles are selected, add them to the user:
                    if (selectedRoles.Length > 0)
                    {
                        foreach (string role in selectedRoles)
                        {
                            if (user != null)
                                await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Create), "User");
        }

        /// <summary>
        /// This action redirects a currently logged in user to the page where they can edit their own credentials.
        /// (This is applied on the logged in username link in Views > Shared > _LoginPartial.cshtml)
        /// </summary>
        /// <returns>The user edit page for the logged in user.</returns>
        public IActionResult Manage()
        {
            if (HttpContext.Session.GetString("userid") != null)
            {
                TempData["OwnEditPage"] = HttpContext.Session.GetString("userid");
                return Redirect("Update/" + HttpContext.Session.GetString("userid"));
            }
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// This task handles the action to display details about a user.
        /// Also, the page requires authorization.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>The user profile.</returns>
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            return View(user);
        }

        /// <summary>
        /// This task renders the page to be able to update a user profile.
        /// Also, the page requires authorization.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The page to update a user profile.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Update(string id)
        {
            AppUser userToEdit = await _userManager.FindByIdAsync(id);
            AppUser loggedInUser = await _userManager.FindByIdAsync(HttpContext.Session.GetString("userid"));
            var currentUserRoles = await _userManager.GetRolesAsync(userToEdit);

            // Create some variables to be called in view:
            TempData["UserName"] = userToEdit.UserName;
            ViewBag.AllRoles = _roleManager.Roles;
            TempData["CurrentUserRoles"] = currentUserRoles;
            // Create view bags to handle SuperAdmin vs Admin conditions to set user roles in view:
            ViewBag.SA_Editable = _userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result ? true : false;
            ViewBag.A_Editable = (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().A]).Result
                                  && (_userManager.IsInRoleAsync(userToEdit, _config[new Role().LA]).Result || !currentUserRoles.Any())) ? true : false;
            ViewBag.LA = _config[new Role().LA];

            if (userToEdit == null)
                return RedirectToAction(nameof(Index));

            // Ensure that users can edit their own profiles while SuperAdmins always have full access,
            // and that Admins can only edit LimitedAdmins:
            else if (userToEdit.Id.Equals(loggedInUser.Id)
                || _userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result
                || (_userManager.IsInRoleAsync(loggedInUser, _config[new Role().A]).Result
                && !(_userManager.IsInRoleAsync(userToEdit, _config[new Role().SA]).Result
                || _userManager.IsInRoleAsync(userToEdit, _config[new Role().A]).Result)))
            {
                // Prevent loss of ViewBag values on a post request by handling input failures here (catched from post action):
                if (TempData["editInput"] != null && TempData["result"] != null)
                {
                    userToEdit = JsonConvert.DeserializeObject<AppUser>(TempData["editInput"].ToString());
                    IdentityResult result = JsonConvert.DeserializeObject<IdentityResult>(TempData["result"].ToString());
                    Errors(result);
                }

                return View(userToEdit);
            }
            else
                return RedirectToAction("AccessDenied", "Account");
        }

        /// <summary>
        /// This task handles the action when a user profile has been requested to be updated,
        /// i.e. when the user update form has been submitted. The page also requires authorization.
        /// </summary>
        /// <param name="input">User input.</param>
        /// <param name="password">User password.</param>
        /// <returns>Updated user on success, or error feedback on input failures.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Update(AppUser input, string password, string[] selectedRoles)
        {
            AppUser user = await _userManager.FindByIdAsync(input.Id);

            // Prepare the role selection:
            IList allRoles = _roleManager.Roles.ToList();
            var roleNames = new List<string>();
            foreach (IdentityRole roleItem in allRoles)
            {
                // In this case, it's only relevant to get all names:
                roleNames.Add(roleItem.Name);
            }
            // Add the selected roles to the user:
            if (selectedRoles.Length > 0)
            {
                foreach (string role in selectedRoles)
                {
                    if (user != null)
                        await _userManager.AddToRoleAsync(user, role);
                }
                // Also, don't forget to remove the unselected roles from the user:
                foreach (string roleName in selectedRoles)
                {
                    roleNames = roleNames.Except(selectedRoles).ToList();
                    foreach (string role in roleNames)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }
                }
            }
            else
            {
                foreach (string role in roleNames)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }
            // Assign the rest of the posted values to the user...
            user.UserName = input.UserName;
            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            user.Email = input.Email;
            user.PhoneNumber = input.PhoneNumber;
            user.Updated = DateTime.Now.ToString("yyyy-MM-dd hh:mm");

            if (ModelState.IsValid)
            {
                IdentityResult result = null;

                if (!string.IsNullOrEmpty(password))
                {
                    result = await _passwordValidator.ValidateAsync(_userManager, user, password);

                    if (!result.Succeeded)
                    {
                        // Create input result feedback and store in TempData to be catched in HttpGet action:
                        TempData["result"] = JsonConvert.SerializeObject(result);
                        TempData["editInput"] = JsonConvert.SerializeObject(input);
                        return RedirectToAction(nameof(Update), "User");
                    }
                    else
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                }

                result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    TempData["result"] = JsonConvert.SerializeObject(result);
                    TempData["editInput"] = JsonConvert.SerializeObject(input);
                    return RedirectToAction(nameof(Update), "User");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// This task handles the action when a user profile has been requested to be deleted,
        /// i.e. when a delete link has been clicked. The "SA&A" policy registered in Startup.cs
        /// is used to only allow SuperAdmins and Admins to perform this action.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The page when a user has been deleted, or error feedback on delete failure.</returns>
        [HttpPost]
        [Authorize(Policy = "SA&A")]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                AppUser loggedInUser = new AppUser { Id = HttpContext.Session.GetString("userid") };
                bool isAdmin = _userManager.GetRolesAsync(loggedInUser).Result.Any(role => role == _config[new Role().A]);
                bool isSuperAdmin = _userManager.GetRolesAsync(loggedInUser).Result.Any(role => role == _config[new Role().SA]);

                // Ensure that Admins can only delete LimitedAdmins or users with no roles:
                if (!isSuperAdmin && isAdmin && !(_userManager.IsInRoleAsync(user, _config[new Role().LA]).Result
                    || _userManager.GetRolesAsync(user).Result.Any()))
                    RedirectToAction("AccessDenied", "Account");

                IdentityResult result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No user was found.");

            return View(nameof(Index), _userManager.Users);
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
