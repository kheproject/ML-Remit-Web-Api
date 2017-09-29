using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PayNearMe.Models.Response
{
    public class TransactionResponse
    {
        public String message { get; set; }
        public String ErrorDetail { get; set; }
        public Int32 respcode { get; set; }

        public String result { get; set; }

        public Int32 count { get; set; }

        public List<TransactionDetailsModel> tl { get; set; }

        public TransactionDetailsModel detail { get; set; }
    }

    public class TransactionResponseMobile 
    {
        public String message { get; set; }
        public Int32 respcode { get; set; }
        public List<TransactionDetailsM> tl { get; set; }

        public Int32 count { get; set; }
    }
}