using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace PayNearMe.Models.Response
{
    public class ProfileResponse
    {
        public ProfileResponse() 
        {

            sender = new CustomerModel();
        }
        public Int32 respcode { get; set; }
        public String message { get; set; }
        public CustomerModel sender { get; set; }

        public List<SelectListItem> IDs { get; set; }
        

    }

    public class getbrachrateclassificationresponse
    {
        public String rescode { get; set; }
        public String msg { get; set; }
        public String bcode { get; set; }
        public String bname { get; set; }
        public String zone { get; set; }
        public String classification { get; set; }
        public String description { get; set; }
        public String buying { get; set; }
        public String selling { get; set; }

    }

    public class getbranchratesresponse
    {
        public String rescode { get; set; }
        public String msg { get; set; }
        public Decimal buying { get; set; }
        public Decimal selling { get; set; }
        public String branchcode { get; set; }
        public String branchname { get; set; }
        public String currency { get; set; }
    }
}