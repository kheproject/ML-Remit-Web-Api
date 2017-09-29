using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models.Response
{
    public class LoginResponse
    {
        public String message { get; set; }
        public String ErrorDetail { get; set; }
        public Int32 respcode { get; set; }

        public String fullName { get; set; }

        public String signupDate { get; set; }

        public String lastLogin { get; set; }

        public CustomerModel customer { get; set; }
    }
}