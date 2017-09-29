using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace PayNearMe.Models
{
    public class TransactionHistory
    {
        public Int32 Count { get; set; }

        public Int32 This { get; set; }

        public List<TransactionDetailsM> tl = new List<TransactionDetailsM>();

        public List<SelectListItem> month { get; set; }
        public String yearValue { get; set; }

        public String monthValue { get; set; }

        public String kptn { get; set; }
        public String TotalAmount { get; set; }
        public String Prinicipal { get; set; }
        public String Status { get; set; }
        public String TransDate { get; set; }
        public String PayouAmount { get; set; }
        public String TrackingURL { get; set; }
        public String orderChargeML { get; set; }

        
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleButtonAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var isValidName = false;
            var keyValue = string.Format("{0}:{1}", Name, Argument);
            var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

            if (value != null)
            {
                controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                isValidName = true;
            }

            return isValidName;
        }
    }
}