using System.ComponentModel.DataAnnotations;

namespace PayNearMe.Models
{



    public class LoginViewModel
    {

        public string securityToken { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

 
}
