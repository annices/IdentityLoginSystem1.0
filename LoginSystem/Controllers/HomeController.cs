using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginSystem.Controllers
{
    /// <summary>
    /// This controller handles the public application start page.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// This action renders the application start page.
        /// </summary>
        /// <returns>Start page.</returns>
        [AllowAnonymous]
        public ActionResult Index()
        {
            string path = "./Views/Temp/CreateSuperAdmin.cshtml";
            FileInfo file = new FileInfo(path);

            if (file.Exists)
                return RedirectToAction("CreateSuperAdmin", "Temp");
            else
                return View();
        }

    } // End class.
} // End namespace.
