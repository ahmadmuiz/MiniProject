using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB.Bson;

namespace MiniTest.Models
{
    public class MessageModel
    {
        public MessageModel() { }

        public ObjectId _id { get; set; }
        public string thread_id { get; set; }
        public string securitytoken { get; set; }
        public string title { get; set; }
        [Display(Name = "Message")]
        public string message { get; set; }
        public string ajaxhref { get; set; }
        public string iconid { get; set; }
        public string humanverify_hash { get; set; }
        public string recaptcha_challenge_field { get; set; }
        public string recaptcha_response_field { get; set; }
        public string parseurl { get; set; }
    }
}