using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class TransactionModel
    {
        
        public String TransactionType { get; set; }
        public String KPTN { get; set; }
        public String TransDate { get; set; }
        public String securityToken { get; set; }
        [Range(1,1000)]
        [Required]
        public Double Principal { get; set; }
        [Required]
        public Double Charge { get; set; }
        [Required]
        public Double ExchangeRate { get; set; }
        public Double vat { get; set; }
        public Double POAmount { get; set; }
        [Required]
        public Double POAmountPHP { get; set; }
        [Required]
        public Double Total { get; set; }
       
        public String controlNo { get; set; }
       
        public String senderCustID { get; set; }
       
        public String receiverCustId { get; set; }

        public String PaymentIdentifier { get; set; }
   


    }

    public class SendoutModel 
    {

        public SendoutModel(TransactionModel transaction) 
        {
            this.transaction = transaction;
   
        }


        public String Username { get; set; }
        public String Password { get; set; }


        public String OperatorID { get; set; }
        public Int32 type { get; set; }
        public Int32 series { get; set; }
        public String pocurrency { get; set; }
        public String paymenttype { get; set; }
        public String trxntype { get; set; }
        public Int32 zonecode { get; set; }
        public String branchcode { get; set; }
        public String Currency { get; set; }
        public String syscreator { get; set; }

        public String station { get; set; }

        public TransactionModel transaction { get; set; }

    }

 
}