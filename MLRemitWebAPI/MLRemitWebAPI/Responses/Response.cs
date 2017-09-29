using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    
    

    public class ChargeResponse
    {
        public int respcode { get; set; }
        public String message { get; set; }
        public Decimal charge { get; set; }
        public String ErrorDetail { get; set; }
        public List<ChargeList> listofcharges { get; set; }
    }


    public class ChargeList 
    {
        public double minAmount { get; set; }
        public double maxAmount { get; set; }
        public double chargeValue { get; set; }

    }
  
}