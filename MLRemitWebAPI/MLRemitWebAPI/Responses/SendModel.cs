using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace PayNearMe.Models
{
    public class SendModel
    {

        public SendModel(TransactionModel trans) 
        {
            this.trans = trans;
        }

        public SendModel() 
        {
            trans = new TransactionModel();
        }
        public TransactionModel trans { get; set; }

        [Required]
        public  String receiver { get; set; }

        public String siteIdentifier { get; set; } 
        public List<SelectListItem> beneficiarylist {get; set;}

        public String Error { get; set; }

    }



   
}

