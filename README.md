# Table of contents
<details>
   <summary>Click here to expand content list.</summary>
   
1. [General information](#1-general-information)
2. [License](#2-license)
3. [System description](#3-system-description)
4. [System requirements](#4-system-requirements)
5. [Supported features](#5-supported-features)
6. [Roles and permissions](#6-roles-and-permissions)
    * [6.1 No roles (default)](#61-no-roles-default)
    * [6.2 LimitedAdmin](#62-limitedadmin)
    * [6.3 Admin](#63-admin)
    * [6.4 SuperAdmin](#64-superadmin)
7. [ER diagram](#7-er-diagram)
8. [User interface](#8-user-interface)
9. [Setup guide](#9-setup-guide)
    * [9.1 Prerequisites](#91-prerequisites)
    * [9.2 Configure the application](#92-configure-the-application)
    * [9.3 NuGet packages](#93-nuget-packages)
    * [9.4 Create the ASP.NET Identity database and its tables](#94-create-the-aspnet-identity-database-and-its-tables)
    * [9.5 Run the application and create your first admin](#95-run-the-application-and-create-your-first-admin)
10. [Contact details](#10-contact-details)
</details>

---

# 1 General information
“Identity Login System 1.0” was created in Visual Studio Community by Annice Strömberg, 2020, with Annice.se as the primary download location.

The script is a multi-user role login system based on the ASP.NET Core Identity framework. Furthermore, the user data is stored in an SQL Server database using Entity Framework (EF) Core – code first.

---

# 2 License
Released under the MIT license.

MIT: [http://rem.mit-license.org](http://rem.mit-license.org/), see [LICENSE](LICENSE).

---

# 3 System description
“Identity Login System 1.0” is built in CSS3, HTML5, JavaScript, C# with ASP.NET Core Identity and Entity Framework Core (code first) with SQL Server as the database management system (DBMS). Furthermore, the application is built according to the model-view-controller (MVC) pattern.

---

# 4 System requirements
The script can be run on a server that supports C# 8.0 with ASP.NET Core 3.0 with the .NET Core 3 platform installed, along with an SQL Server supported database.

---

# 5 Supported features
The following functions and features are supported by this script:
  * Multi-user role login system based on cookies.
  * User password encryption (HMAC-SHA256).
  * Password recovery function via a generated reset token link.
  * Database storage of user details (via EF Core code first).
  * Protection against cross-site request forgery.
  * Sort and filter function of users.
  * Paging function of users on the user list page.
  * CRUD function of users for Admins and SuperAdmins.
  * Client and server side validation.
  * Responsive design.

---
  
# 6 Roles and permissions
In this application, a new user account has by default no roles assigned to it. However, a user account can be assigned to one or many of the following permission roles:
  * LimitedAdmin
  * Admin
  * SuperAdmin

## 6.1 No roles (default)
A user with no roles yet assigned to the account will only have read permissions of other users. However, a registered user will always be able to update the own user details.

## 6.2 LimitedAdmin
LimitedAdmin users will have read permissions for all users and roles, but no edit permissions.

## 6.3 Admin
Admins will have read permissions for all users and roles. Also, the Admin users can edit LimitedAdmins, and users with no roles. However, Admins can only edit LimitedAdmins as long as they donnot also have equivalent or superior roles assigned to them as the Admins.

## 6.4 SuperAdmin
SuperAdmins have at all times full permissions and access throughout the entire application. One SuperAdmin can also edit another SuperAdmin.
  
---

# 7 ER diagram
This section illustrates the relational database and all its tables supported by the ASP.NET Identity 3.0 framework. Furthermore, the blue marked database tables in the image below are the ones used by this script.

<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/er-diagram.png" alt="" width="700">

---

# 8 User interface
Screenshot of the user list page in desktop vs. responsive view for a SuperAdmin user:

<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-user-list-desk.png" alt="" width="500"> <img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-user-list-resp.png" alt="" width="200">

Screenshot of a user edit page in desktop vs. responsive view for a SuperAdmin user:

<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-user-edit-desktop.png" alt="" width="430"> <img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-user-edit-responsive.png" alt="" width="240">

Screenshot of the role list page in desktop vs. responsive view a SuperAdmin user:

<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-role-list-desktop.png" alt="" width="500"> <img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-role-list-responsive.png" alt="" width="300">

Screenshot of the role edit page in desktop vs responsive view for a SuperAdmin user:

<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-role-edit-desktop.png" alt="" width="430"> <img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/gui-role-edit-responsive.png" alt="" width="240">

---

# 9 Setup guide
As this script was created in Visual Studio Community with SQL Server, I will go through the necessary setup steps accordingly (all softwares used for this application setup are free).

## 9.1 Prerequisites
  * [Install SQL Server Express](https://www.microsoft.com/sv-se/sql-server/sql-server-downloads)
  * [Install SQL Server Management Studio (SSMS)](https://docs.microsoft.com/sv-se/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15)
  * [Install .NET Core 3.1 (SDK)](https://dotnet.microsoft.com/download)
  * [Install Visual Studio Community](https://visualstudio.microsoft.com/vs/community/)
  
## 9.2 Configure the application
1. Select to open the application in Visual Studio by double clicking the “LoginSystem.sln” file via the unzipped script folder path:
*IdentityLoginSystem1.0 > LoginSystem > LoginSystem.sln*

2. Once the LoginSystem solution is open in Visual Studio, select to open its “appsettings.json” file in the “Solution Explorer” window and change the commented values below to suit your own settings. (**Note!** To enable the ability to send password recovery emails via your Gmail, you will have to configure your Gmail account to [enable less secure apps](https://myaccount.google.com/lesssecureapps)):

```json
{
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "LoginSystemConnection": "Server=.\\SQLEXPRESS;Database=LoginSystem;Trusted_Connection=True;MultipleActiveResultSets=true" // Only change this if you want to use another DB name!
  },

  "EmailSender": {
    "Host": "smtp.gmail.com", // Keep this setting if you use Gmail.
    "Port": 587, // Keep this setting if you use Gmail.
    "EnableSSL": true,
    "UserName": "your@gmail.com", // Your email.
    "Password": "YourGmailPassword" // Your email password.
  },

  "Paging": {
    "ItemsPerPage": "50" // The number of records to display per page on the user list page.
  },

  "UserRoles": {
    "SA": "SuperAdmin", // Keep the role names, otherwise you must ensure they reflect the ones stored in DB!
    "A": "Admin",
    "LA": "LimitedAdmin"
  }
}
```

## 9.3 NuGet packages
3. Also, ensure you have the following NuGet packages installed for the solution, otherwise [install](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell) them:
  
    * Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore (3.0.0)
    * Microsoft.AspNetCore.Identity.EntityFrameworkCore (3.0.0)
    * Microsoft.AspNetCore.Identity.UI (3.0.0)
    * Microsoft.EntityFrameworkCore.SqlServer (3.0.0)
    * Microsoft.EntityFrameworkCore.Tools (3.0.0)
    * Microsoft.Extensions.Loggin.Debug (3.0.0)
    * Microsoft.VisualStudio.Web.CodeGeneration.Design (3.0.0)
    * ReflectionIT.Mvc.Paging (4.0.0)
    
## 9.4 Create the ASP.NET Identity database and its tables
5. In Visual Studio, open the package manager console (PMC) via menu option: *Tools > NuGet Package Manager > Package Manager Console*

6. In the PMC, check that you are in the same folder as your “Startup.cs” file by typing the “dir” command followed by enter. If the startup file is listed, you can create a new database migration by typing the following command and then press enter: add-migration *<a migration name>*
<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/pmc-add-migration.png" alt="" width="400">

After you have executed the above command, you will notice that a new migration folder named “Migrations” has been added in the “Solution Explorer” window.

7. After you have added the migration, create the database and its tables by typing the following command in PMC followed by enter: *update-database*
<img src="https://diagrams.annice.se/c-sharp-identity-login-system-1.0/pmc-update-database.png" alt="" width="400">

8. Once you see the feedback message “Done” in the PMC, the database has been created based on your connection string in “appsettings.json” along with the default database tables supported by the ASP.NET Identity framework. (You can also control this by connecting to your SQLEXPRESS instance via SSMS and check for the database tables there.)

## 9.5 Run the application and create your first admin
9. When the database and its tables are created, you can select to run the application from Visual Studio via the play button in the top bar.

10. On the initial application launch, you will be redirected to a page to create your first super admin. Fill in the mandatory fields and click to create this user.

11. Once the first super admin is created, you can login with your specified user credentials and start using the app!

---

# 10 Contact details
For general feedback related to this script, such as any discovered bugs etc., you can contact me via the following email address: [info@annice.se](mailto:info@annice.se)
