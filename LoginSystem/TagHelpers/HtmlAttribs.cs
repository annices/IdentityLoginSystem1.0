using LoginSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
namespace LoginSystem.TagHelpers
{
    /// <summary>
    /// This class creates a tag attribute to be able to get all users with a specific role and display them in a table cell (td).
    /// </summary>
    [HtmlTargetElement("td", Attributes = "asp-users-in-role")]
    public class RoleUsers : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleUsers(UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr)
        {
            _userManager = userMgr;
            _roleManager = roleMgr;
        }

        [HtmlAttributeName("asp-users-in-role")]
        public string Role { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> users = new List<string>();
            IdentityRole role = await _roleManager.FindByIdAsync(Role);

            if (role != null)
            {
                foreach (var user in _userManager.Users)
                {
                    if (user != null && await _userManager.IsInRoleAsync(user, role.Name))
                    {
                        users.Add(user.UserName);
                    }
                }
            }
            // Handle output in case when many users are assigned to a role:
            if(users.Count > 400)
            {
                users = users.Take(400).ToList();
                output.Content.SetContent(string.Join(", ", users)+"...");
            }
            else
                output.Content.SetContent(users.Count == 0 ? "No users" : string.Join(", ", users));
        }

    } // End class.

    /// <summary>
    /// This class creates a tag attribute to be able to get all roles assigned to a specific user and display them in a table cell (td).
    /// </summary>
    [HtmlTargetElement("td", Attributes = "asp-user-roles")]
    public class UserRoles : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;

        public UserRoles(UserManager<AppUser> userMgr)
        {
            _userManager = userMgr;
        }

        [HtmlAttributeName("asp-user-roles")]
        public string User { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = new AppUser { Id = User };
            IList<string> roles = await _userManager.GetRolesAsync(user);

            output.Content.SetContent(roles.Count == 0 ? "No roles" : string.Join(", ", roles));
        }

    } // End class.

    /// <summary>
    /// This class creates a tag attribute to display/hide a link to be able to edit user roles depending on permission.
    /// </summary>
    [HtmlTargetElement("a", Attributes = "asp-editable-for")]
    public class UIElementVisibilityOnRolePage : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;

        public UIElementVisibilityOnRolePage(UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr, IHttpContextAccessor contextAccessor, IConfiguration config)
        {
            _userManager = userMgr;
            _roleManager = roleMgr;
            _contextAccessor = contextAccessor;
            _config = config;
        }

        [HtmlAttributeName("asp-editable-for")]
        public string Role { get; set; }

        /// <summary>
        /// This method ensures that SuperAdmins always have full permissions to change user roles,
        /// while Admins can only change LimitedAdmin roles. Furthermore, write permissions will be
        /// denied for LimitedAdmins and users with no roles.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser loggedInUser = await _userManager.FindByIdAsync(_contextAccessor.HttpContext.Session.GetString("userid"));
            IdentityRole role = new IdentityRole { Name = Role };
            var loggedInUserRoles = _userManager.GetRolesAsync(loggedInUser).Result;
            bool isSuperAdmin = loggedInUserRoles.Any(role => role == _config[new Role().SA]);
            bool isAdmin = loggedInUserRoles.Any(role => role == _config[new Role().A]);

            if ((!isSuperAdmin && isAdmin && !role.Name.Equals(_config[new Role().LA])) || !(isSuperAdmin || isAdmin))
                output.SuppressOutput();
        }

    } // End class.

    /// <summary>
    /// This class creates a tag attribute to display/hide links to be able to edit users depending on user permission.
    /// </summary>
    [HtmlTargetElement("div", Attributes = "asp-visible-for")]
    public class UIElementVisibilityOnUserPage : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;

        public UIElementVisibilityOnUserPage(UserManager<AppUser> userMgr, IHttpContextAccessor contextAccessor, IConfiguration config)
        {
            _userManager = userMgr;
            _contextAccessor = contextAccessor;
            _config = config;
        }

        [HtmlAttributeName("asp-visible-for")]
        public string User { get; set; }

        /// <summary>
        /// This method ensures that SuperAdmins always have full permissions, while Admins can only edit
        /// LimitedAdmins or users with no roles. Furthermore, users with no roles or LimitedAdmins will
        /// only have read permissions.
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = new AppUser { Id = User };
            AppUser loggedInUser = await _userManager.FindByIdAsync(_contextAccessor.HttpContext.Session.GetString("userid"));
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            IList<string> loggedInUserRoles = await _userManager.GetRolesAsync(loggedInUser);

            if (loggedInUserRoles.Count == 0 || (loggedInUserRoles.Count == 1 && loggedInUserRoles[0].Equals(_config[new Role().LA])))
            {
                output.SuppressOutput();
            }
            else
            {
                bool isSuperAdmin = loggedInUserRoles.Any(role => role == _config[new Role().SA]);
                bool isAdmin = loggedInUserRoles.Any(role => role == _config[new Role().A]);

                foreach (var role in userRoles)
                {
                    // userRoles.Count == 1 && role.Equals(_config[new Role().LA]) && !(isAdmin || isSuperAdmin))
                    if (((role.Equals(_config[new Role().A]) || role.Equals(_config[new Role().SA])) && !isSuperAdmin)
                        || userRoles.Count == 1 && role.Equals(_config[new Role().LA]) && !(isAdmin || isSuperAdmin))
                        output.SuppressOutput();
                }
            }
        }

    } // End class.

    /// <summary>
    /// This class creates a tag attribute to display/hide the create user link depending on user permissions.
    /// </summary>
    [HtmlTargetElement("p", Attributes = "asp-is-user")]
    public class UIElementVisibleCreateLink : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;

        public UIElementVisibleCreateLink(UserManager<AppUser> userMgr, IHttpContextAccessor contextAccessor, IConfiguration config)
        {
            _userManager = userMgr;
            _contextAccessor = contextAccessor;
            _config = config;
        }

        [HtmlAttributeName("asp-is-user")]
        public string User { get; set; }

        /// <summary>
        /// This method ensures that only SuperAdmins and Admins can see the link to create a new user.
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = new AppUser { UserName = User };
            IQueryable<string> userId = _userManager.Users.Where(u => u.UserName.Equals(user.UserName)).Select(u => u.Id);

            foreach (var id in userId)
            {
                user.Id = id;
            }

            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Count == 0 || (userRoles.Count == 1 && userRoles[0].Equals(_config[new Role().LA])))
                output.SuppressOutput();
        }

    } // End class.

    /// <summary>
    /// This class creates a tag attribute to disable input fields depending on user permissions.
    /// </summary>
    [HtmlTargetElement("input", Attributes = "asp-disable")]
    public class UIElementDisable : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;

        public UIElementDisable(UserManager<AppUser> userMgr, IHttpContextAccessor contextAccessor, IConfiguration config)
        {
            _userManager = userMgr;
            _contextAccessor = contextAccessor;
            _config = config;
        }

        [HtmlAttributeName("asp-disable")]
        public string User { get; set; }

        /// <summary>
        /// This method ensures that Admins can only edit LimitedAdmin roles of users not also having equivalent or superior roles.
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = new AppUser { Id = User };
            AppUser loggedInUser = await _userManager.FindByIdAsync(_contextAccessor.HttpContext.Session.GetString("userid"));

            bool notEditable = ((_userManager.GetRolesAsync(user).Result.Any(role => role == _config[new Role().SA])
                                || _userManager.GetRolesAsync(user).Result.Any(role => role == _config[new Role().A]))
                                && !_userManager.IsInRoleAsync(loggedInUser, _config[new Role().SA]).Result);

            if (notEditable)
                output.Attributes.SetAttribute("disabled", "disabled");
        }

    } // End class.

} // End namespace.
