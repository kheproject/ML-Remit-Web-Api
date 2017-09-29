using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models
{
    public class HomeModel
    {
        public String fullName { get; set; }
        public String memberSince { get; set; }

        public String lastLogin { get; set; }
        public String ExchangeRate { get; set; }
        public String DailyLimit { get; set; }
        public String MonthlyLimit { get; set; }
        public List<TransactionDetailsM> tl { get; set; }

    }
}