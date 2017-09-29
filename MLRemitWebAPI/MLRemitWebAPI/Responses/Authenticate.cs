using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class Authenticate
    {
      
        public String locale { get; set; }
        public String userId { get; set; }
        public String authenticationCode { get; set; }

    }

    public class AuthenticateResponse 
    {
        public Int32 respcode { get; set; }
        public String message { get; set; }

        public String password { get; set; }

        public String userID { get; set; }
    }

    public class AuthenticateRequest
    {
        public String securityToken { get; set; }
        public String UserID { get; set; }
        public String ActivationCode { get; set; }
    }
}