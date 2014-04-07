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
using HtmlAgilityPack;
using MiniTest.Controllers.Attributes;
using MiniTest.Repository;

namespace MiniTest.Controllers
{
    public class HomeController : BaseController
    {
        MongoDBRepository mongoDBRepository = null;

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
            SendKaskusMessage( message);
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
                bool checkAuth = Login(@"https://www.kaskus.co.id/user/login", username, password, securitytoken, url, md5password, md5password_utf);
                if (checkAuth)
                {
                    IsAuthenticated = true;
                    ApplicationId = "Mini Project";
                    System.Web.Security.FormsAuthentication.SetAuthCookie(ApplicationId, true);
                    var user = new Models.UserModel
                    {
                        UserName = username,
                        Password = password,
                        SecurityToken = securitytoken,
                        url = @"https://www.kaskus.co.id/user/login",
                        MD5Password = md5password,
                        MD5Password_UTF = md5password_utf
                    };
                    mongoDBRepository = new MongoDBRepository();
                    mongoDBRepository.CreateUserModel(user);
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

        [HttpGet]
        public ActionResult Token()
        {
            string token = GetLoginSecurityToken(@"http://www.kaskus.co.id/");
            return Json(token, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Captcha()
        {
            HtmlDocument Doc = getUrlContent(@"http://www.kaskus.co.id/post_reply/5340bf8b1f0bc31f3c8b4587");
            string image = getUrlCaptchaImage(Doc);
            string captcha_challenge = getRecaptchaChallengeField(Doc);
            string verify_hash = getHumanVerifyHash(Doc);
            string token = getSecurityToken(Doc);
            return Json(new { image = image, captcha_challenge = captcha_challenge, verify_hash = verify_hash, token = token }, JsonRequestBehavior.AllowGet);
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
                    Cookies = parseCookie(request, response.Headers);
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
        private void SendKaskusMessage( string message)
        {
            string thread_id = "5340bf8b1f0bc31f3c8b4587";
            //string securityToken = GetSecurityToken(string.Format(@"http://www.kaskus.co.id/post_reply/{0}", thread_id));
            string securityToken = "1396878862-77174a43f8113c15850c50e06bf6410e";
            string title = "Just Testing";
            string ajaxhref = @"/misc/getsmilies";
            string iconid = "0";
            string humanverify_hash = "00eed173f0576194f6de4757902a5f78";
            string recaptcha_challenge_field = "03AHJ_Vut9x4rr56TkcTdFbHu_45J0XVXc8gVgvzKyeE54ipY0IKk5se94fpjjmT5roIpsiGV0cttQgGp23gPazL6kCU4AnXsjmh7taLMaJ0fYjaPF0Fmp96NtYI3iH3C3Olk_41PQKzDr2FUwheben23A3FN3EG48nZVxBFNuxH3nX-QmfnUjBhkP3v-gFJXTpFWRWORpO38dmZgvLyJh0hedADzPgfPmOhrsv_ChG3AgJZy-pstKT5A";
            string recaptcha_response_field = "69999498 442";
            string parseurl = "1";
            string emailupdate = "9999";
            string folderid = "0";
            string rating = "0";
            string sbutton = "Submit reply";
            string url = string.Format(@"http://www.kaskus.co.id/post_reply/{0}", thread_id);

            StringBuilder postData = new StringBuilder();
            postData.Append((String.Format("securitytoken={0}&", securityToken)));
            postData.Append((String.Format("title={0}&", title)));
            postData.Append((String.Format("message={0}&", message)));
            postData.Append((String.Format("ajaxhref={0}&", ajaxhref)));
            postData.Append((String.Format("iconid={0}&", iconid)));
            postData.Append((String.Format("humanverify[hash]={0}&", humanverify_hash)));
            postData.Append((String.Format("recaptcha_challenge_field={0}&", recaptcha_challenge_field)));
            postData.Append((String.Format("recaptcha_response_field={0}&", recaptcha_response_field)));
            postData.Append((String.Format("parseurl={0}&", parseurl)));
            postData.Append((String.Format("emailupdate={0}&", emailupdate)));
            postData.Append((String.Format("folderid={0}&", folderid)));
            postData.Append((String.Format("rating={0}&", rating)));
            postData.Append((String.Format("sbutton={0}&", sbutton)));
            //save to mongodb
            var item_message = new Models.MessageModel
            {
                ajaxhref = ajaxhref,
                humanverify_hash = humanverify_hash,
                iconid = iconid,
                message = message,
                parseurl = parseurl,
                recaptcha_challenge_field = recaptcha_challenge_field,
                recaptcha_response_field = recaptcha_response_field,
                securitytoken = securityToken,
                thread_id = thread_id,
                title = title
            };
            mongoDBRepository = new MongoDBRepository();
            mongoDBRepository.CreateMessageModel(item_message);

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
            byte[] data = Encoding.ASCII.GetBytes(postData.ToString());
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
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