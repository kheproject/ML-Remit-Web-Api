using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayNearMe.Models
{


    public abstract class Sender 
    {
        
        public String CustomerID { get; set; }
        public String BranchID { get; set; }

        public String activationCode { get; set; }

        public String ImagePath { get; set; }

        public String ImagePath1 { get; set; }

        public String ImagePath2 { get; set; }

        public String ImagePath3 { get; set; }

        public String ImagePath4 { get; set; }

        public String CreatedBy { get; set; }

        public String strBase64Image1F { get; set; }

        public String strBase64Image1B { get; set; }

        public String strBase64Image2F { get; set; }

        public String strBase64Image2B { get; set; }

    }
    public class CustomerModel : Sender
    {

        public String securityToken { get; set; }
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Please input 2 to 30 characters long.")]
        [Required]
        [Display(Name = "First Name")]
        public String firstName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Please input 2 to 30 characters long.")]
        public String lastName { get; set; }
        [Display(Name = "Middle Name")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Please input 2 to 30 characters long.")]
        public String middleName { get; set; }

        [StringLength(120, MinimumLength = 2, ErrorMessage = "Please input 2 to 120 characters long.")]
        [Display(Name = "Street")]
        [RegularExpression(@"^[a-zA-ZñÑ0-9 .,-]+$", ErrorMessage = "Please input valid characters.")]
        [Required]
        public String Street { get; set; }
        [Required]
        [Display(Name = "City")]
        public String City { get; set; }
        [Required]
        [Display(Name = "State")]
        public String State { get; set; }
        [Required]
        public String StateAbbr { get; set; }
        [Required]
        public String Country { get; set; }
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Please input 5 characters long.")]
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "ZipCode must be numeric")]
        public String ZipCode { get; set; }
        [Required]
        public String Gender { get; set; } 
        [Required]
        public String BirthDate { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Please input 10 characters long.")]
        [Required]
        public String PhoneNo { get; set; }

        [StringLength(50, MinimumLength = 5, ErrorMessage = "Please input 5 to 50 characters long.")]
        [Display(Name = "Email Address")]
        [Required]
        public String UserID { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "Please input 8 to 50 characters long.")]
       [Required]
        public String Password { get; set; }
        
        public String strBase64Image { get; set; }

      
        [Display(Name = "ID Number")]
        [StringLength(20, MinimumLength=2, ErrorMessage = "Please input 2 to 20 characters long.")]
        [Required]
        public String IDNo { get; set; }

        
        [Required]
        public String IDType { get; set; }
        [Required]
        public String ExpiryDate { get; set; }

        public Boolean sendSMS { get; set; }

      
    }
}