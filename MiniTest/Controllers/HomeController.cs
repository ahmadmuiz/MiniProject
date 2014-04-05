using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
            SendMessage(@"http://www.kaskus.co.id/user/editprofile", "12", message);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string securitytoken, string md5password, string md5password_utf, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(username)) throw new Exception("Please fill username.");
                bool checkAuth = Login(@"https://www.kaskus.co.id/user/login", username, password, "1396676221-46ebb6e13bee896959eaeb49ea341150", @"/", "59513df1d1602cd98937563a63d38f5b", "59513df1d1602cd98937563a63d38f5b");
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
        private bool Login(string loginPageAddress, string userName, string password, string security_token, string url, string md5password, string md5password_utf)
        {
            try
            {
                StringBuilder loginData = new StringBuilder();
                loginData.Append((String.Format("username={0}&", userName)));
                loginData.Append((String.Format("password={0}&", password)));
                loginData.Append((String.Format("securitytoken={0}&", security_token)));
                loginData.Append((String.Format("url={0}&", url)));
                loginData.Append((String.Format("md5password={0}&", md5password)));
                loginData.Append((String.Format("md5password_utf={0}", md5password_utf)));
                var request = (HttpWebRequest)WebRequest.Create(loginPageAddress);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.66 Safari/537.36";
                var buffer = Encoding.ASCII.GetBytes(loginData.ToString());
                request.ContentLength = buffer.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }
                string result = string.Empty;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Cookies =  parseCookie(request, response.Headers);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        response.Close();
                    }
                }
                if (Cookies.Count > 0 && Cookies["userid"] != null)
                {
                    return true;
                }
                else return false;
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.66 Safari/537.36";
            if (Cookies.Count > 0)
            {
                string hash = Cookies["hash"] != null ? Cookies["hash"].Value : string.Empty;
                string userid = Cookies["userid"] != null ? Cookies["userid"].Value : string.Empty;
                string key = Cookies["key"] != null ? Cookies["key"].Value : string.Empty;
                request.Headers["Cookie"] = string.Format("hash={0}; userid={1}; key={2}", hash, userid, key); 
            }
            //CookieContainer container = new CookieContainer();
            //container.Add(Cookies);
            //request.CookieContainer = container;

            byte[] _byteVersion = Encoding.ASCII.GetBytes(string.Concat("content="));
            request.ContentLength = _byteVersion.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(_byteVersion, 0, _byteVersion.Length);
                stream.Close();
            }
            string result = string.Empty;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    response.Close();
                }
            }
        }
    }
}