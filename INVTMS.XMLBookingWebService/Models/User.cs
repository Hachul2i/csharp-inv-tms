using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INVTMS.XMLBookingWebService.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Token { get; set; }
    }
}