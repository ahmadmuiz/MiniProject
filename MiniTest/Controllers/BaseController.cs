using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        protected string GetSecurityToken(string url)
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

    }
}