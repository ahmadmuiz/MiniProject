using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MiniTest.Controllers
{
    public class BaseController : Controller
    {
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
                return Convert.ToBoolean( Session["IsAuthenticated"].ToString());
            }
            set
            {
                Session["IsAuthenticated"] = value;
            }
        }

    }
}