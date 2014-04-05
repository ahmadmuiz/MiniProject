using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniTest.Models
{
    public class UserModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SecurityToken { get; set; }
        public string url { get; set; }
        public string MD5Password { get; set; }
        public string MD5Password_UTF { get; set; }
    }
}