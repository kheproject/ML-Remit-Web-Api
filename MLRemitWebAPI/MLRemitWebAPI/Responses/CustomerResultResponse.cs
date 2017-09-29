using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class CustomerResultResponse
    {
        public int respcode { get; set; }
        public string message { get; set; }
        public string ErrorDetail { get; set; }

        public String receiverCustID { get; set; }
        public List<BeneficiaryModel> benelist { get; set; }
        //public string rcvid { get; set; }
    }
}