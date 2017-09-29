using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data;

namespace PayNearMe.Models
{
    public class PendingTransaction
    {

        public String Userid { get; set; }
        public DataTable table = new DataTable("choki");
        public String controlnumber { get; set; }
        public String SFirstname { get; set; }
        public String SMiddlename { get; set; }
        public String SLastname { get; set; }
        public String RFirstname { get; set; }
        public String RMiddlename { get; set; }
        public String RLastname { get; set; }
        public String orderChargeML { get; set; }
        public String orderChargePNM { get; set; }
        public String ExchangeRate { get; set; }
        public Int32 Count { get; set; }

        public Int32 This { get; set; }

        public String receiverID { get; set; }

        public List<TransactionDetailsM> tl = new List<TransactionDetailsM>();


        //-----------------------------------

        //For Pending Class

        public String kptn { get; set; }
        public String orderTotalAmount { get; set; }
        public String Prinicipal { get; set; }
        public String Status { get; set; }
        public String TransDate { get; set; }
        public String PayouAmount { get; set; }
        public String TrackingURL { get; set; }

    }

}