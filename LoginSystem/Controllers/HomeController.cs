using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
