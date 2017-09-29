using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models.Response
{
  

    public class SendoutResponse
    {
    public int respcode { get; set; }
    //public String respmsg;
    public String message { get; set; }
    public String ErrorDetail { get; set; }
    public String kptn { get; set; }
    public String orno { get; set; }
    public DateTime transdate { get; set; }
    public String controlno { get; set; }


    }

}