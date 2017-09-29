﻿using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

/// <summary>
/// Summary description for AddKYCResponse
/// </summary>
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



