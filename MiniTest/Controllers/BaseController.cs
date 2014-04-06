using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;

namespace MiniTest.Controllers
{
    public class BaseController : Controller
    {

        protected CookieCollection Cookies
        {
            get
            {
                if (Session["Cookies"] == null) return new CookieCollection();
                return (Session["Cookies"] as CookieCollection);
            }
            set
            {
                Session["Cookies"] = value;
            }
        }

        /// <summary>
        /// handle Cookie Kaskus (reposnse.headers["Set-Cookie"])
        /// </summary>
        protected string CookieHeader
        {
            get
            {
                if (Session["CookieHeader"] == null) return string.Empty;
                return Session["CookieHeader"].ToString();
            }
            set
            {
                Session["CookieHeader"] = value;
            }
        }


        /// <summary>
        /// Parse  not well-formatted cookie, see http://stackoverflow.com/questions/15103513/httpwebresponse-cookies-empty-despite-set-cookie-header-no-redirect
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        protected CookieCollection parseCookie(HttpWebRequest request, WebHeaderCollection header)
        {
            CookieCollection collection = new CookieCollection();
            for (int i = 0; i < header.Count; i++)
            {
                string name = header.GetKey(i);
                if (name != "Set-Cookie")
                    continue;
                string value = header.Get(i);
                foreach (var singleCookie in value.Split(','))
                {
                    Match match = Regex.Match(singleCookie, "(.+?)=(.+?)[^,];");
                    if (match.Captures.Count == 0)
                        continue;
                    match = Regex.Match(singleCookie, "(.+?)=(.+?);");
                    collection.Add(
                       new Cookie(
                           match.Groups[1].ToString(),
                           match.Groups[2].ToString(),
                           "/",
                           request.Host.Split(':')[0]));
                }
            }
            return collection;
        }


        protected bool SessionAdministrator
        {
            get
            {
                bool result = false;
                if (Session["SessionAdministrator"] == null) return false;
                bool.TryParse(Session["SessionAdministrator"].ToString(), out result);
                return result;
            }
            set
            {
                Session["SessionAdministrator"] = value;
            }
        }

        protected string ApplicationId
        {
            get
            {
                if (Session["ApplicationId"] == null) return string.Empty;
                return Session["ApplicationId"].ToString();
            }
            set
            {
                Session["ApplicationId"] = value;
            }
        }


        protected bool IsAuthenticated
        {
            get
            {
                if (Session["IsAuthenticated"] == null) return false;
                return Convert.ToBoolean(Session["IsAuthenticated"].ToString());
            }
            set
            {
                Session["IsAuthenticated"] = value;
            }
        }

        /// <summary>
        /// get security token from kaskus, use htmlagility
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string GetLoginSecurityToken(string url)
        {
            HtmlWeb Web = new HtmlWeb();
            HtmlDocument Doc = Web.Load(url);
            Doc.OptionOutputAsXml = true;
            Doc.OptionAutoCloseOnEnd = true;
            Doc.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            //get security token element
            HtmlNodeCollection node;
            node = Doc.DocumentNode.SelectNodes("//input[./@name=\"securitytoken\"]");
            if (node != null && node.Count > 0)
            {
                return node.First().Attributes["value"].Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Load HTML content, 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected HtmlDocument getUrlContent(string url)
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
            byte[] data = Encoding.ASCII.GetBytes("content=");
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
            HtmlDocument Doc = new HtmlDocument();
            Doc.LoadHtml(result);
            Doc.OptionOutputAsXml = true;
            Doc.OptionAutoCloseOnEnd = true;
            Doc.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            return Doc;
        }

        protected string getUrlCaptchaImage(HtmlDocument Doc)
        {
            HtmlNodeCollection node;
            node = Doc.DocumentNode.SelectNodes("//img[./@id=\"recaptcha_challenge_image\"]");
            if (node != null && node.Count > 0)
            {
                return node.First().Attributes["src"].Value;
            }
            return string.Empty;
        }

        protected string getSecurityToken(HtmlDocument Doc)
        {
            HtmlNodeCollection node;
            node = Doc.DocumentNode.SelectNodes("//input[./@name=\"securitytoken\"]");
            if (node != null && node.Count > 0)
            {
                return node.First().Attributes["value"].Value;
            }
            return string.Empty;

        }

        protected string getHumanVerifyHash(HtmlDocument Doc)
        {
            HtmlNodeCollection node;
            node = Doc.DocumentNode.SelectNodes("//input[./@name=\"humanverify[hash]\"]");
            if (node != null && node.Count > 0)
            {
                return node.First().Attributes["value"].Value;
            }
            return string.Empty;
        }

        protected string getRecaptchaChallengeField(HtmlDocument Doc)
        {
            HtmlNodeCollection node;
            node = Doc.DocumentNode.SelectNodes("//input[./@id=\"recaptcha_challenge_field\"]");
            if (node != null && node.Count > 0)
            {
                return node.First().Attributes["value"].Value;
            }
            return string.Empty;
        }

    }
}