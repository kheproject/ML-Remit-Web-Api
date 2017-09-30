using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AESEncrypt;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Configuration;
using log4net;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Data;

namespace MLRemitWebAPI.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private String encStringKey = "B905BD7BFBD902DCB115B327F9018CEA";
        private AESEncryption encdata = new AESEncryption();
        private String connection = string.Empty;
        IDictionary config;
        private ILog kplog;

        private String smtpServer = string.Empty;
        private String smtpUser = String.Empty;
        private String smtpSender = String.Empty;
        private String smtpPass = String.Empty;
        private Boolean smtpSsl;

        public ForgotPasswordController()
        {
            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            connection = config["globalcon"].ToString();
            kplog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            smtpServer = config["smtpServer"].ToString();
            smtpUser = config["smtpUser"].ToString();
            smtpPass = config["smtpPass"].ToString();
            smtpSender = config["smtpSender"].ToString();
            smtpSsl = Convert.ToBoolean(config["smtpSsl"]);
        }

        //
        // GET: /ForgotPassword/
        public ActionResult fprstpswrd(String e, String sc, String cid, String fn)
        {
            if (e == string.Empty && sc == string.Empty &&
                fn == string.Empty && cid == string.Empty)
            {
                ViewBag.Keyword = "Error!";
                ViewBag.message = "Invalid link, Are you missing something?";
            }
            else
            {
                try
                {
                    e = encdata.AESDecrypt(e.Replace(' ', '+'), encStringKey);
                    sc = encdata.AESDecrypt(sc.Replace(' ', '+'), encStringKey);
                    cid = encdata.AESDecrypt(cid.Replace(' ', '+'), encStringKey);
                    fn = encdata.AESDecrypt(fn.Replace(' ', '+'), encStringKey);
                    using (MySqlConnection con = new MySqlConnection(connection))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM kpcustomersglobal.PayNearMe "
                                            + "WHERE UserID = @e AND FullName = @fn AND CustomerID = @cid AND securityCode = @sc;";
                            cmd.Parameters.AddWithValue("e", e);
                            cmd.Parameters.AddWithValue("fn", fn);
                            cmd.Parameters.AddWithValue("cid", cid);
                            cmd.Parameters.AddWithValue("sc", sc);

                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                if (updateAndNotifyUser(e, sc, fn, cid))
                                {
                                    ViewBag.Keyword = "Success!";
                                    ViewBag.message = "Your password has been resetted. A mail has been sent to your email address with your new password, please do check. Thank You!";
                                }
                                else
                                {
                                    ViewBag.Keyword = "Failed!";
                                }
                            }
                            else
                            {
                                throw new Exception("Unable to find user....");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Keyword = "System Error!";
                    ViewBag.message = "Invalid link, this could be due to the link you have browsed is expired. Are you missing something?";
                    kplog.Error("ForgotPasswordController[fprstpswrd] email :" + e + ", CustomerID :" + cid + ", fullName :" + fn 
                               + ", security code :" + sc + " ─ System Error ─" + ex.Message);
                }
            }
            return PartialView();
        }

        private Boolean updateAndNotifyUser(String e, String sc, String fn, String cid)
        {
            Random random = new Random();
            const string chars = "9AB8CD7EF6GH5IJ4KL3MN2OP1QR0ST9UV8WX7YZ6012345";
            String npasswrd = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                using (MySqlCommand cmd = con.CreateCommand())
                {   
                    cmd.Transaction = trans;
                    try
                    {
                        cmd.CommandText = "UPDATE `kpcustomersglobal`.`PayNearMe` "
                                        + "SET securityCode = null, Password = @npasswrd "
                                        + "WHERE UserID = @e AND securityCode = @sc AND CustomerID = @cid AND FullName = @fn;";
                        cmd.Parameters.AddWithValue("e", e);
                        cmd.Parameters.AddWithValue("sc", sc);
                        cmd.Parameters.AddWithValue("fn", fn);
                        cmd.Parameters.AddWithValue("cid", cid);
                        cmd.Parameters.AddWithValue("npasswrd", npasswrd);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            if (sendSecurityCode(e, npasswrd, fn))
                            {
                                trans.Commit();
                                return true;
                            }
                            else
                            {
                                ViewBag.message = "Sorry, was unable to send mail, Please try again by refreshing this page...";
                                trans.Rollback();
                            }   
                        }
                        else
                            ViewBag.message = "Server was unable to update your request, Please try again by refreshing the page.";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.message = "An error has occured, please try again. If the problem persist please contact admin...";
                        kplog.Error("ForgotPasswordController[updateAndNotifyUser] email :" + e + ", CustomerID :" + cid + ", fullName :" + fn
                               + ", security code :" + sc + " ─ System Error ─" + ex.Message);
                    }
                }
            }
            return false;
        }

        private Boolean sendSecurityCode(String email, String newPassword, String FullName)
        {
            
            SmtpClient client = new SmtpClient();
            client.EnableSsl = smtpSsl;
            client.UseDefaultCredentials = true;
            client.Host = smtpServer;
            client.Port = 587;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            MailMessage msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress("ML Remit<" + smtpSender + ">");
            msg.Subject = "ML Remit - Password Reset!";
            msg.Body = "<div style=\"font-size: 16px; font-family: Consolas; text-align: justify; margin: 0 auto; width: 500px; color: black; padding: 20px; border-left: 1px solid #FFF0CA; border-right: 1px solid #FFF0CA; border-radius: 20px;\">"
                     + "<p> Good day Ma'am/Sir <b>" + FullName + "</b>,</p>"
                     + "<p>"
                     + "<b>M. Lhuillier</b> goes online with "
                     + "<b>ML Remit</b> - fast - convenient - safe."
                     + "</p><br />"
                     + "<p>You have successfully reset your password. </p>"
                     + "Your new password is <br />"
                     + "<span style=\"font-size: 20px;\"> " + newPassword + "</span> <br />"
                     + "You may now access your account with the new given password and change it to your desired password. Thank You!"
                     + "<br /><br />"
                     + "<div style=\"font-size: 14px; border-top: 1px solid lightgray; text-align: center; padding-top: 5px; background-color: gray;\">"
                     + "-- This mail is auto generated. Please do not reply. --"
                     + "</div></div>";
            msg.IsBodyHtml = true;

            Boolean isSent = false;
            for (int retries = 0; retries < 3; retries++)
            {
                try
                {
                    client.Send(msg);
                    isSent = true;
                    retries = 3;
                }
                catch (Exception err)
                {
                    if (retries == 2)
                        kplog.Error(err.ToString());
                    if (retries < 2)
                        Thread.Sleep(1300); //Delay for 1.3seconds
                }
            }
            return isSent;
        }
	}
}