using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiniTest.Controllers
{
    public class HomeController: BaseController
    {
        public ActionResult Index()
        {
            if (!IsAdministrator)
            {
                return RedirectToAction("Login");
            }else 
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return PartialView();
        }
    }
}