using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class ChangePasswordModel
    {
        public String currentPassword { get; set; }
        public String newPassword { get; set; }
        public String confirmPassword { get; set; }
        public String UserID { get; set; }
        public String securityToken { get; set; }
    }
}