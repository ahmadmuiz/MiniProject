using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MiniTest.Controllers.Attributes;

namespace MiniTest.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet, CacheControl(HttpCacheability.NoCache), SessionExpireFilter]
        public ActionResult Index()
        {
            if (!IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            else
                return View();
        }

        [HttpPost, SessionExpireFilter]
        public ActionResult SendMessage(string message)
        {
            SendMessage(@"https://www.kaskus.co.id/forum/", "", message);
            return RedirectToAction("Index");
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
                bool checkAuth = Login(@"https://www.kaskus.co.id/user/login", username, password);
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

        /// <summary>
        /// handle login authentication 
        /// </summary>
        /// <param name="loginPageAddress"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool Login(string loginPageAddress, string userName, string password)
        {
            try
            {
                NameValueCollection loginData = new NameValueCollection();
                loginData.Add("username", userName);
                loginData.Add("password", password);
                CookieContainer container;
                var request = (HttpWebRequest)WebRequest.Create(loginPageAddress);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                var buffer = Encoding.ASCII.GetBytes(loginData.ToString());
                request.ContentLength = buffer.Length;
                var requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();

                container = request.CookieContainer = new CookieContainer();

                var response = request.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception e)
            {
                string message = string.Format("{0} {1}", e.Message, e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// send message to kaskus forum
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        private void SendMessage(string url, string id, string message)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Credentials = CredentialCache.DefaultCredentials;
                string parameters = string.Format("thread_id={0}&message={1}");
                var response = client.UploadString(url, "POST", parameters);
            }
        }

    }
}