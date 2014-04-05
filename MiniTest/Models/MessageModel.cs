using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MiniTest.Models
{
    public class MessageModel
    {
        public MessageModel(){}

        public long Id { get; set; }
        [Display(Name="Message")]
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}