using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLRemitWebAPI.Responses
{
    public class Reponses
    {
    }

    public class AddKYCResponse
    {
        public Int32 respcode { get; set; }
        public String message { get; set; }
        public String ErrorDetail { get; set; }
        public String MLCardNo { get; set; }

        public zipCodeResp zCodeResp { get; set; }


    }

    public class zipCodeResp
    {

        public String State { get; set; }
        public String City { get; set; }
        public String Abbr { get; set; }
    }
}