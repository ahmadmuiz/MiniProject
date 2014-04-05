using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiniTest.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (!IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            else
                return View();
        }

        [HttpPost   ]
        public ActionResult SendMessage()
        {
            return View(); 
        }

        [HttpGet]
        public ActionResult Login()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) throw new Exception("Please fill username and password.");
                bool checkAuth = true;
                if (checkAuth)
                {
                    IsAuthenticated = true;
                    ApplicationId = "Mini Project";
                    System.Web.Security.FormsAuthentication.SetAuthCookie(ApplicationId, true);
                    return RedirectToAction("Index", "Home");    //redirect to default page
                }
                else
                {
                    ViewBag.Message = "You have no authorization to access the application.";
                    Session.Abandon();
                    Session.Clear();
                }

            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return PartialView("Login");
            }
            return PartialView("Login");
        }

        public ActionResult LogOut()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login");
        }

    }
}