using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class BeneficiaryModel : Receiver
    {
        public String securityToken { get; set; }
        public string province { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "invalid input")]
        public string firstName { get; set; }

        [StringLength(30, MinimumLength = 2, ErrorMessage = "Please input 2 to 30 characters long")]
        public string midlleName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Please input 2 to 30 characters long")]
        public string lastname { get; set; }

        [Required]
        public string gender { get; set; }

        [RegularExpression(@"^[a-zA-ZñÑ0-9 .,-]+$", ErrorMessage = "Please input valid characters.")]
        [Required]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "invalid input")]
        public string city { get; set; }

        [Required]
        [StringLength(5, MinimumLength = 4, ErrorMessage = "invalid input")]
        public string zipcode { get; set; }

        [Required]
        public string dateOfBirth { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Please input 10 digit number")]
        public string phoneNo { get; set; }
        [RegularExpression(@"^[a-zA-ZñÑ0-9 .,-]+$", ErrorMessage = "Please input valid characters.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Minimum of 2 characters long")]
        public string relation { get; set; }

        public string receiverCustID { get; set; }

        public string SenderCustID { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-ZñÑ0-9 .,-]+$", ErrorMessage = "Please input valid characters.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Minimum of 2 characters long")]
        public string street { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Minimum of 2 characters long")]
        public string country { get; set; }

        public string strBase64Image { get; set; }

        public string ImagePath { get; set; }

    }

    public class BeneficiaryResponse 
    {

        public Int32 respcode { get; set; }

        public String message { get; set; }
        public BeneficiaryModel data { get; set; }
    }

    public abstract class Receiver 
    {
        
        public string address { get; set; }

                                         
    
    }


}