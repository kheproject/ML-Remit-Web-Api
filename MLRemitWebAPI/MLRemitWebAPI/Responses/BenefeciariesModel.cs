using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class BenefeciariesModel
    {
      
        public List<Benefeciary> benefeciaries { get; set; }

    }

    public class Benefeciary 
    {
        public String HeaderName { get; set; }
        public String ReceiverCustID { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        public String Phone { get; set; }
        public String Payer { get; set; }
    
    }
}