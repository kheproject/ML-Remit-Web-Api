using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace PayNearMe.Models
{
    public class RegisterModel
    {
        public CustomerModel data { get; set; }

        public RegisterModel() 
        {
            data = new CustomerModel();
        }

        public List<SelectListItem> IDTypeDropDownList { get; set; }

        [Required]
        public Boolean privacyPolicyAgreement { get; set; }
        public String userIPAddress { get;set;}

        public String errorMessage { get; set; }

    }

    public class ListOfac
    {
        public String KPTN { get; set; }
        public String fullName { get; set; }
        public String uid { get; set; }
        public String firstname { get; set; }
        public String lastname { get; set; }
        public String sdntype { get; set; }
        public String dateOfBirth { get; set; }
        public String placeofbirth { get; set; }
        public String alias { get; set; }
        public String soundexvalue { get; set; }
        public Int32 score { get; set; }
        public String PersonOnHold { get; set; }
    }

}