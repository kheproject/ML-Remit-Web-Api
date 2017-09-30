using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PayNearMe.Models;
using PayNearMe.Models.Response;
using System.IO;
using System.Net.Security;
using System.Configuration;
using System.Collections;
using MySql.Data.MySqlClient;
using log4net;
using log4net.Config;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Threading;
using System.Web.Http.Controllers;
using System.Threading.Tasks;
using System.Globalization;
using CrystalDecisions.Shared;
using AESEncrypt;
using MLRemitWebAPI.Controllers;




namespace PayNearMe.Controllers.api
{
    public class WebServiceController : ApiController
    {

        IDictionary config;
        string server = string.Empty;
        private MySqlCommand command;
        private String connection = string.Empty;
        private String dbconofac = string.Empty;
        private String forex = string.Empty;
        private String PNMServer = string.Empty;
        private DateTime dt;
        private String secureToken = "TUxIVUlMTElFUkl0U2RrryloTnNZekI0OTVRUTZKNU1YcjAvTzFyST0";
        //
        //Forgot Password encrypt string key
        //Added by: Khevin R. Tulang
        private String encStringKey = "B905BD7BFBD902DCB115B327F9018CEA";
        private MySqlCommand custcommand;
        private MySqlTransaction custtrans = null;
        private String siteIdentifier = string.Empty;
        private String secretKey = string.Empty;
        private static double pnmCharge = 3.99;
        private String ftp = string.Empty;
        private String http = string.Empty;
        private ILog kplog;
        private double dailyLimit = 0.0;
        private double monthlyLimit = 0.0;
        private static readonly HttpClient client = new HttpClient();
        private String smtpServer = string.Empty;
        private String smtpUser = String.Empty;
        private String smtpSender = String.Empty;
        private String smtpPass = String.Empty;
        private Boolean smtpSsl;
        private String iDologyServer = string.Empty;
        private String iDologyUser = String.Empty;
        private String iDologyPass = String.Empty;
        private Boolean iDology = false;
        private AESEncryption encdata = new AESEncryption();

        public WebServiceController()
        {

            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            server = config["server"].ToString();
            connection = config["globalcon"].ToString();
            dbconofac = config["ofaccon"].ToString();
            dailyLimit = Convert.ToDouble(config["dailyLimit"]);
            monthlyLimit = Convert.ToDouble(config["monthlyLimit"]);
            ftp = config["ftp"].ToString();
            http = config["http"].ToString();
            kplog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            smtpServer = config["smtpServer"].ToString();
            smtpUser = config["smtpUser"].ToString();
            smtpPass = config["smtpPass"].ToString();
            smtpSender = config["smtpSender"].ToString();
            smtpSsl = Convert.ToBoolean(config["smtpSsl"]);
            siteIdentifier = config["siteIdentifier"].ToString();
            secretKey = config["secretKey"].ToString();
            forex = config["mlforexrate"].ToString();
            iDology = Convert.ToBoolean(config["iDology"]);
            iDologyServer = config["iDologyServer"].ToString();
            iDologyUser = config["iDologyUser"].ToString();
            iDologyPass = config["iDologyPass"].ToString();

        }

        //done loggings
        [HttpPost]
        public CustomerResultResponse insertbeneficiary(BeneficiaryModel bene)
        {
            kplog.Info("START--- > PARAMS: " + JsonConvert.SerializeObject(bene));

            if (bene.securityToken != secureToken) 
            {
                kplog.Info(getRespMessage(7));
                return new CustomerResultResponse { respcode = 7, message = getRespMessage(7) };
            }

            String sendercustid = bene.SenderCustID;
            String rcvrfirstname = cleanString(bene.firstName);
            String rcvrlastname = cleanString(bene.lastname);
            String rcvrmiddlename = cleanString(bene.midlleName);
            String rcvrcountry = bene.country;
            String rcvrstreet = cleanString(bene.street);
            String rcvrcitystate = cleanString(bene.city);
            String rcvrzipcode = bene.zipcode;
            String rcvrbirthdate = bene.dateOfBirth;
            String rcvrgender = bene.gender;
            String rcvrrelation = cleanString(bene.relation);
            String rcvrcontactno = cleanString(bene.phoneNo);
            String rcvrcustid = bene.receiverCustID;
            String uploadpath = string.Empty;
            String filepath = string.Empty;

            try
            {
                dt = getServerDateGlobal1(false);
            }
            catch (Exception ex)
            {


                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();

                        MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                        Int32 sr = 0;
                        Int32 srkyc = 0;
                        Int32 bkyc = 0;
                        string updatebeneficiary = String.Empty;
                        string updatecustomerseries = String.Empty;
                        string benecustid = String.Empty;
                        string benecustidkyc = String.Empty;
                        string benecustidkycsame = String.Empty;
                        string rcvid = String.Empty;
                        using (command = con.CreateCommand())
                        {

                            try
                            {
                                string checking = String.Empty;

                                if (rcvrmiddlename == null)
                                {
                                    checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory where sendercustid=@sendercustid and firstname=@firstname and lastname=@lastname;";
                                    rcvrmiddlename = "";
                                }
                                else
                                {
                                    checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory where sendercustid=@sendercustid and firstname=@firstname and lastname=@lastname and middlename=@middlename;";
                                }
                                command.Transaction = trans;
                                command.CommandText = checking;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("sendercustid", sendercustid);
                                command.Parameters.AddWithValue("firstname", rcvrfirstname);
                                command.Parameters.AddWithValue("lastname", rcvrlastname);
                                command.Parameters.AddWithValue("middlename", rcvrmiddlename);
                                MySqlDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    reader.Close();

                                    con.Close();
                                    kplog.Info("Beneficiary Already Exist");
                                    return new CustomerResultResponse { respcode = 0, message = "Beneficiary Already Exist" };
                                }
                                else
                                {

                                    reader.Close();

                                    //KYC series
                                    String querykycseries = "select series from kpformsglobal.customerseries";
                                    command.CommandText = querykycseries;
                                    command.Parameters.Clear();
                                    MySqlDataReader rdrkyc = command.ExecuteReader();
                                    if (rdrkyc.HasRows)
                                    {
                                        rdrkyc.Read();
                                        if (!(rdrkyc["series"] == DBNull.Value))
                                        {
                                            srkyc = Convert.ToInt32(rdrkyc["series"].ToString());
                                        }
                                    }
                                    rdrkyc.Close();

                                    Int32 sr1kyc = srkyc + 1;
                                    if (srkyc == 0)
                                    {
                                        updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1kyc + "','" + dt.ToString("yyyy") + "')";
                                    }
                                    else
                                    {
                                        updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1kyc + "', year = '" + dt.ToString("yyyy") + "'";
                                    }
                                    command.CommandText = updatecustomerseries;
                                    command.ExecuteNonQuery();
                                    benecustidkyc = generateCustIDGlobal(command);


                                    //Beneficiary series
                                    string slctmaxseries = "select series from kpformsglobal.beneficiaryseries";
                                    command.CommandText = slctmaxseries;
                                    command.Parameters.Clear();
                                    MySqlDataReader rdrseries = command.ExecuteReader();
                                    if (rdrseries.HasRows)
                                    {
                                        rdrseries.Read();
                                        if (!(rdrseries["series"] == DBNull.Value))
                                        {
                                            sr = Convert.ToInt32(rdrseries["series"].ToString());
                                        }
                                    }
                                    rdrseries.Close();

                                    Int32 sr1 = sr + 1;
                                    if (sr == 0)
                                    {
                                        updatebeneficiary = "INSERT INTO kpformsglobal.beneficiaryseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                                    }
                                    else
                                    {
                                        updatebeneficiary = "update kpformsglobal.beneficiaryseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                                    }
                                    command.CommandText = updatebeneficiary;
                                    command.ExecuteNonQuery();
                                    benecustid = generateBeneficiaryCustIDGlobal(command);
                                }
                                reader.Close();

                                command.CommandText = "INSERT INTO kpcustomersglobal.BeneficiaryHistory(custidb, custids, firstname, lastname, middlename, fullname, street, citystate, country, zipcode, birthdate, gender, relation, contactno, sendercustid, lasttransdate) values(@custidb, @custids, @firstname, @lastname, @middlename, @fullname, @street, @citystate, @country, @zipcode, @birthdate, @gender, @relation, @contactno, @sendercustid, now())";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("custidb", benecustid);
                                command.Parameters.AddWithValue("custids", bkyc == 1 ? benecustidkycsame : benecustidkyc);
                                command.Parameters.AddWithValue("firstname", rcvrfirstname);
                                command.Parameters.AddWithValue("lastname", rcvrlastname);
                                command.Parameters.AddWithValue("middlename", rcvrmiddlename);
                                command.Parameters.AddWithValue("fullname", rcvrlastname + ", " + rcvrfirstname + " " + rcvrmiddlename);
                                command.Parameters.AddWithValue("street", rcvrstreet);
                                command.Parameters.AddWithValue("citystate", rcvrcitystate);
                                command.Parameters.AddWithValue("country", rcvrcountry);
                                command.Parameters.AddWithValue("zipcode", rcvrzipcode);
                                command.Parameters.AddWithValue("birthdate", rcvrbirthdate == String.Empty ? null : Convert.ToDateTime(rcvrbirthdate).ToString("yyyy-MM-dd"));
                                command.Parameters.AddWithValue("gender", rcvrgender);
                                command.Parameters.AddWithValue("relation", rcvrrelation);
                                command.Parameters.AddWithValue("contactno", rcvrcontactno);
                                command.Parameters.AddWithValue("sendercustid", sendercustid);
                                int y = command.ExecuteNonQuery();
                                command.Parameters.Clear();

                                kplog.Info("Success kpcustomersglobal.BeneficiaryHistory");

                                if (!string.IsNullOrEmpty(bene.strBase64Image))
                                {
                                    String filename = getTimeStamp().ToString() + ".png";
                                    uploadpath = ftp + "/PayNearMe/Images/" + filename;
                                    uploadFileImage(bene.strBase64Image, uploadpath);
                                    filepath = http + "/PayNearMe/Images/" + filename;

                                }
                                command.CommandText = "INSERT INTO kpcustomersglobal.BeneficiaryPayNearMe(ReceiverCustID,isActivate,ImagePath,Province) VALUES (@benecustid,'1',@imagePath,@province)";
                                command.Parameters.AddWithValue("benecustid", benecustid);
                                command.Parameters.AddWithValue("imagePath", filepath);
                                command.Parameters.AddWithValue("province", bene.province);
                                int x = command.ExecuteNonQuery();

                                kplog.Info("Success kpcustomersglobal.BeneficiaryPayNearMe");





                                if (y > 0 && x > 0)
                                {
                                    // PayNearMe API
                                    Int32 timestamp = getTimeStamp();
                                    string yearofbirth = Convert.ToDateTime(rcvrbirthdate).ToString("yyyy");

                                    string query = "city=" + rcvrcitystate + "&country=" + rcvrcountry + "&first_name=" + rcvrfirstname + "&last_name=" + rcvrlastname + "&middle_name=" + rcvrmiddlename + "&postal_code=" + rcvrzipcode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + benecustid + "&street=" + rcvrstreet + "&timestamp=" + timestamp.ToString() +
                                                    "&user_type=receiver&version=2.0&year_of_birth=" + yearofbirth;



                                    string signature = generateSignature(query);

                                    query = query + "&signature=" + signature;

                                    Uri uri = new Uri(server + "/json-api/create_user?" + query);

                                    string res = SendRequest(uri);
                                    kplog.Info("Response: PayNearMe API: create_user: " + res);

                                    dynamic data = JObject.Parse(res);

                                    if (data.status == "ok")
                                    {
                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Added");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary's information is successfully added!", receiverCustID = benecustid };
                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        con.Close();

                                        string error = "";
                                        for (int xx = 0; xx < data.errors.Count; xx++)
                                        {
                                            error = error + " " + data.errors[xx].description;
                                        }
                                        kplog.Error(error);
                                        return new CustomerResultResponse { respcode = 0, message = error };
                                    }


                                }

                                trans.Rollback();
                                con.Close();
                                kplog.Info("Error in Adding Beneficiary");
                                return new CustomerResultResponse { respcode = 0, message = "Error in Adding Beneficiary" };

                            }
                            catch (MySqlException myx)
                            {
                                trans.Rollback();
                                con.Close();
                                kplog.Fatal(myx.ToString());
                                return new CustomerResultResponse { respcode = 0, message = myx.Message, ErrorDetail = myx.ToString() };
                            }
                        }
                    }
                    catch (MySqlException mex)
                    {

                        con.Close();
                        kplog.Fatal(mex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = mex.ToString(), ErrorDetail = mex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString(), ErrorDetail = ex.ToString() };
            }
        }

        //done loggings RR
        [HttpPost]
        public CustomerResultResponse updateBeneficiary(BeneficiaryModel model)
        {
            kplog.Info("START -->  PARAMS: " + JsonConvert.SerializeObject(model));

            if (model.securityToken != secureToken) 
            {
                kplog.Error(getRespMessage(7));
                return new CustomerResultResponse { respcode = 0, message = getRespMessage(7) };
            }

            String rcvrfirstname = cleanString(model.firstName);
            String rcvrlastname = cleanString(model.lastname);
            String rcvrmiddlename = cleanString(model.midlleName);
            String rcvrstreet = cleanString(model.street);
            String rcvrcitystate = cleanString(model.city);
            String rcvrcountry = model.country;
            String rcvrzipcode = model.zipcode;
            String rcvrbirthdate = model.dateOfBirth;
            String rcvrgender = model.gender;
            String rcvrrelation = cleanString(model.relation);
            String rcvrcontactno = (model.phoneNo).Trim();
            String rcvrcustid = model.receiverCustID;

            try
            {
                dt = getServerDateGlobal1(false);
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        string updatebeneficiary = String.Empty;
                        string updatecustomerseries = String.Empty;
                        string benecustid = String.Empty;
                        string benecustidkyc = String.Empty;
                        string benecustidkycsame = String.Empty;
                        string rcvid = String.Empty;

                        using (command = con.CreateCommand())
                        {

                            try
                            {

                                string checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b on a.CustIDB = b.ReceiverCustID where a.CustIDB=@rcvrcustid and sendercustid=@sendercustid1";
                                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                                command.Transaction = trans;
                                command.CommandText = checking;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("rcvrcustid", rcvrcustid);
                                command.Parameters.AddWithValue("sendercustid1", model.SenderCustID);
                                MySqlDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    reader.Close();

                                    string uptblbeneficiary = "update kpcustomersglobal.BeneficiaryHistory set FirstName=@nrfname, LastName=@nrlname, MiddleName=@nrmname, FullName=@nrfullname, CityState=@nrcity, ZipCode=@nrzipcode, BirthDate=@nrbdate, Gender=@nrgender, Relation=@nrrelation, ContactNo=@nrcontact, lasttransdate = now(), Street = @nrstreet, Country = @nrcountry where sendercustid=@sendercustid1  and CustIDB=@rcvrcustid";
                                    command.CommandText = uptblbeneficiary;
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("sendercustid1", model.SenderCustID);
                                    command.Parameters.AddWithValue("nrfname", rcvrfirstname);
                                    command.Parameters.AddWithValue("nrlname", rcvrlastname);
                                    command.Parameters.AddWithValue("nrmname", rcvrmiddlename);
                                    command.Parameters.AddWithValue("nrfullname", rcvrlastname + ", " + rcvrfirstname + " " + rcvrmiddlename);
                                    command.Parameters.AddWithValue("nrcity", rcvrcitystate);
                                    command.Parameters.AddWithValue("nrzipcode", rcvrzipcode);
                                    command.Parameters.AddWithValue("nrbdate", rcvrbirthdate == String.Empty ? null : Convert.ToDateTime(rcvrbirthdate).ToString("yyyy-MM-dd"));
                                    command.Parameters.AddWithValue("nrgender", rcvrgender);
                                    command.Parameters.AddWithValue("nrrelation", rcvrrelation);
                                    command.Parameters.AddWithValue("nrcontact", rcvrcontactno);
                                    command.Parameters.AddWithValue("rcvrcustid", rcvrcustid);
                                    command.Parameters.AddWithValue("nrstreet", rcvrstreet);
                                    command.Parameters.AddWithValue("nrcountry", rcvrcountry);
                                    int x = command.ExecuteNonQuery();

                                    if (!string.IsNullOrEmpty(model.province)) 
                                    {
                                        command.Parameters.Clear();
                                        command.CommandText = "UPDATE kpcustomersglobal.BeneficiaryPayNearMe SET Province = @province where ReceiverCustID=@rCustId";
                                        command.Parameters.AddWithValue("province", model.province);
                                        command.Parameters.AddWithValue("rCustId", rcvrcustid);
                                        command.ExecuteNonQuery();
                                    }

                                    String filepath = string.Empty;
                                    String uploadpath = string.Empty;
                                    if (!string.IsNullOrEmpty(model.strBase64Image))
                                    {
                                        String filename = getTimeStamp().ToString() + ".png";
                                        uploadpath = ftp + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(model.strBase64Image, uploadpath);
                                        filepath = http + "/PayNearMe/Images/" + filename;


                                        command.Parameters.Clear();
                                        command.CommandText = "UPDATE kpcustomersglobal.BeneficiaryPayNearMe SET ImagePath = @image where ReceiverCustID=@rCustId";
                                        command.Parameters.AddWithValue("image", filepath);
                                        command.Parameters.AddWithValue("rCustId", rcvrcustid);
                                        command.ExecuteNonQuery();

                                        kplog.Info("Success Update kpcustomersglobal.BeneficiaryPayNearMe -- ImagePath ");
                                    }

                                    if (x > 0)
                                    {

                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Updated");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary's information is successfully updated!" };

                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        con.Close();
                                        kplog.Info("Error in Updating Beneficiary");
                                        return new CustomerResultResponse { respcode = 0, message = "Error in Updating Beneficiary" };
                                    }

                                }

                                else
                                {
                                    reader.Close();
                                    con.Close();
                                    kplog.Info("Beneficiary Not Found");
                                    return new CustomerResultResponse { respcode = 0, message = "Beneficiary Not Found" };
                                }
                            }
                            catch (Exception ex)
                            {
                                kplog.Fatal(ex.ToString());
                                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }
        }


        //done loggings RR
        [HttpGet]
        public CustomerResultResponse getbeneficiarylist(String sendercustid, String securityToken)
        {
            kplog.Info("START -->  PARAMS: sendercustid: " + sendercustid);
            try
            {
                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new CustomerResultResponse { respcode = 0, message = getRespMessage(7) };
                }

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select a.firstname, a.lastname, a.middlename, a.fullname, a.street, a.citystate, a.country, if(a.zipcode is null,'', a.zipcode) as zipcode, date_format(a.birthdate,'%Y-%m-%d') as birthdate, a.gender, a.contactno, a.Relation,a.CustIDB,b.ImagePath,b.Province from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b  ON a.CustIDB = b.ReceiverCustID where a.sendercustid=@sendercustid and b.isActivate = 1 order by LastTransDate DESC";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sendercustid", sendercustid);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    DateTime? dtcheck;
                                    try { dtcheck = Convert.ToDateTime(rdr["BirthDate"]); }
                                    catch (Exception) { dtcheck = null; }

                                    list.Add(new BeneficiaryModel
                                    {

                                        address = rdr["street"].ToString() + " " + rdr["citystate"].ToString() + " " + rdr["zipcode"].ToString() + " " + rdr["country"].ToString(),
                                        receiverCustID = rdr["CustIDB"].ToString(),
                                        firstName = rdr["firstname"].ToString(),
                                        lastname = rdr["lastname"].ToString(),
                                        midlleName = rdr["middlename"].ToString(),
                                        city = rdr["CityState"].ToString(),
                                        country = rdr["Country"].ToString(),
                                        zipcode = rdr["ZipCode"].ToString(),
                                        dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("MM/dd/yyyy") : "",
                                        gender = rdr["Gender"].ToString(),
                                        relation = rdr["Relation"].ToString(),
                                        phoneNo = rdr["ContactNo"].ToString(),
                                        street = rdr["street"].ToString(),
                                        province = rdr["Province"].ToString(),
                                        SenderCustID = sendercustid,
                                        ImagePath = rdr["ImagePath"].ToString()


                                    });



                                }
                            }
                            rdr.Close();


                            if (list.Count > 0)
                            {


                                var response = new CustomerResultResponse { respcode = 1, message = "Found", benelist = list };
                                kplog.Info(JsonConvert.SerializeObject(response));
                                return response;
                            }
                            else
                            {

                                kplog.Info("No Data Found");
                                return new CustomerResultResponse { respcode = 0, message = "No Beneficiary Found", benelist = null };
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }
        }


        //done loggings RR
        [HttpGet]
        public BeneficiaryResponse getBeneficiaryInfo(String receiverCustID, String securityToken)
        {
            kplog.Info("PARAMS --- > receiverCustID: " + receiverCustID);

            if (securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new BeneficiaryResponse { respcode = 0, message = getRespMessage(7) };
            }

            try
            {
                DateTime? dtcheck;
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select SenderCustID,firstname, lastname, middlename, fullname, street, citystate, country, if(zipcode is null,'',zipcode) as zipcode, date_format(birthdate,'%Y-%m-%d') as birthdate, gender, contactno, Relation, CustIDB,b.ImagePath,b.Province from kpcustomersglobal.BeneficiaryHistory a INNER JOIN kpcustomersglobal.BeneficiaryPayNearMe b ON a.CustIDB = b.ReceiverCustID where CustIDB=@rcvrCustID;";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("rcvrCustID", receiverCustID);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Read();
                                try
                                {
                                    dtcheck = Convert.ToDateTime(rdr["BirthDate"]);
                                }
                                catch (Exception)
                                {

                                    dtcheck = null;
                                }


                                BeneficiaryModel model = new BeneficiaryModel()
                                {
                                    receiverCustID = rdr["CustIDB"].ToString(),
                                    firstName = rdr["firstname"].ToString(),
                                    lastname = rdr["lastname"].ToString(),
                                    midlleName = rdr["middlename"].ToString(),
                                    city = rdr["CityState"].ToString(),
                                    country = rdr["Country"].ToString(),
                                    zipcode = rdr["ZipCode"].ToString(),
                                    dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("yyyy-MM-dd") : "",
                                    gender = rdr["Gender"].ToString(),
                                    relation = rdr["Relation"].ToString(),
                                    phoneNo = rdr["ContactNo"].ToString().Substring(1),
                                    street = rdr["street"].ToString(),
                                    SenderCustID = rdr["SenderCustID"].ToString(),
                                    ImagePath = rdr["ImagePath"].ToString(),
                                    province = rdr["Province"].ToString()

                                };

                                rdr.Close();
                                kplog.Info("FOUND: " + JsonConvert.SerializeObject(model));
                                kplog.Info("Success : Data Found");
                                return new BeneficiaryResponse { respcode = 1, message = "Success", data = model };
                            }
                            else
                            {
                                rdr.Close();
                                kplog.Info("Success : No Data Found");
                                return new BeneficiaryResponse { respcode = 0, message = "No Data found", data = null };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
            }




        }

        private BeneficiaryResponse getBeneficiaryInfo(String receiverCustID)
        {
            kplog.Info("PARAMS --- > receiverCustID: " + receiverCustID);

           

            try
            {
                DateTime? dtcheck;
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select SenderCustID,firstname, lastname, middlename, fullname, street, citystate, country, if(zipcode is null,'',zipcode) as zipcode, date_format(birthdate,'%Y-%m-%d') as birthdate, gender, contactno, Relation, CustIDB,b.ImagePath,b.Province from kpcustomersglobal.BeneficiaryHistory a INNER JOIN kpcustomersglobal.BeneficiaryPayNearMe b ON a.CustIDB = b.ReceiverCustID where CustIDB=@rcvrCustID;";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("rcvrCustID", receiverCustID);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Read();
                                try
                                {
                                    dtcheck = Convert.ToDateTime(rdr["BirthDate"]);
                                }
                                catch (Exception)
                                {

                                    dtcheck = null;
                                }


                                BeneficiaryModel model = new BeneficiaryModel()
                                {
                                    receiverCustID = rdr["CustIDB"].ToString(),
                                    firstName = rdr["firstname"].ToString(),
                                    lastname = rdr["lastname"].ToString(),
                                    midlleName = rdr["middlename"].ToString(),
                                    city = rdr["CityState"].ToString(),
                                    country = rdr["Country"].ToString(),
                                    zipcode = rdr["ZipCode"].ToString(),
                                    dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("yyyy-MM-dd") : "",
                                    gender = rdr["Gender"].ToString(),
                                    relation = rdr["Relation"].ToString(),
                                    phoneNo = rdr["ContactNo"].ToString().Substring(1),
                                    street = rdr["street"].ToString(),
                                    SenderCustID = rdr["SenderCustID"].ToString(),
                                    ImagePath = rdr["ImagePath"].ToString(),
                                    province = rdr["Province"].ToString()

                                };

                                rdr.Close();
                                kplog.Info("FOUND: " + JsonConvert.SerializeObject(model));
                                kplog.Info("Success : Data Found");
                                return new BeneficiaryResponse { respcode = 1, message = "Success", data = model };
                            }
                            else
                            {
                                rdr.Close();
                                kplog.Info("Success : No Data Found");
                                return new BeneficiaryResponse { respcode = 0, message = "No Data found", data = null };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
            }




        }


        //done loggings RR
        [HttpPost]
        public CustomerResultResponse deActivateBeneficiary(String receiverCustID, String securityToken)
        {
            kplog.Info("PARAMS --- > receiverCustID: " + receiverCustID);
            try
            {


                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new CustomerResultResponse { respcode = 0, message = getRespMessage(7) };
                }

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select * from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b ON a.CustIDB = b.ReceiverCustID where a.CustIDB=@rcvrCustID;";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("rcvrCustID", receiverCustID);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Close();
                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.BeneficiaryPayNearMe SET isActivate = 0 where ReceiverCustID = @CustID";
                                cmd.Parameters.AddWithValue("CustID", receiverCustID);
                                int x = cmd.ExecuteNonQuery();

                                if (x > 0)
                                {
                                    kplog.Info("SUCCESS : Successfully deactivated Beneficiary!");
                                    return new CustomerResultResponse { respcode = 1, message = "Beneficiary is successfully deactivated!" };
                                }
                                else
                                {
                                    kplog.Error("Error : Mysql Error");
                                    return new CustomerResultResponse { respcode = 0, message = "Mysql Error" };
                                }

                            }
                            else
                            {
                                rdr.Close();
                                kplog.Info("SUCCESS : Beneficiary does not exist!");
                                return new CustomerResultResponse { respcode = 0, message = "Beneficiary does not exist!" };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }

        }

        //done loggings RR
        [HttpPost]
        public AddKYCResponse addKYCGlobal(CustomerModel req)
        {

            kplog.Info("PARAMS --> " + JsonConvert.SerializeObject(req));

            if (req.securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new AddKYCResponse { respcode = 0, message = getRespMessage(7) };
            }

            String SenderFName = cleanString(req.firstName);
            String SenderLName = cleanString(req.lastName);
            String SenderMName = cleanString(req.middleName);
            String SenderStreet = cleanString(req.Street);
            String SenderCity = req.City;
            String SenderCountry = req.Country;
            String SenderGender = req.Gender;
            String SenderBirthdate = ConvertDateTime(req.BirthDate);
            String SenderBranchID = req.BranchID;
            String MobileNo = req.PhoneNo.Trim();
            String ZipCode = req.ZipCode;
            String activationCode = generateActivationCode();
            String mobileToken = generateMobileToken();
            String Email = req.UserID.Trim();
            String Password = req.Password;
            String Name = string.Empty;
            String State = req.State;
            String Gender = req.Gender;
            String CreatedBy = req.CreatedBy;
            String PhoneNo = req.PhoneNo;
            String IDNo = req.IDNo.Trim();
            String IDType = req.IDType;
            String ExpiryDate = ConvertDateTime(req.ExpiryDate);
            string strBase64 = req.strBase64Image;
            string strBase641 = req.strBase64Image1F;
            string strBase642 = req.strBase64Image1B;
            string strBase643 = req.strBase64Image2F;
            string strBase644 = req.strBase64Image2B;

            String filePath = string.Empty;
            String filePath1 = string.Empty;
            String filePath2 = string.Empty;
            String filePath3 = string.Empty;
            String filePath4 = string.Empty;
            String browsepath = string.Empty;
            String browsepath1 = string.Empty;
            String browsepath2 = string.Empty;
            String browsepath3 = string.Empty;
            String browsepath4 = string.Empty;


            if (string.IsNullOrEmpty(SenderFName) || string.IsNullOrEmpty(SenderLName) || string.IsNullOrEmpty(Email))
            {
                return new AddKYCResponse { respcode = 0, message = "Please input required Fields!" };
            }

            if (SenderMName == null)
            {
                SenderMName = "";
            }


            if (SenderMName == "" || SenderMName == String.Empty)
            {
                Name = SenderFName + " " + SenderLName;
            }
            else
            {
                Name = SenderFName + " " + SenderMName + " " + SenderLName;
            }


            kplog.Info("SenderFName: " + SenderFName + ", SenderLName: " + SenderLName + " SenderMName: " + SenderMName + ", SenderStreet: " + SenderStreet + ", SenderProvinceCity: " + State + "SenderCountry: " + SenderCountry + ", ZipCode: " + ZipCode + ", SenderGender: " + SenderGender + ", SenderBirthdate: " + SenderBirthdate + ", SenderBranchID: " + SenderBranchID);


            try
            {


                if(iDology) 
                {
                    var apiIDologyResp = ExpectID_IQ_Check(req).Result;


                    if (apiIDologyResp == "FAIL")
                    {
                        kplog.Error("apiIDOLOGY FAIL: Name=" + Name);
                        return new AddKYCResponse { respcode = 0, message = "Please check and make sure you provided a valid information, if persist please contact support!" };
                    }
                    else if (apiIDologyResp == "ERROR")
                    {
                        kplog.Error("apiIDOLOGY ERROR: Name=" + Name);
                        return new AddKYCResponse { respcode = 0, message = "Something went wrong, Please try Again!" };
                    }


                }

                dt = getServerDateGlobal();
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new AddKYCResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString(), MLCardNo = null };
            }


            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                try
                {
                    custconn.Open();
                    Int32 sr = 0;
                    String custid = string.Empty;
                    string updatecustomerseries = String.Empty;
                    string senderid = String.Empty;
                    custtrans = custconn.BeginTransaction(IsolationLevel.ReadCommitted);

                    using (custcommand = custconn.CreateCommand())
                    {

                        custcommand.CommandText = "Select CustID FROM kpcustomersglobal.customers where FirstName = @fname and LastName = @lname and MiddleName = @mname and BirthDate = @bdate LIMIT 1";
                        custcommand.Parameters.AddWithValue("fname", SenderFName);
                        custcommand.Parameters.AddWithValue("lname", SenderLName);
                        custcommand.Parameters.AddWithValue("mname", SenderMName);
                        custcommand.Parameters.AddWithValue("bdate", SenderBirthdate);


                        using (MySqlDataReader Reader1 = custcommand.ExecuteReader())
                        {

                            if (Reader1.HasRows)
                            {
                                Reader1.Read();
                                custid = Reader1["CustID"].ToString();
                                Reader1.Close();
                                custcommand.Parameters.Clear();
                                custcommand.CommandText = "Select * from kpcustomersglobal.PayNearMe where CustomerID = @custID OR UserID=@userID";
                                custcommand.Parameters.AddWithValue("custID", custid);
                                custcommand.Parameters.AddWithValue("userID", Email);
                                MySqlDataReader rdrUni = custcommand.ExecuteReader();
                                if (rdrUni.HasRows)
                                {
                                    rdrUni.Close();
                                    kplog.Info("Customer Already Registered");
                                    return new AddKYCResponse { respcode = 0, message = "Customer is already registered!" };

                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(strBase64))
                                    {

                                        String filename = getTimeStamp().ToString() + ".png";
                                        filePath = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase64, filePath);

                                        kplog.Info("UPLOAD: " + filePath);
                                    }
                                    if (!string.IsNullOrEmpty(strBase641))
                                    {
                                        String filename = getTimeStamp().ToString() + "1F"+".png";
                                        filePath1 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath1 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase641, filePath1);
                                        kplog.Info("UPLOAD: " + filePath1);
                                    }
                                    if (!string.IsNullOrEmpty(strBase642))
                                    {
                                        String filename = getTimeStamp().ToString() + "1B" + ".png";
                                        filePath2 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath2 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase642, filePath2);
                                        kplog.Info("UPLOAD: " + filePath2);
                                    }
                                    if (!string.IsNullOrEmpty(strBase643))
                                    {
                                        String filename = getTimeStamp().ToString() + "2F" + ".png";
                                        filePath3 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath3 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase643, filePath3);
                                        kplog.Info("UPLOAD: " + filePath3);
                                    }
                                    if (!string.IsNullOrEmpty(strBase644))
                                    {
                                        String filename = getTimeStamp().ToString() + "2B" + ".png";
                                        filePath4 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath4 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase644, filePath4);
                                        kplog.Info("UPLOAD: " + filePath4);
                                    }

                                    rdrUni.Close();

                                    custcommand.Parameters.Clear();
                                    custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                            + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath,validID1Front,validID1Back,validID2Front,validID2Back,mobileToken) "
                                                            + "VALUES "
                                                            + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                            + "@PrivacyPolicyAgreement, @activationCode,@ImagePath,@ImagePath1,@ImagePath2,@ImagePath3,@ImagePath4,@mobileToken)";
                                    custcommand.Parameters.AddWithValue("custID", custid);
                                    custcommand.Parameters.AddWithValue("Password", Password);
                                    custcommand.Parameters.AddWithValue("UserID", Email);
                                    custcommand.Parameters.AddWithValue("FullName", Name);
                                    custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                                    custcommand.Parameters.AddWithValue("activationCode", activationCode);
                                    custcommand.Parameters.AddWithValue("mobileToken", mobileToken);
                                    custcommand.Parameters.AddWithValue("ImagePath", browsepath);
                                    custcommand.Parameters.AddWithValue("ImagePath1", browsepath1);
                                    custcommand.Parameters.AddWithValue("ImagePath2", browsepath2);
                                    custcommand.Parameters.AddWithValue("ImagePath3", browsepath3);
                                    custcommand.Parameters.AddWithValue("ImagePath4", browsepath4);
                                    custcommand.ExecuteNonQuery();

                                    kplog.Info("INSERT kpcustomersglobal.PayNearMe");
                                }

                                Int32 timestamp = getTimeStamp();
                                string yearofbirth = Convert.ToDateTime(SenderBirthdate).ToString("yyyy");

                                string queryAPI = "city=" + SenderCity + "&country=" + SenderCountry + "&first_name=" + SenderFName + "&last_name=" + SenderLName + "&middle_name=" + SenderMName + "&postal_code=" + ZipCode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + custid + "&street=" + SenderStreet + "&timestamp=" + timestamp.ToString() +
                                                "&user_type=sender&version=2.0&year_of_birth=" + yearofbirth;



                                string signature = generateSignature(queryAPI);

                                queryAPI = queryAPI + "&signature=" + signature;

                                Uri uri = new Uri(server + "/json-api/create_user?" + queryAPI);

                                string res = SendRequest(uri);
                                kplog.Info("Response: PayNearMe API create_user: " + res);
                                dynamic data = JObject.Parse(res);

                                if (data.status == "ok")
                                {
                                    custtrans.Commit();
                                    custconn.Close();

                                    sendEmailActivation(Email, SenderFName, activationCode, mobileToken);

                                    kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " MLCardNo " + senderid);
                                    return new AddKYCResponse { respcode = 1, message = getRespMessage(1), MLCardNo = senderid };
                                }
                                else
                                {
                                    custtrans.Rollback();
                                    custconn.Close();

                                    string error = "";
                                    for (int xx = 0; xx < data.errors.Count; xx++)
                                    {
                                        error = error + " " + data.errors[xx].description;
                                    }
                                    kplog.Info(error);
                                    return new AddKYCResponse { respcode = 0, message = error };
                                }


                            }
                        }
                    }

                    using (custcommand = custconn.CreateCommand())
                    {

                        custcommand.Parameters.Clear();
                        custcommand.CommandText = "Select * from kpcustomersglobal.PayNearMe where  UserID=@userID";
                        custcommand.Parameters.AddWithValue("userID", Email);
                        MySqlDataReader rdrUni = custcommand.ExecuteReader();
                        if (rdrUni.HasRows)
                        {
                            rdrUni.Close();
                            kplog.Info("Customer Already Registered");
                            return new AddKYCResponse { respcode = 0, message = "Customer already registered." };

                        }

                        rdrUni.Close();
                        //string senderid = generateCustIDGlobal(custcommand);
                        String query = "select series from kpformsglobal.customerseries";
                        custcommand.Parameters.Clear();
                        custcommand.CommandText = query;
                        MySqlDataReader Reader = custcommand.ExecuteReader();
                        if (Reader.HasRows)
                        {
                            Reader.Read();
                            if (!(Reader["series"] == DBNull.Value))
                            {
                                sr = Convert.ToInt32(Reader["series"].ToString());
                            }
                        }
                        Reader.Close();

                        Int32 sr1 = sr + 1;
                        custcommand.Transaction = custtrans;

                        if (sr == 0)
                        {
                            updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                            kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.customerseries: series: " + sr1 + " year: " + dt.ToString("yyyy"));
                        }
                        else
                        {
                            updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                            kplog.Info("SUCCESS:: UPDATE kpformsglobal.customerseries: SET series: " + sr1 + " year: " + dt.ToString("yyyy"));
                        }


                        custcommand.CommandText = updatecustomerseries;
                        custcommand.ExecuteNonQuery();

                        senderid = generateCustIDGlobal(custcommand);

                        String insertCustomer = "INSERT INTO kpcustomersglobal.customers (CustID, FirstName, LastName, MiddleName, Street, ProvinceCity, Country, ZipCode, Gender, Birthdate, DTCreated, CreatedBy, PhoneNo, Mobile, Email, CreatedByBranch,IDNo,IDType,ExpiryDate) VALUES (@SCustID, @SFirstName, @SLastName, @SMiddleName, @SStreet, @SProvinceCity, @SCountry, @SZipcode, @SGender, @SBirthdate, @DTCreated,@CreatedBy, @PhoneNo, @MobileNo, @Email,@CreatedByBranch,@IDNo,@IDType,@ExpiryDate);";
                        custcommand.CommandText = insertCustomer;

                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("SCustID", senderid);
                        custcommand.Parameters.AddWithValue("SFirstName", SenderFName);
                        custcommand.Parameters.AddWithValue("SLastName", SenderLName);
                        custcommand.Parameters.AddWithValue("SMiddleName", SenderMName);
                        custcommand.Parameters.AddWithValue("SStreet", SenderStreet);
                        custcommand.Parameters.AddWithValue("SProvinceCity", State);
                        custcommand.Parameters.AddWithValue("SZipcode", ZipCode);
                        custcommand.Parameters.AddWithValue("SCountry", SenderCountry);
                        custcommand.Parameters.AddWithValue("SGender", SenderGender);
                        custcommand.Parameters.AddWithValue("SBirthdate", SenderBirthdate);
                        custcommand.Parameters.AddWithValue("DTCreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        custcommand.Parameters.AddWithValue("PhoneNo", PhoneNo);
                        custcommand.Parameters.AddWithValue("MobileNo", MobileNo);
                        custcommand.Parameters.AddWithValue("Email", Email);
                        custcommand.Parameters.AddWithValue("CreatedBy", "PayNearMe");
                        custcommand.Parameters.AddWithValue("CreatedByBranch", "PayNearMe");

                        custcommand.Parameters.AddWithValue("IDType", IDType);
                        custcommand.Parameters.AddWithValue("IDNo", IDNo);
                        custcommand.Parameters.AddWithValue("ExpiryDate", ExpiryDate);

                        custcommand.ExecuteNonQuery();

                        kplog.Info("INSERT kpcustomersglobal.customers");

                        String insertCustomerDetails = "INSERT INTO kpcustomersglobal.customersdetails(CustID,HomeCity) values(@dcustid,@dhomecity)";
                        custcommand.CommandText = insertCustomerDetails;
                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("dcustid", senderid);
                        custcommand.Parameters.AddWithValue("dhomecity", SenderCity);
                        custcommand.ExecuteNonQuery();

                        kplog.Info("INSERT kpcustomersglobal.customersdetails");
                        //PAYNEARME
                        if (!string.IsNullOrEmpty(strBase64))
                        {

                            String filename = getTimeStamp().ToString() + ".png";
                            filePath = ftp + "/PayNearMe/Images/" + filename;
                            browsepath = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase64, filePath);

                            kplog.Info("UPLOAD: filepath : " + browsepath);
                        }
                        if (!string.IsNullOrEmpty(strBase641))
                        {
                            String filename = getTimeStamp().ToString() + "1F" + ".png";
                            filePath1 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath1 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase641, filePath1);
                            kplog.Info("UPLOAD: filepath : " + browsepath1);
                        }
                        if (!string.IsNullOrEmpty(strBase642))
                        {
                            String filename = getTimeStamp().ToString() + "1B" + ".png";
                            filePath2 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath2 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase642, filePath2);
                            kplog.Info("UPLOAD: filepath : " + browsepath2);
                        }
                        if (!string.IsNullOrEmpty(strBase643))
                        {
                            String filename = getTimeStamp().ToString() + "2F" + ".png";
                            filePath3 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath3 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase643, filePath3);
                            kplog.Info("UPLOAD: filepath : " + browsepath3);
                        }
                        if (!string.IsNullOrEmpty(strBase644))
                        {
                            String filename = getTimeStamp().ToString() + "2B" + ".png";
                            filePath4 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath4 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase644, filePath4);
                            kplog.Info("UPLOAD: filepath : " + browsepath4);
                        }

                        custcommand.Parameters.Clear();
                        custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath,validID1Front,validID1Back,validID2Front,validID2Back,mobileToken) "
                                                + "VALUES "
                                                + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                + "@PrivacyPolicyAgreement, @activationCode,@ImagePath,@ImagePath1,@ImagePath2,@ImagePath3,@ImagePath4,@mobileToken)";
                        custcommand.Parameters.AddWithValue("custID", senderid);
                        custcommand.Parameters.AddWithValue("Password", Password);
                        custcommand.Parameters.AddWithValue("UserID", Email);
                        custcommand.Parameters.AddWithValue("FullName", Name);
                        custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                        custcommand.Parameters.AddWithValue("activationCode", activationCode);
                        custcommand.Parameters.AddWithValue("ImagePath", browsepath);
                        custcommand.Parameters.AddWithValue("ImagePath1", browsepath1);
                        custcommand.Parameters.AddWithValue("ImagePath2", browsepath2);
                        custcommand.Parameters.AddWithValue("ImagePath3", browsepath3);
                        custcommand.Parameters.AddWithValue("ImagePath4", browsepath4);
                        custcommand.Parameters.AddWithValue("mobileToken", mobileToken);
                        custcommand.ExecuteNonQuery();
                        //

                        kplog.Info("INSERT kpcustomersglobal.PayNearMe");

                        String insertCustomerLogs = "INSERT INTO kpadminlogsglobal.customerlogs(ScustID,Details,Syscreated,Syscreator,Type) values(@scustid,@details,now(),@creator,@type)";
                        custcommand.CommandText = insertCustomerLogs;
                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("scustid", senderid);
                        custcommand.Parameters.AddWithValue("details", "{Name:" + " " + SenderFName + " " + SenderMName + " " + SenderLName + ", " + "Street:" + " " + SenderStreet + ", " + "ProvinceCity:" + " " + State + ", " + "ZipCode:" + " " + ZipCode + ", " + "Country:" + " " + SenderCountry + ", " + "Gender:" + " " + SenderGender + ", " + "BirthDate:" + " " + SenderBirthdate + ", " + "PhoneNo:" + " " + PhoneNo + ", " + "MobileNo:" + " " + MobileNo + ", " + "Email:" + " " + Email + ", " + "CreatedByBranch" + " PayNearMe}");
                        custcommand.Parameters.AddWithValue("creator", CreatedBy);
                        custcommand.Parameters.AddWithValue("type", "N");
                        custcommand.ExecuteNonQuery();



                        kplog.Info("SUCCESS:: INSERT INTO kpcustomersglobal.customers: SCustID: " + senderid + " " +
                        "SFirstName: " + SenderFName + " " +
                        "SLastName: " + SenderLName + " " +
                        "SMiddleName: " + SenderMName + " " +
                        "SStreet: " + SenderStreet + " " +
                        "SProvinceCity: " + State + " " +
                        "SZipcode: " + ZipCode + " " +
                        "SCountry: " + SenderCountry + " " +
                        "SGender: " + SenderGender + " " +
                        "SBirthdate: " + SenderBirthdate + " " +
                        "DTCreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " " +
                        "PhoneNo: " + PhoneNo + " " +
                        "MobileNo: " + MobileNo + " " +
                        "Email: " + Email + " " +
                        "CreatedBy: " + CreatedBy);


                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.customerlogs: scustid: " + senderid + " " +
                        "details: " + "{Name:" + " " + SenderFName + " " + SenderMName + " " + SenderLName + " " + "Street:" + " " + SenderStreet + " " + "ProvinceCity:" + " " + State + " " + "ZipCode:" + " " + ZipCode + " " + "Country:" + " " + SenderCountry + " " + "Gender:" + " " + SenderGender + "  + " + "BirthDate:" + " " + SenderBirthdate + " " + "PhoneNo:" + " " + PhoneNo + " " + "MobileNo:" + " " + MobileNo + " " + "Email:" + " " + Email + " " + "CreatedByBranch" + " PayNearMe}" + " " +
                        "creator: " + CreatedBy + " " +
                        "type: N");

                        Int32 timestamp = getTimeStamp();
                        string yearofbirth = Convert.ToDateTime(SenderBirthdate).ToString("yyyy");

                        string queryAPI = "city=" + SenderCity + "&country=" + SenderCountry + "&first_name=" + SenderFName + "&last_name=" + SenderLName + "&middle_name=" + SenderMName + "&postal_code=" + ZipCode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + senderid + "&street=" + SenderStreet + "&timestamp=" + timestamp.ToString() +
                                        "&user_type=sender&version=2.0&year_of_birth=" + yearofbirth;



                        string signature = generateSignature(queryAPI);

                        queryAPI = queryAPI + "&signature=" + signature;

                        Uri uri = new Uri(server + "/json-api/create_user?" + queryAPI);

                        string res = SendRequest(uri);
                        kplog.Info("RESPONSE: PayNearMe API- create_user: " + res);
                        dynamic data = JObject.Parse(res);

                        if (data.status == "ok")
                        {
                            custtrans.Commit();
                            custconn.Close();
                            sendEmailActivation(Email, SenderFName, activationCode, mobileToken);
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " MLCardNo " + senderid);
                            return new AddKYCResponse { respcode = 1, message = getRespMessage(1), MLCardNo = senderid };
                        }
                        else
                        {
                            custtrans.Rollback();
                            custconn.Close();

                            string error = "";
                            for (int xx = 0; xx < data.errors.Count; xx++)
                            {
                                error = error + " " + data.errors[xx].description;
                            }
                            kplog.Info(error);
                            return new AddKYCResponse { respcode = 0, message = error };
                        }




                    }
                }
                catch (Exception mex)
                {
                    custtrans.Rollback();
                    custconn.Close();
                    int respcode = 0;
                    if (mex.Message.StartsWith("Duplicate"))
                    {
                        Int32 sr = 0;
                        String updatecustomerseries = string.Empty;
                        respcode = 6;
                        kplog.Fatal("FAILED:: message: " + getRespMessage(respcode) + " ErrorDetail: " + mex.ToString());
                        using (MySqlConnection conTrap = new MySqlConnection(connection))
                        {
                            conTrap.Open();
                            using (MySqlCommand custcommand2 = conTrap.CreateCommand())
                            {
                                //string senderid = generateCustIDGlobal(custcommand);
                                String query = "select series from kpformsglobal.customerseries";
                                custcommand2.CommandText = query;
                                MySqlDataReader Reader = custcommand2.ExecuteReader();
                                if (Reader.HasRows)
                                {
                                    Reader.Read();
                                    if (!(Reader["series"] == DBNull.Value))
                                    {
                                        sr = Convert.ToInt32(Reader["series"]);//.ToString());
                                    }
                                }
                                Reader.Close();

                                Int32 sr1 = sr + 1;


                                if (sr == 0)
                                {
                                    updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                                    kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.customerseries: series: " + sr1 + " year: " + dt.ToString("yyyy"));
                                }
                                else
                                {
                                    updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                                    kplog.Info("SUCCESS:: UPDATE kpformsglobal.customerseries: SET series: " + sr1 + " year: " + dt.ToString("yyyy"));
                                }
                                custcommand2.CommandText = updatecustomerseries;
                                custcommand2.ExecuteNonQuery();
                            }
                        }

                    }
                    custconn.Close();
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(respcode) + " ErrorDetail: " + mex.ToString());
                    return new AddKYCResponse { respcode = respcode, message = getRespMessage(respcode), ErrorDetail = mex.ToString() };
                }
            }
        }


        //done loggings RR
        [HttpGet]
        public AddKYCResponse getState(String zipCode, String securityToken)
        {
            try
            {
                kplog.Info("PARAMS: zipCode: " + zipCode + ", securityToken :" + securityToken);

                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new AddKYCResponse { respcode = 0, message = getRespMessage(7) };
                }

                zipCodeResp resp = new zipCodeResp();

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select State , City from kpformsglobal.zipcodesG where ZipCode1 = @zipcode and State = 'California'";
                        cmd.Parameters.AddWithValue("zipcode", zipCode);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {

                            rdr.Read();
                            resp.State = rdr["State"].ToString();
                            resp.City = rdr["City"].ToString();
                            kplog.Info("SUCCESS : '" + resp + "'");
                            return new AddKYCResponse { respcode = 1, message = "Success", zCodeResp = resp };
                        }
                        else
                        {

                            rdr.Close();
                            kplog.Info("SUCCESS : INVALID ZIPCODE");
                            return new AddKYCResponse { respcode = 0, message = "Invalid Zipcode" };
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                kplog.Error("ERROR : '" + ex.ToString() + "'");
                return new AddKYCResponse { respcode = 0, message = "Error occured", ErrorDetail = ex.ToString() };
            }
        }


        [HttpPost]
        public LoginResponse LoginPayNearMeMobile(LoginViewModel model)
        {

            if (model.securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new LoginResponse { respcode = 0, message = getRespMessage(7) };
            }


            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select pinCodeStatus,retry,CustomerID from kpcustomersglobal.PayNearMe where UserID = @email";
                        cmd.Parameters.AddWithValue("email", model.EmailAddress);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {
                            rdr.Read();
                            Int32 pincodeStat = Convert.ToInt32(rdr["pinCodeStatus"]);
                            Int32 retry = Convert.ToInt32(rdr["retry"]);
                            String CustomerID = rdr["CustomerID"].ToString();
                            if (pincodeStat != 1)
                            {
                                return new LoginResponse { respcode = 0, message = "Account is deactivated please contact support to activate." };
                            }

                            rdr.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @email and Password = @password";
                            cmd.Parameters.AddWithValue("email", model.EmailAddress);
                            cmd.Parameters.AddWithValue("password", model.Password);
                            MySqlDataReader rdrPass = cmd.ExecuteReader();
                            if (rdrPass.HasRows)
                            {
                                rdrPass.Read();

                                String filepath = rdrPass["ImagePath"].ToString();
                                Int32 isActive = Convert.ToInt32(rdrPass["isEmailActivated"]);

                                if (isActive == 0)
                                {
                                    rdrPass.Close();

                                    con.Close();
                                    kplog.Info("Account not yet activated");
                                    return new LoginResponse { respcode = 2, message = "Account not yet activated" };


                                }

                                String fullname = rdrPass["FullName"].ToString();
                                String signupDate = rdrPass["SignupDate"].ToString();
                                String lastLogin = rdrPass["LastLogin"].ToString();
                                String customerID = rdrPass["CustomerID"].ToString();
                                rdrPass.Close();

                                cmd.Parameters.Clear();
                                cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe Set LastLogin = NOW() , retry = '0' where UserID = @email and Password = @password;";
                                cmd.Parameters.AddWithValue("email", model.EmailAddress);
                                cmd.Parameters.AddWithValue("password", model.Password);
                                cmd.ExecuteNonQuery();

                                if (string.IsNullOrEmpty(signupDate))
                                {
                                    signupDate = getServerDateGlobal().ToString();
                                }
                                if (string.IsNullOrEmpty(lastLogin))
                                {
                                    lastLogin = getServerDateGlobal().ToString();
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Select a.FirstName,a.LastName,a.MiddleName,a.Street,a.ProvinceCity,a.Country,a.ZipCode,a.Gender,a.Birthdate,a.PhoneNo,a.Email,b.HomeCity from kpcustomersglobal.customers a INNER JOIN kpcustomersglobal.customersdetails b ON a.CustID=b.CustID  where a.custID = @CustID";
                                cmd.Parameters.AddWithValue("CustID", customerID);
                                MySqlDataReader rdrModel = cmd.ExecuteReader();

                                rdrModel.Read();



                                CustomerModel customer = new CustomerModel
                                {
                                    CustomerID = customerID,
                                    ImagePath = filepath,
                                    firstName = rdrModel["FirstName"].ToString(),
                                    lastName = rdrModel["LastName"].ToString(),
                                    middleName = rdrModel["MiddleName"].ToString(),
                                    Street = rdrModel["Street"].ToString(),
                                    State = rdrModel["ProvinceCity"].ToString(),
                                    Country = rdrModel["Country"].ToString(),
                                    ZipCode = rdrModel["ZipCode"].ToString(),
                                    Gender = rdrModel["Gender"].ToString(),
                                    BirthDate = rdrModel["Birthdate"].ToString(),
                                    PhoneNo = rdrModel["PhoneNo"].ToString(),
                                    UserID = rdrModel["Email"].ToString(),
                                    City = rdrModel["HomeCity"].ToString()

                                };


                                con.Close();
                                kplog.Info("Success : fullName = '" + fullname + "', signupDate = '" + signupDate + "', lastLogin = '" + lastLogin + "', customer = '" + customer + "'");
                                return new LoginResponse { respcode = 1, message = "Success", fullName = fullname, signupDate = signupDate, lastLogin = lastLogin, customer = customer };

                            }
                            else
                            {
                                rdrPass.Close();


                                if (retry >= 3)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set pinCodeStatus = '0' where CustomerID = @userID";
                                    cmd.Parameters.AddWithValue("userID", CustomerID);
                                    cmd.ExecuteNonQuery();
                                    kplog.Info("Exceeded maximum attempt, Please contact support to activate your account!");
                                    con.Close();
                                    return new LoginResponse { respcode = 0, message = "Exceeded maximum attempt, Please contact support to activate your account!" };

                                }
                                else
                                {
                                    retry++;
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set retry = '" + retry + "' where CustomerID = @userID";
                                    cmd.Parameters.AddWithValue("userID", CustomerID);
                                    cmd.ExecuteNonQuery();

                                    Int32 retrylimit = 3 - retry;

                                    if (retrylimit == 0)
                                    {
                                        con.Close();
                                        kplog.Info("Invalid Password, you have reached the maximum attempt. Please contact support to activate account");
                                        return new LoginResponse { respcode = 2, message = "Invalid Password, you have reached the maximum attempt. Please contact support to activate account" };
                                    }
                                    else
                                    {
                                        con.Close();
                                        kplog.Info("Invalid Password, you have " + retrylimit + " attempts left.");
                                        return new LoginResponse { respcode = 0, message = "Invalid Password, you have " + retrylimit + " attempts left." };
                                    }
                                }


                            }

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            kplog.Info("Email not registered");
                            return new LoginResponse { respcode = 0, message = "Email not yet registered" };
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                kplog.Error(ex.ToString());
                return new LoginResponse { respcode = 0, message = ex.ToString() };
            }
        }


        [HttpGet]
        public TransactionResponseMobile getAllTransactionByCategoryM(string CustomerID, String month, String year, String status, String securityToken)
        {
            try
            {
                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new TransactionResponseMobile { respcode = 0, message = getRespMessage(7) };
                }

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    string customerid = string.Empty;
                    con.Open();
                    MySqlCommand cmd = con.CreateCommand();
                    List<TransactionDetailsM> listData = new List<TransactionDetailsM>();
                    int Count = 0;


                    DateTime dt = getServerDateGlobal();

                    String thisMonth = dt.ToString("MM");
                    String thisYear = dt.ToString("yyyy");
                    String lastMonth = dt.AddMonths(-1).ToString("MM");
                    String table = string.Empty;

                    cmd.Connection = con;
                    String sql = string.Empty;

                    if (string.IsNullOrEmpty(month))
                    {
                        table = thisMonth;
                    }
                    else
                    {
                        table = month;
                    }
                    if (string.IsNullOrEmpty(year))
                    {
                        year = thisYear;
                    }

                    if (year == "now")
                    {

                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND date(orderCreated) = curdate()  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                    }
                    else
                    {
                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND YEAR(orderCreated) = '" + year + "'  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }

                    }







                    cmd.CommandText = sql;
                    MySqlDataReader rdr1 = cmd.ExecuteReader();

                    while (rdr1.Read())
                    {
                        listData.Add(new TransactionDetailsM
                        {
                            kptn = rdr1["siteOrderIdentifier"].ToString(),
                            TransDate = rdr1["orderCreated"].ToString(),
                            principal = rdr1["orderPrincipal"].ToString(),
                            charge = (Convert.ToDouble(rdr1["orderChargeML"]) + Convert.ToDouble(rdr1["orderChargePNM"])).ToString(),
                            totalamount = rdr1["orderAmountTotal"].ToString(),
                            trackingURL = rdr1["orderTrackingUrl"].ToString(),
                            poamount = rdr1["orderPOAmountPHP"].ToString(),
                            Status = rdr1["status"].ToString(),
                            RFullName = rdr1["beneficiaryname"].ToString(),
                            exchangeRate = rdr1["orderExchangeRate"].ToString()

                        });
                        Count = listData.Count;
                    }
                    rdr1.Close();



                    kplog.Info("Success : Data Found, respcode = 1, listdata = '" + listData + "'");
                    return new TransactionResponseMobile { respcode = 1, tl = listData, count = Count };
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new TransactionResponseMobile { respcode = 0, message = ex.ToString() };
            }
        }

        [HttpGet]
        public TransactionResponseMobile sendMailTransReport(string CustomerID, String month, String year, String status, String securityToken)
        {
            try
            {
                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new TransactionResponseMobile { respcode = 0, message = getRespMessage(7) };
                }

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    string customerid = string.Empty;
                    con.Open();
                    MySqlCommand cmd = con.CreateCommand();
                    List<TransactionDetailsM> listData = new List<TransactionDetailsM>();
                    int Count = 0;


                    DateTime dt = getServerDateGlobal();

                    String thisMonth = dt.ToString("MM");
                    String thisYear = dt.ToString("yyyy");
                    String lastMonth = dt.AddMonths(-1).ToString("MM");
                    String table = string.Empty;

                    cmd.Connection = con;
                    String sql = string.Empty;

                    if (string.IsNullOrEmpty(month))
                    {
                        table = thisMonth;
                    }
                    else
                    {
                        table = month;
                    }
                    if (string.IsNullOrEmpty(year))
                    {
                        year = thisYear;
                    }

                    if (year == "now")
                    {

                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND date(orderCreated) = curdate()  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                    }
                    else
                    {
                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND YEAR(orderCreated) = '" + year + "'  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }

                    }



                    cmd.CommandText = sql;
                    MySqlDataReader rdr1 = cmd.ExecuteReader();

                    while (rdr1.Read())
                    {
                        listData.Add(new TransactionDetailsM
                        {
                            kptn = rdr1["siteOrderIdentifier"].ToString(),
                            TransDate = rdr1["orderCreated"].ToString(),
                            principal = rdr1["orderPrincipal"].ToString(),
                            charge = (Convert.ToDouble(rdr1["orderChargeML"]) + Convert.ToDouble(rdr1["orderChargePNM"])).ToString(),
                            totalamount = rdr1["orderAmountTotal"].ToString(),
                            trackingURL = rdr1["orderTrackingUrl"].ToString(),
                            poamount = rdr1["orderPOAmountPHP"].ToString(),
                            Status = rdr1["status"].ToString(),
                            RFullName = rdr1["beneficiaryname"].ToString(),
                            exchangeRate = rdr1["orderExchangeRate"].ToString()

                        });
                        Count = listData.Count;
                    }
                    rdr1.Close();



                    try
                    {
                        generateReportMobile(listData, CustomerID);
                        kplog.Info("Success : Data Found, respcode = 1, listdata = '" + listData + "'");
                        return new TransactionResponseMobile { respcode = 1, message = "Successfully Sent Transaction Report to Email." };
                    }
                    catch (Exception err)
                    {

                        kplog.Fatal(err.ToString());
                        return new TransactionResponseMobile { respcode = 0, message = err.ToString() };
                    }



                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new TransactionResponseMobile { respcode = 0, message = ex.ToString() };
            }
        }


        //Done Loggings RR
        [HttpPost]
        public ProfileResponse editProfile(CustomerModel model)
        {
            kplog.Info("START--- > PARAMS: " + JsonConvert.SerializeObject(model));
            try
            {
                if (model.securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new ProfileResponse { respcode = 0, message = getRespMessage(7) };
                }
                String custid = String.Empty;
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @email";
                        cmd.Parameters.AddWithValue("email", model.UserID);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {
                            rdr.Read();
                            custid = rdr["CustomerID"].ToString();
                            rdr.Close();

                            String bdate = ConvertDateTime(model.BirthDate);
                            String expiryDate = ConvertDateTime(model.ExpiryDate);

                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.customers set  Street = @Street, ProvinceCity = @ProvinceCity, Country = @Country, ZipCode= @ZipCode, Birthdate = @BirthDate, Gender = @Gender,Mobile =@Mobile, IDType=@IDType,IDNo=@IDNo,ExpiryDate=@ExpiryDate where CustID = @CustID;";
                            cmd.Parameters.AddWithValue("Street", model.Street);
                            cmd.Parameters.AddWithValue("ProvinceCity", model.State);
                            cmd.Parameters.AddWithValue("Country", model.Country);
                            cmd.Parameters.AddWithValue("ZipCode", model.ZipCode);
                            cmd.Parameters.AddWithValue("BirthDate", bdate);
                            cmd.Parameters.AddWithValue("Gender", model.Gender);
                            cmd.Parameters.AddWithValue("Mobile", model.PhoneNo);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.Parameters.AddWithValue("IDNo", model.IDNo);
                            cmd.Parameters.AddWithValue("IDType", model.IDType);
                            cmd.Parameters.AddWithValue("ExpiryDate", expiryDate);
                            cmd.ExecuteNonQuery();

                            kplog.Info("Success Update kpcustomersglobal.customers");

                            cmd.Parameters.Clear();
                            cmd.CommandText = "Update kpcustomersglobal.customersdetails set HomeCity = @City where custID = @CustID;";
                            cmd.Parameters.AddWithValue("City", model.City);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.ExecuteNonQuery();

                            kplog.Info("Success Update kpcustomersglobal.customersdetails");


                            if (!string.IsNullOrEmpty(model.strBase64Image))
                            {
                                String filename = getTimeStamp().ToString() + ".png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;
                                uploadFileImage(model.strBase64Image, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set ImagePath = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - ImagePath");

                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image1F))
                            {
                                String filename = getTimeStamp().ToString() + "1F.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;
                                uploadFileImage(model.strBase64Image1F, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID1Front = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID1Front");

                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image1B))
                            {
                                String filename = getTimeStamp().ToString() + "1B.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;

                                uploadFileImage(model.strBase64Image1B, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID1Back = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();


                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID1Back");
                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image2F))
                            {

                                String filename = getTimeStamp().ToString() + "2F.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;


                                uploadFileImage(model.strBase64Image2F, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID2Front = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID2Front");
                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image2B))
                            {
                                String filename = getTimeStamp().ToString() + "2B.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;

                                uploadFileImage(model.strBase64Image2B, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID2Back = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID2Back");

                            }


                            con.Close();
                            kplog.Info("SUCCESSS : respcode 1, message: Sucessfully Updated -- END");
                            return new ProfileResponse { respcode = 1, message = "Profile is successfully updated!" };

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            kplog.Info("SUCCESSS : No Profile Found! -- END");
                            return new ProfileResponse { respcode = 0, message = "No Profile Found! " };
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }


        }
        //Done Loggings RR
        [HttpPost]
        public ProfileResponse changePassword(ChangePasswordModel model)
        {
            try
            {
                kplog.Info("START--- > PARAMS: " + JsonConvert.SerializeObject(model));
                if (model.securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new ProfileResponse { respcode = 0, message = getRespMessage(7) };
                }
                using (MySqlConnection con = new MySqlConnection(connection))
                {

                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @email and Password = @oldpassword";
                        cmd.Parameters.AddWithValue("email", model.UserID);
                        cmd.Parameters.AddWithValue("oldpassword", model.currentPassword);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {

                            if (model.newPassword != model.confirmPassword)
                            {
                                rdr.Close();
                                con.Close();
                                kplog.Info("SUCCESSS : Password did not match!");
                                return new ProfileResponse { respcode = 0, message = "Password did not match!" };
                            }

                            rdr.Read();
                            String custid = rdr["CustomerID"].ToString();
                            rdr.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe Set Password = @Password where CustomerID = @custID";
                            cmd.Parameters.AddWithValue("custID", custid);
                            cmd.Parameters.AddWithValue("Password", model.newPassword);
                            cmd.ExecuteNonQuery();


                            con.Close();
                            kplog.Info("SUCCESSS : Respcode = 1, message = Succesfully Change Password!");
                            return new ProfileResponse { respcode = 1, message = "Password is successfully updated!" };

                        }
                        else
                        {
                            kplog.Info("SUCCESSS : Respcode = 0, message = Incorrect Password!");
                            return new ProfileResponse { respcode = 0, message = "Incorrect Password!" };
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }
        }


        //done loggings
        [HttpGet]
        public ProfileResponse getProfile(String CustomerID,String securityToken)
        {

            try
            {
                kplog.Info("PARAMS: CustomerID: "+CustomerID+", securityToken : "+securityToken);
                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new ProfileResponse { respcode = 0, message = getRespMessage(7) };
                }
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();

                    using (MySqlCommand cmd = con.CreateCommand())
                    {

                        String expirydate;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select FirstName,LastName,MiddleName,Street,ProvinceCity as State, Country,ZipCode,BirthDate,c.HomeCity as City,Gender, Mobile,b.UserID as UserID, b.ImagePath as filepath,b.validID1Front,b.validID1Back,b.validID2Front,b.validID2Back,b.CustomerID,a.IDNo,a.IDType,a.ExpiryDate from kpcustomersglobal.customers a inner join kpcustomersglobal.PayNearMe b on a.CustID = b.CustomerID inner join kpcustomersglobal.customersdetails c on a.CustID = c.CustID where a.custID = @custID";
                        cmd.Parameters.AddWithValue("custID", CustomerID);
                        MySqlDataReader rdrProf = cmd.ExecuteReader();
                        if (rdrProf.HasRows)
                        {


                            rdrProf.Read();
                            String filepath = rdrProf["filepath"].ToString();
                            String filepath1 = rdrProf["validID1Front"].ToString();
                            String filepath2 = rdrProf["validID1Back"].ToString();
                            String filepath3 = rdrProf["validID2Front"].ToString();
                            String filepath4 = rdrProf["validID2Back"].ToString();
                            String bdate = rdrProf["BirthDate"].ToString();
                            if (bdate.StartsWith("00") || bdate == String.Empty)
                            {
                                bdate = "";
                            }
                            else
                            {
                                bdate = Convert.ToDateTime(rdrProf["BirthDate"]).ToString("MM/dd/yyyy");
                            }
                            if (string.IsNullOrEmpty(rdrProf["ExpiryDate"].ToString()) || rdrProf["ExpiryDate"].ToString().StartsWith("00"))
                            {
                                expirydate = "";
                            }
                            else
                            {
                                expirydate = Convert.ToDateTime(rdrProf["ExpiryDate"]).ToString("MM/dd/yyyy");
                            }

                            var senderx = new CustomerModel
                                {
                                    firstName = rdrProf["Firstname"].ToString(),
                                    middleName = rdrProf["MiddleName"].ToString(),
                                    lastName = rdrProf["LastName"].ToString(),
                                    Street = rdrProf["Street"].ToString(),
                                    State = rdrProf["State"].ToString(),
                                    Country = rdrProf["Country"].ToString(),
                                    ZipCode = rdrProf["ZipCode"].ToString(),
                                    BirthDate = bdate,
                                    City = rdrProf["City"].ToString(),
                                    Gender = rdrProf["Gender"].ToString(),
                                    PhoneNo = rdrProf["Mobile"].ToString(),
                                    UserID = rdrProf["UserID"].ToString(),
                                    ImagePath = filepath,
                                    ImagePath1 = filepath1,
                                    ImagePath2 = filepath2,
                                    ImagePath3 = filepath3,
                                    ImagePath4 = filepath4,
                                    CustomerID = rdrProf["CustomerID"].ToString(),
                                    IDNo = rdrProf["IDNo"].ToString(),
                                    IDType = rdrProf["IDType"].ToString(),
                                    ExpiryDate = expirydate
                                };

                            kplog.Info("SUCCESS :: respcode : 1, Message: Success!, Object: "+ JsonConvert.SerializeObject(senderx));
                            return new ProfileResponse
                            {
                                respcode = 1,
                                message = "Success",
                                sender = senderx



                            };
                        }
                        else
                        {

                            rdrProf.Close();
                            kplog.Info("SUCCESS : respcode = 0, message = not yet registered!");
                            return new ProfileResponse { respcode = 0, message = "Not yet registered" };

                        }


                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FATAL:: respcode: 0 , message: "+ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }
        }

        private ProfileResponse getProfile(String CustomerID)
        {

            try
            {
                kplog.Info("PARAMS: CustomerID: " + CustomerID );
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();

                    using (MySqlCommand cmd = con.CreateCommand())
                    {

                        String expirydate;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select FirstName,LastName,MiddleName,Street,ProvinceCity as State, Country,ZipCode,BirthDate,c.HomeCity as City,Gender, Mobile,b.UserID as UserID, b.ImagePath as filepath,b.validID1Front,b.validID1Back,b.validID2Front,b.validID2Back,b.CustomerID,a.IDNo,a.IDType,a.ExpiryDate from kpcustomersglobal.customers a inner join kpcustomersglobal.PayNearMe b on a.CustID = b.CustomerID inner join kpcustomersglobal.customersdetails c on a.CustID = c.CustID where a.custID = @custID";
                        cmd.Parameters.AddWithValue("custID", CustomerID);
                        MySqlDataReader rdrProf = cmd.ExecuteReader();
                        if (rdrProf.HasRows)
                        {


                            rdrProf.Read();
                            String filepath = rdrProf["filepath"].ToString();
                            String filepath1 = rdrProf["validID1Front"].ToString();
                            String filepath2 = rdrProf["validID1Back"].ToString();
                            String filepath3 = rdrProf["validID2Front"].ToString();
                            String filepath4 = rdrProf["validID2Back"].ToString();
                            String bdate = rdrProf["BirthDate"].ToString();
                            if (bdate.StartsWith("00") || bdate == String.Empty)
                            {
                                bdate = "";
                            }
                            else
                            {
                                bdate = Convert.ToDateTime(rdrProf["BirthDate"]).ToString("MM/dd/yyyy");
                            }
                            if (string.IsNullOrEmpty(rdrProf["ExpiryDate"].ToString()) || rdrProf["ExpiryDate"].ToString().StartsWith("00"))
                            {
                                expirydate = "";
                            }
                            else
                            {
                                expirydate = Convert.ToDateTime(rdrProf["ExpiryDate"]).ToString("MM/dd/yyyy");
                            }

                            var senderx = new CustomerModel
                            {
                                firstName = rdrProf["Firstname"].ToString(),
                                middleName = rdrProf["MiddleName"].ToString(),
                                lastName = rdrProf["LastName"].ToString(),
                                Street = rdrProf["Street"].ToString(),
                                State = rdrProf["State"].ToString(),
                                Country = rdrProf["Country"].ToString(),
                                ZipCode = rdrProf["ZipCode"].ToString(),
                                BirthDate = bdate,
                                City = rdrProf["City"].ToString(),
                                Gender = rdrProf["Gender"].ToString(),
                                PhoneNo = rdrProf["Mobile"].ToString(),
                                UserID = rdrProf["UserID"].ToString(),
                                ImagePath = filepath,
                                ImagePath1 = filepath1,
                                ImagePath2 = filepath2,
                                ImagePath3 = filepath3,
                                ImagePath4 = filepath4,
                                CustomerID = rdrProf["CustomerID"].ToString(),
                                IDNo = rdrProf["IDNo"].ToString(),
                                IDType = rdrProf["IDType"].ToString(),
                                ExpiryDate = expirydate
                            };

                            kplog.Info("SUCCESS :: respcode : 1, Message: Success!, Object: " + JsonConvert.SerializeObject(senderx));
                            return new ProfileResponse
                            {
                                respcode = 1,
                                message = "Success",
                                sender = senderx
                            };
                        }
                        else
                        {

                            rdrProf.Close();
                            kplog.Info("SUCCESS : respcode = 0, message = not yet registered!");
                            return new ProfileResponse { respcode = 0, message = "Not yet registered" };

                        }


                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FATAL: respcode : 0 , message: "+ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }
        }


        //done loggings RR
        [HttpGet]
        public CustomerResultResponse resendActivationCode(String UserID, String securityToken)
        {
            kplog.Info("PARAMS: UserID: "+UserID+", securityToken: "+securityToken);

            String activationCode = generateActivationCode();
            String mobileToken = generateMobileToken();

            if (securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new CustomerResultResponse { respcode = 0, message = getRespMessage(7) };
            }
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                String FullName = string.Empty;
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "Select isEmailActivated,FullName from kpcustomersglobal.PayNearMe where UserID = @UserID";
                cmd.Parameters.AddWithValue("UserID", UserID);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    Int32 isEmailAct = Convert.ToInt32(rdr["isEmailActivated"]);
                    FullName = rdr["FullName"].ToString();
                    rdr.Close();

                    if (isEmailAct == 1)
                    {
                        con.Close();
                        kplog.Info("Account Already Activated");
                        return new CustomerResultResponse { respcode = 1, message = "Account already Activated" };

                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET activationCode = @activationCode where UserID = @UserID";
                    cmd.Parameters.AddWithValue("activationCode", activationCode);
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    int x = cmd.ExecuteNonQuery();

                    if (x > 0)
                    {
                        con.Close();
                        sendEmailActivation(UserID, FullName, activationCode, "");
                        kplog.Info("SUCCESS : respcode = 0, message = Success");
                        return new CustomerResultResponse { respcode = 1, message = "Success Sending Activation Code" };
                    }
                    else
                    {
                        con.Close();
                        kplog.Error("SUCCESS : respcode = 0, message = Failed in sending ActivationCode");
                        return new CustomerResultResponse { respcode = 0, message = "Failed" };
                    }


                }
                else
                {

                    con.Close();
                    kplog.Info("SUCCESS : respcode = 0, message = Email does not exist!");
                    return new CustomerResultResponse { respcode = 0, message = "Email does not exist!" };

                }



            }



        }


        //done loggings RR
        [HttpGet]
        public CustomerResultResponse resendMobileToken(String UserID, String securityToken)
        {
            kplog.Info("PARAMS: UserID: "+UserID+", SecurityToken: "+securityToken);

            String mobileToken = generateMobileToken();

            if (securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new CustomerResultResponse { respcode = 0, message = getRespMessage(7) };
            }
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                String FullName = string.Empty;
                MySqlCommand cmd = con.CreateCommand();


                cmd.Parameters.Clear();
                cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET mobileToken = @mobileToken where UserID = @UserID";
                cmd.Parameters.AddWithValue("mobileToken", mobileToken);
                cmd.Parameters.AddWithValue("UserID", UserID);
                int x = cmd.ExecuteNonQuery();

                if (x > 0)
                {

                    
                    con.Close();
                    sendEmailActivation(UserID, FullName, "", mobileToken);
                    kplog.Info("SUCCESS: : respcode : 1 , message: Done Update mobileToken: "+UserID);
                    return new CustomerResultResponse { respcode = 1, message = "Success" };
                }
                else
                {
                    con.Close();
                    kplog.Error("FALED: : respcode : 0 , message: Failed Update MobileToken: "+UserID);
                    return new CustomerResultResponse { respcode = 0, message = "Failed" };
                }

            }

        }

        //done loggings
        [HttpPost]
        public AuthenticateResponse authenticateSignup(AuthenticateRequest req)
        {
            String userID = req.UserID;
            String activationCode = req.ActivationCode;
            kplog.Info("PARAMS: "+ JsonConvert.SerializeObject(req));
            try
            {
                if (req.securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new AuthenticateResponse { respcode = 0, message = getRespMessage(7) };
                }
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();

                    MySqlCommand cmd = con.CreateCommand();

                    cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @UserID;";
                    cmd.Parameters.AddWithValue("UserID", userID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select Password from kpcustomersglobal.PayNearMe where activationCode = @activationCode and UserID = @UserID;";
                        cmd.Parameters.AddWithValue("activationCode", activationCode);
                        cmd.Parameters.AddWithValue("UserID", userID);
                        MySqlDataReader rdrCode = cmd.ExecuteReader();
                        if (rdrCode.HasRows)
                        {
                            rdrCode.Read();
                            String password = rdrCode["Password"].ToString();
                            rdrCode.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET isEmailActivated = 1 where UserID = @UserID and activationCode = @activationCode";
                            cmd.Parameters.AddWithValue("activationCode", activationCode);
                            cmd.Parameters.AddWithValue("UserID", userID);
                            int x = cmd.ExecuteNonQuery();

                            if (x > 0)
                            {
                                con.Close();
                                kplog.Info("SUCCESS!: done Update Activate User: "+userID);
                                return new AuthenticateResponse { respcode = 1, message = "Success", password = password, userID = userID };
                            }
                            else
                            {
                                con.Close();
                                kplog.Error("FAILED: FAILED UPDATE to Activate USER: "+userID);
                                return new AuthenticateResponse { respcode = 0, message = "Failed" };
                            }


                        }
                        else
                        {
                            kplog.Info("SUCCESS : respcode = 0, message = Invalid Activation code!");
                            return new AuthenticateResponse { respcode = 0, message = "Invalid Activation code" };
                        }

                    }
                    else
                    {
                        kplog.Info("SUCCESS : respcode = 0, message = UserID does not exist!");
                        return new AuthenticateResponse { respcode = 0, message = "UserID does not exist!" };
                    }


                }
            }
            catch (Exception ex)
            {
                kplog.Error("FAILED:: respcode: 0 message: '" + getRespMessage(0) + "'  ErrorDetail: '" + ex.ToString() + "'");
                return new AuthenticateResponse { respcode = 0, message = ex.ToString() };
            }



        }


        //done loggings RR
        [HttpPost]
        public CreateOrderResponse createOrder(TransactionModel model)
        {
            kplog.Info(JsonConvert.SerializeObject(model));

            if (model.securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                List<Error> err = new List<Error>();
                err.Add(new Error
                {

                    description = getRespMessage(7) 
                });
                return new CreateOrderResponse { status = "error", errors = err };
            }
            try
            {
                Double dailyLimit = getDailyLimit(model.senderCustID);
                dailyLimit = dailyLimit - model.Principal;
                Double monthLy = getMonthlyLimit(model.senderCustID);
                monthLy = monthLy - model.Principal;
                if (monthLy < 0) 
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "Monthly Limit amount Exceeded"
                    });
                    kplog.Info("Monthly Limit Exceeded: "+model.senderCustID);
                    return new CreateOrderResponse { status = "error", errors = err };
                
                }
                if (dailyLimit < 0) 
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "Daily Limit amount Exceeded"
                    });
                    kplog.Info("Daily Limit Exceeded: " + model.senderCustID);
                    return new CreateOrderResponse { status = "error", errors = err };
                    
                }
                var sender = getProfile(model.senderCustID).sender;
                var receiver = getBeneficiaryInfo(model.receiverCustId).data;
                DateTime dt = getServerDateGlobal();
                model.KPTN = generateKPTNPayNearMe("711", 3, model.TransactionType);
                model.TransDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                String Month = dt.ToString("MM");
                model.Principal = Math.Round(model.Principal, 2);
                string amountTotal = (model.Principal + model.Charge).ToString();

                //generate control
                string station = model.TransactionType == "Web" ? "1" : "2";
                var resp = generateControlGlobal("boswebserviceusr", "boyursa805", "711", 0, "PNME7117113", 3, station);
                string controlNo = string.Empty;
                if (resp.respcode == 1)
                {
                    controlNo = resp.controlno;
                }
                else
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "SQL ERROR: Pls Contact Support"
                    });
                    kplog.Error("Error in GenerateControlGlobal: "+ resp.message);
                    return new CreateOrderResponse { status = "error", errors = err };
                }
                //create-order-pnm
                if (dt.ToString("HH") == "23")
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "Sorry, transactions are not allowed beyond 11PM. Please transact again after 12AM. Thank You! "
                    });
                    kplog.Info("Sorry, transactions are not allowed beyond 11PM. Please transact again after 12AM. Thank You!");
                    return new CreateOrderResponse { status = "error", errors = err };
                }

                string order_expiration_date = dt.ToString("yyyy-MM-dd 23:59:59 PT");
                string site_customer_identifier = controlNo;
                string min_payment = amountTotal;
                string site_customer_name = sender.firstName + " " + sender.lastName;
                string site_customer_email = sender.UserID;
                string site_customer_phone = sender.PhoneNo;
                bool site_customer_sms_ok = sender.sendSMS;
                string site_creator_identifier = sender.CustomerID;
                string order_amount = amountTotal;
                string order_currency = "USD";
                string receiver_user_identifier = receiver.receiverCustID;
                string sender_user_identifier = sender.CustomerID;
                string site_order_identifier = model.KPTN;
                string timestamp = getTimeStamp().ToString();
                string version = "2.0";

                string queryAPI = "minimum_payment_amount=" + min_payment + "&minimum_payment_currency=USD&order_amount=" + order_amount + "&order_currency=" + order_currency + "&order_expiration_date=" + order_expiration_date + "&order_type=exact&receiver_user_identifier=" + receiver_user_identifier +
                    "&return_html_slip=true&return_pdf_slip=true&sender_user_identifier=" + sender_user_identifier +
                    "&site_customer_email=" + site_customer_email +
                    "&site_customer_identifier=" + site_customer_identifier +
                    "&site_customer_name=" + site_customer_name +
                    "&site_customer_phone=" + site_customer_phone + "&site_customer_sms_ok=" + site_customer_sms_ok +
                    "&site_identifier=" + siteIdentifier + "&site_order_description=" + "Test" + "&site_order_identifier=" + site_order_identifier +
                    "&timestamp=" + timestamp + "&version=" + version + "";

                string signature = generateSignature(queryAPI);

                queryAPI = queryAPI + "&signature=" + signature;

                Uri uri = new Uri(server + "/json-api/create_order?" + queryAPI);

                string res = SendRequest(uri);
                kplog.Info(res);
                CreateOrderResponse response = new CreateOrderResponse();

                dynamic data = JsonConvert.DeserializeObject(res, typeof(CreateOrderResponse));
                response = data;


                //XmlSerializer serializer = new XmlSerializer(typeof(CreateOrderResponse));                
                //using (TextReader reader = new StringReader(res)) 
                //{
                //dynamic data = serializer.Deserialize(reader);
                //    response = data;
                //}


                if (response.status == "ok")
                {

                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(connection))
                        {
                            con.Open();
                            using (MySqlCommand cmd = con.CreateCommand())
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = "INSERT INTO paynearme.order" + Month + " (siteOrderIdentifier,pnmOrderIdentifier,siteCustomerIdentifier,receiverIdentifier,senderIdentifier,orderCreated,orderPrincipal,orderChargeML,orderChargePNM,orderExchangeRate,orderAmountTotal,orderType,orderDuration,status,orderTrackingUrl,orderPOAmountPHP,ControlNo) " +
                                    " VALUES(@siteOrderIdentifier,@pnmOrderIdentifier,@siteCustomerIdentifier,@receiverIdentifier,@senderIdentifier,@orderCreated,@orderPrincipal,@orderChargeML,@orderChargePNM,@orderExchangeRate,@orderAmountTotal,@orderType,@orderDuration,@status,@orderTrackingUrl,@orderPOAmountPHP,@ControlNo);";

                                cmd.Parameters.AddWithValue("siteOrderIdentifier", response.order.site_order_identifier);
                                cmd.Parameters.AddWithValue("pnmOrderIdentifier", response.order.pnm_order_identifier);
                                cmd.Parameters.AddWithValue("siteCustomerIdentifier", response.order.customer.site_customer_identifier);
                                cmd.Parameters.AddWithValue("receiverIdentifier", response.order.users.user[1].user_site_identifier);
                                cmd.Parameters.AddWithValue("senderIdentifier", response.order.users.user[0].user_site_identifier);
                                cmd.Parameters.AddWithValue("orderCreated", response.order.order_created);
                                cmd.Parameters.AddWithValue("orderPrincipal", model.Principal);
                                cmd.Parameters.AddWithValue("orderChargeML", (model.Charge - pnmCharge));
                                cmd.Parameters.AddWithValue("orderChargePNM", pnmCharge);
                                cmd.Parameters.AddWithValue("orderExchangeRate", model.ExchangeRate);
                                cmd.Parameters.AddWithValue("orderAmountTotal", response.order.order_amount);
                                cmd.Parameters.AddWithValue("orderType", response.order.order_type);
                                cmd.Parameters.AddWithValue("orderDuration", order_expiration_date);
                                cmd.Parameters.AddWithValue("status", response.order.order_status);
                                cmd.Parameters.AddWithValue("orderTrackingUrl", response.order.order_tracking_url);
                                cmd.Parameters.AddWithValue("orderPOAmountPHP", model.POAmountPHP);
                                cmd.Parameters.AddWithValue("ControlNo", controlNo);
                                int xx = cmd.ExecuteNonQuery();
                                if (xx > 0)
                                {
                                    kplog.Info("SUCCESS:: respcode: 1 message: INSERT paynearme" + Month + " : " + site_order_identifier);
                                    con.Close();
                                    return response;
                                }
                                else
                                {
                                    List<Error> err = new List<Error>();
                                    err.Add(new Error
                                    {

                                        description = "MYSQL ERROR: Pls Contact Support"
                                    });
                                    con.Close();
                                    kplog.Error(JsonConvert.SerializeObject(err));
                                    return new CreateOrderResponse { status = "error", errors = err };
                                }
                            }

                        }
                    }
                    catch (MySqlException ex)
                    {

                        List<Error> err = new List<Error>();
                        err.Add(new Error
                        {

                            description = ex.ToString()
                        });

                        kplog.Fatal(JsonConvert.SerializeObject(err));
                        return new CreateOrderResponse { status = "error", errors = err };
                    }
                   

                }
                else
                {
                    kplog.Error(JsonConvert.SerializeObject(response));
                    return response;
                }
            }
            catch (Exception ex)
            {
                List<Error> err = new List<Error>();
                err.Add(new Error
                {

                    description = ex.ToString()
                });

                kplog.Fatal(JsonConvert.SerializeObject(err));
                return new CreateOrderResponse { status = "error", errors = err };


            }
        }

    
        //done loggings RR
        private getbrachrateclassificationresponse Getbranchrateclassification(String bcode, String zone)
        {
            kplog.Info("SUCCES:: message:: bcode: " + bcode + " zone: " + zone);

          
            try
            {
                String sql;
                sql = "call mlforexrate.sp_getbranchclassification ('" + bcode + "', '" + zone + "') ";

                using (MySqlConnection con = new MySqlConnection(forex))
                {
                    con.Open();
                    command = con.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    MySqlDataReader dr = command.ExecuteReader();

                    if (dr.HasRows == true)
                    {
                        dr.Read();
                        kplog.Info("SUCCESS:: message: " + getRespMessage(1));
                        return new getbrachrateclassificationresponse { rescode = "1", msg = getRespMessage(1), bcode = (string)dr["branchcode"], bname = (string)dr["branchname"], zone = (string)dr["zonecode"], classification = (string)dr["classification"], description = (string)dr["descriptions"], buying = (string)dr["buying"], selling = (string)dr["selling"] };
                    }
                    else
                    {
                        kplog.Error("FAILED:: message: " + getRespMessage(1));
                        return new getbrachrateclassificationresponse { rescode = "0", msg = getRespMessage(0) };
                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.ToString());
                return new getbrachrateclassificationresponse { rescode = "0", msg = ex.Message };
                // createLog.makeLog("ERROR" + ex.Message);
            }

        }


        //done loggings RR
        [HttpGet]
        public getbranchratesresponse GetBranchRates(String bcode, String zone, String currency, String securityToken)
        {
            kplog.Info("PARAMS:: bcode : "+bcode+", zonecode: "+zone+" , currency: "+currency+", securityToken: "+securityToken);
            try
            {
                if (securityToken != secureToken)
                {
                    kplog.Error(getRespMessage(7));
                    return new getbranchratesresponse { rescode = "0", msg = getRespMessage(7) };
                }

                String classification = Getbranchrateclassification(bcode, zone).classification;
                kplog.Info("SUCCES:: message:: bcode: " + bcode + " zone: " + zone + " classification: " + classification + " currency: " + currency);

                String sql = String.Empty;
                String sqlmanual = String.Empty;
                String sqlchk = String.Empty;
                String remarks = String.Empty;
                Int32 identifier = 0;
                DateTime? effectivedate = null;

                sql = "call mlforexrate.sp_getbranchrates ('" + bcode + "', '" + zone + "', '" + currency + "', '" + classification + "');";
                sqlmanual = "SELECT b.branchname,b.branchcode,bm.curr_sell as selling,bm.curr_buy as buying,@pcur as currency FROM mlforexrate.brachrateclassification b INNER JOIN mlforexrate.branchforexmanual bm ON bm.branchcode = b.branchcode and bm.zonecode = b.zonecode WHERE bm.branchcode = @pbcode and bm.zonecode = @pzcode";
                sqlchk = "SELECT remarks, identifier, IF(effectivedate IS NULL,NULL,DATE_FORMAT(effectivedate,'%Y-%m-%d %H:%i:%s')) AS effectivedate FROM mlforexrate.branchforextagrates WHERE branchcode = @bcode and zonecode = @zcode";

                using (MySqlConnection con = new MySqlConnection(forex))
                {
                    command = new MySqlCommand();

                    con.Open();

                    command.CommandText = sqlchk;
                    command.Connection = con;
                    command.CommandType = CommandType.Text;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("bcode", bcode);
                    command.Parameters.AddWithValue("zcode", zone);
                    MySqlDataReader rdrchk = command.ExecuteReader();
                    if (rdrchk.Read())
                    {
                        remarks = rdrchk["remarks"].ToString();
                        identifier = Convert.ToInt32(rdrchk["identifier"].ToString());
                        if (rdrchk["effectivedate"] == DBNull.Value)
                        {
                            effectivedate = null;
                        }
                        else
                        {
                            effectivedate = Convert.ToDateTime(rdrchk["effectivedate"]);
                        }
                    }
                    rdrchk.Close();

                    if (remarks != String.Empty)
                    {
                        if (remarks == "Automate")
                        {
                            command.CommandText = sql;
                            command.Connection = con;
                            command.CommandType = CommandType.Text;
                            command.Parameters.Clear();
                            MySqlDataReader dr = command.ExecuteReader();

                            if (dr.HasRows == true)
                            {
                                dr.Read();
                                //con.Close();
                                kplog.Info("SUCCES:: message: " + getRespMessage(1));
                                return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };


                            }
                            else
                            {
                                kplog.Error(getRespMessage(0));
                                return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                            }
                        }
                        else
                        {
                            DateTime servrdt = Convert.ToDateTime(getServerDateGlobal());
                            int result = DateTime.Compare((DateTime)servrdt, (DateTime)effectivedate);
                            if (result >= 0)
                            {
                                command.CommandText = sqlmanual;
                                command.Connection = con;
                                command.CommandType = CommandType.Text;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("pbcode", bcode);
                                command.Parameters.AddWithValue("pzcode", zone);
                                command.Parameters.AddWithValue("pcur", currency);
                                MySqlDataReader rdrgetmanual = command.ExecuteReader();
                                if (rdrgetmanual.Read())
                                {
                                    kplog.Info("SUCCES:: message: " + getRespMessage(1) + ", selling: " + ((decimal)rdrgetmanual["selling"]).ToString() + ", buying: " + ((decimal)rdrgetmanual["buying"]).ToString() + ", branchcode: " + ((string)rdrgetmanual["branchcode"]).ToString() + ", branchname: " + ((string)rdrgetmanual["branchname"]).ToString() + ", currency: " + ((string)rdrgetmanual["currency"]).ToString());
                                    return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)rdrgetmanual["selling"], buying = (decimal)rdrgetmanual["buying"], branchcode = (string)rdrgetmanual["branchcode"], branchname = (string)rdrgetmanual["branchname"], currency = (string)rdrgetmanual["currency"] };
                                }
                                rdrgetmanual.Close();
                                con.Close();
                            }
                            else
                            {
                                command.CommandText = sql;
                                command.Connection = con;
                                command.CommandType = CommandType.Text;
                                command.Parameters.Clear();
                                MySqlDataReader dr = command.ExecuteReader();

                                if (dr.HasRows == true)
                                {
                                    dr.Read();
                                    kplog.Info("SUCCESS:: message: " + getRespMessage(1) + ", selling: " + ((decimal)dr["selling"]).ToString() + ", buying: " + ((decimal)dr["buying"]).ToString() + ", branchcode :" + ((string)dr["branchcode"]).ToString() + ", branchname: " + ((string)dr["branchname"]).ToString() + ", currency :" + ((string)dr["currency"]).ToString());
                                    return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };

                                }
                                else
                                {
                                    con.Close();
                                    kplog.Error("FAILED:: message: " + getRespMessage(0));
                                    return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                                }
                            }
                        }
                    }
                    else
                    {
                        command.CommandText = sql;
                        command.Connection = con;
                        command.CommandType = CommandType.Text;
                        command.Parameters.Clear();
                        MySqlDataReader dr = command.ExecuteReader();

                        if (dr.HasRows == true)
                        {
                            dr.Read();
                            kplog.Info("SUCCES:: message: " + getRespMessage(1) + ", selling: " + ((decimal)dr["selling"]).ToString() + ", buying: " + ((decimal)dr["buying"]).ToString() + ", branchcode :" + ((string)dr["branchcode"]).ToString() + ", branchname: " + ((string)dr["branchname"]).ToString() + ", currency :" + ((string)dr["currency"]).ToString());
                            return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };

                        }
                        else
                        {
                            con.Close();
                            kplog.Error("FAILED:: message: " + getRespMessage(0));
                            return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                        }
                    }

                    con.Close();
                    kplog.Error("FAILED:: message: " + getRespMessage(0));
                    return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };

                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.ToString());
                return new getbranchratesresponse { rescode = "0", msg = ex.Message };
                //
            }

        }


        //done loggings RR
        [HttpGet]
        public ChargeResponse getChargeTable(String bcode, String zcode, String securityToken)
        {
            if (securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new ChargeResponse { respcode = 0, message = getRespMessage(7) };
            }

            var resp = calculateChargePerBranchGlobalMobile(bcode, zcode);

            if (resp.respcode == 1)
            {
                kplog.Info("SUCCESS: : " + JsonConvert.SerializeObject(resp));
                return resp;
            }
            else if (resp.respcode == 16)
            {
                var response = calculateChargeGlobalMobile(bcode, zcode);
                kplog.Info("SUCCESS: : " + JsonConvert.SerializeObject(response));
                return response;
            }
            else
            {
                kplog.Info("SUCCESS: : " + JsonConvert.SerializeObject(resp));
                return resp;
            }
        }


        public DateTime getServerDateGlobal(Boolean isOpenConnection)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();
                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();


                                kplog.Info("SUCCESS:: Server Date: " + serverdate);
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {
                    DateTime serverdate;

                    command.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = command.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();

                        kplog.Info("SUCCESS:: Server Date: " + serverdate);
                        return serverdate;
                    }
                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
                throw new Exception(ex.Message);
            }
        }
 
        [HttpGet]
        public TokenResponse validateToken(String token, String CustomerID, String securityToken)
        {
            if (securityToken != secureToken){
            
                kplog.Error(getRespMessage(7));
                return new TokenResponse { respcode = 0, message = getRespMessage(7) };
            }

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "Select mobileToken,retry,pinCodeStatus from kpcustomersglobal.PayNearMe where CustomerID =@userID";

                    cmd.Parameters.AddWithValue("userID", CustomerID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        String mobileToken = rdr["mobileToken"].ToString();
                        Int32 retry = Convert.ToInt32(rdr["retry"]);
                        Int32 pinCodeStatus = Convert.ToInt32(rdr["pinCodeStatus"]);
                        rdr.Close();
                        if (pinCodeStatus == 0)
                        {
                            con.Close();
                            return new TokenResponse { respcode = 0, message = "Exceeded maximum attempt, Please contact support to activate your account!" };
                        }
                        else if (mobileToken != token)
                        {

                            if (retry >= 3)
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set pinCodeStatus = '0' where CustomerID = @userID";
                                cmd.Parameters.AddWithValue("userID", CustomerID);
                                cmd.ExecuteNonQuery();

                                con.Close();
                                return new TokenResponse { respcode = 0, message = "Exceeded maximum attempt, Please contact support to activate your account!" };

                            }
                            else
                            {
                                retry++;
                                cmd.Parameters.Clear();
                                cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set retry = '" + retry + "' where CustomerID = @userID";
                                cmd.Parameters.AddWithValue("userID", CustomerID);
                                cmd.ExecuteNonQuery();

                                Int32 retrylimit = 3 - retry;

                                if (retrylimit == 0)
                                {

                                    cmd.Parameters.Clear();
                                    cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set pinCodeStatus = '0' where CustomerID = @userID";
                                    cmd.Parameters.AddWithValue("userID", CustomerID);
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                    return new TokenResponse { respcode = 2, message = "Invalid Pincode, you have reached the maximum attempt. Please contact support to activate account" };
                                }
                                else
                                {
                                    con.Close();
                                    return new TokenResponse { respcode = 0, message = "Invalid Pincode, you have " + retrylimit + " attempts left." };
                                }




                            }
                        }
                        else
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe set Retry = 0 where CustomerID = @userID";
                            cmd.Parameters.AddWithValue("userID", CustomerID);
                            cmd.ExecuteNonQuery();
                            con.Close();
                            return new TokenResponse { respcode = 1, message = "Correct Pin Code" };
                        }



                    }
                    else
                    {


                        con.Close();
                        return new TokenResponse { respcode = 0, message = "Incorrect Pin Code" };
                    }
                }
            }

        }

        [HttpGet]
        public TokenResponse changeToken(String oldtoken, String newtoken, String CustomerID, String securityToken)
        {
            if (securityToken != secureToken)
            {
                kplog.Error(getRespMessage(7));
                return new TokenResponse { respcode = 0, message = getRespMessage(7) };
            }
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where mobileToken = @oldtoken and CustomerID =@userID";
                    cmd.Parameters.AddWithValue("oldtoken", oldtoken);
                    cmd.Parameters.AddWithValue("userID", CustomerID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET mobileToken=@newToken where mobileToken=@oldtoken and CustomerId=@userID;";
                        cmd.Parameters.AddWithValue("userID", CustomerID);
                        cmd.Parameters.AddWithValue("oldtoken", oldtoken);
                        cmd.Parameters.AddWithValue("newToken", newtoken);
                        cmd.ExecuteNonQuery();

                        return new TokenResponse { respcode = 1, message = "PIN CODE is successfully updated!" };
                    }
                    else
                    {
                        return new TokenResponse { respcode = 0, message = "Invalid Old Pin Code" };
                    }
                }
            }

        }

        [HttpGet]
        public List<System.Web.Mvc.SelectListItem> getIDTypes()
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                con.CreateCommand();
                List<System.Web.Mvc.SelectListItem> list = new List<System.Web.Mvc.SelectListItem>();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM kpformsglobal.`sysallowedidtype` WHERE zonecode = 3";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        list.Add(new System.Web.Mvc.SelectListItem
                        {
                            Value = "",
                            Text = "--SELECT--"
                        });

                        while (rdr.Read())
                        {
                            list.Add(new System.Web.Mvc.SelectListItem
                            {
                                Value = rdr["idType"].ToString(),
                                Text = rdr["idType"].ToString()
                            });
                        }
                        rdr.Close();
                        con.Close();
                        return list;
                    }
                    return null;
                }
            }

        }

        
      

        #region private method

        private Double getDailyLimit(String CustID)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {

                DateTime dt = getServerDateGlobal();
                String table = dt.ToString("MM");
                Double limitSO = dailyLimit;
                Double dailySum = 0.0;
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as limitSO from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d')  and senderIdentifier = @custID;";
                    cmd.Parameters.AddWithValue("custID", CustID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        dailySum = Convert.ToDouble(rdr["limitSO"]);
                        limitSO = limitSO - dailySum;
                        if (limitSO < 0)
                        {
                            return 0.00;
                        }
                        else
                        {
                            return limitSO;
                        }
                    }
                    else
                    {
                        return limitSO;
                    }
                }
            }

        }


        private Double getMonthlyLimit(String CustID)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {

                DateTime dt = getServerDateGlobal();
                String table = dt.ToString("MM");
                Double limitMonthly = monthlyLimit;
                Double sumDaily = 0.0;
                Double sumPartial = 0.0;
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as monthlySum  from paynearme.order" + table + " where `status` = 'confirm' and DATE_FORMAT(OrderCreated,'%Y-%m-%d') != DATE_FORMAT(NOW(),'%Y-%m-%d') and senderIdentifier = @custID ;";
                    cmd.Parameters.AddWithValue("custID", CustID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    sumPartial = Convert.ToDouble(rdr["monthlySum"]);

                    if (sumPartial > 0)
                    {


                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as sumDaily from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d') and senderIdentifier = @custID;";
                        cmd.Parameters.AddWithValue("custID", CustID);
                        rdr = cmd.ExecuteReader();
                        rdr.Read();
                        sumDaily = Convert.ToDouble(rdr["sumDaily"]);
                        if (sumDaily > 0)
                        {

                            limitMonthly = limitMonthly - (sumDaily + sumPartial);

                            if (limitMonthly < 0) { limitMonthly = 0.00; }
                            return limitMonthly;



                        }
                        else
                        {
                            limitMonthly = limitMonthly - sumPartial;
                            if (limitMonthly < 0) { limitMonthly = 0.00; }
                            return limitMonthly;
                        }

                    }
                    else
                    {
                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as sumDaily from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d') and senderIdentifier = @custID;";
                        cmd.Parameters.AddWithValue("custID", CustID);
                        rdr = cmd.ExecuteReader();
                        rdr.Read();
                        sumDaily = Convert.ToDouble(rdr["sumDaily"]);
                        if (sumDaily > 0)
                        {


                            limitMonthly = limitMonthly - sumDaily;

                            return limitMonthly;
                        }
                        else
                        {

                            return limitMonthly;
                        }
                    }

                }
            }

        }

       //done loggings
        private void sendEmailActivation(String userID, String firstName, String activationCode, String mobileToken)
        {
            SmtpClient client = new SmtpClient();
            client.EnableSsl = smtpSsl;
            client.UseDefaultCredentials = false;
            client.Host = smtpServer;
            client.Port = 587;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            MailMessage msg = new MailMessage();
            msg.To.Add(userID);

            msg.From = new MailAddress(smtpSender);
            msg.Subject = "ML Remit - Email Activation";


            msg.Body = "Good day Ma'am/Sir " + firstName + ",<br /><br />"
                     + "With Mlhuillier as easy to send money to your friends and family around<br />"
                     + "different parts of the world in a fast, convenient and secure way.<br /><br />"
                     + "Please confirm your e-mail address to activate Mlhuillier account.<br /><br />"
                     + "Activation Code : <b>" + activationCode + "</b> <br /><br />"
                     + "Mobile Pin Code <b>" + mobileToken + "</b>";

            //msg.Body = "<div style=\"font-size: 16px; font-family: Consolas; text-align: justify; margin: 0 auto; width: 500px; color: black; padding: 20px; border-left: 1px solid #130d01; border-right: 1px solid #130d01; border-radius: 20px;\">"
            //       + "<img src='https://mlremit.mlhuillier1.com/paynearme/Images/logo_en.png' style='margin-left:15%'/>"
            //       + "<p> Good day Ma'am/Sir <b>" + firstName + "</b>,</p>"
            //       + "<p>"
            //       + "With M. Lhuillier it is easy to send money to your friends and family around "
            //       + "different parts of the world in a fast, convenient and secure way."
            //       + "</p>"
            //       + "<p> Please confirm your e-mail address to activate <br />M. Lhuillier account. </p>"
            //       + "Let's activate your account! <br />"
            //       + "Activation Code : <b>" + activationCode + "</b> <br />"
            //       + "Mobile Pin Code : <b>" + mobileToken + "</b> <br />"
            //       + "<br /><br />"
            //       + "<div style=\"font-size: 14px; border-top: 1px solid lightgray; text-align: center; padding-top: 5px; background-color: gray;\">"
            //       + "-- This mail is auto generated. Please do not reply. --"
            //       + "</div></div>";

            msg.IsBodyHtml = true;
            try
            {
                client.Send(msg);
                kplog.Info("Sent: " + JsonConvert.SerializeObject(msg));

            }
            catch (Exception err)
            {
                kplog.Error(err.ToString());

            }
        }


        //done loggings
        private void uploadFileImage(string strBase64, string filepath)
        {
            try
            {
                byte[] byteresponse = Convert.FromBase64String(strBase64);

                Stream stream2 = new MemoryStream(byteresponse);
                Uri path = new Uri(filepath);

                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(path);
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential("", "");
                req.ContentLength = stream2.Length;
                Stream reqStream = req.GetRequestStream();
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.CopyTo(reqStream);
                reqStream.Close();

                kplog.Info("SUCCESS upload Image: " + filepath);
            }
            catch (Exception ex)
            {

                kplog.Error("ERROR: FILE UPLOAD ----------> " + filepath + "ERROR: " + ex.ToString());
            }



        }


        private ChargeResponse calculateChargeGlobalMobile(String bcode, String zcode)
        {


            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        Decimal dec = 0;
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();
                        List<ChargeList> listofCharge = new List<ChargeList>();
                        try
                        {
                            String query = "SELECT nextID,currID,nDateEffectivity,cDateEffectivity,cEffective,nextID, NOW() as currentDate FROM kpformsglobal.headercharges WHERE cEffective = 1;";

                            command.CommandText = query;
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.Read())
                            {
                                Int32 nextID = Convert.ToInt32(Reader["nextID"]);
                                Int32 type = Convert.ToInt32(Reader["currID"]);

                                DateTime nDateEffectivity = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? NullDate : Convert.ToDateTime(Reader["nDateEffectivity"]);
                                DateTime currentDate = Convert.ToDateTime(Reader["currentDate"]);

                                if (nextID == 0)
                                {

                                    Reader.Close();
                                    String queryRates = "SELECT * FROM kpformsglobal.charges WHERE `type` = @type;";
                                    command.CommandText = queryRates;

                                    command.Parameters.AddWithValue("type", type);

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (ReaderRates.HasRows)
                                    {
                                        while (ReaderRates.Read())
                                        {
                                            listofCharge.Add(new ChargeList
                                            {
                                                minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                            });
                                        }
                                        ReaderRates.Close();
                                    }
                                }
                                else
                                {
                                    Reader.Close();

                                    int result = DateTime.Compare(nDateEffectivity, currentDate);

                                    if (result < 0)
                                    {



                                        command.Transaction = trans;
                                        command.Parameters.Clear();
                                        String updateRates = "update kpformsglobal.headercharges SET  cEffective = 2 where cEffective = 1";
                                        command.CommandText = updateRates;
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String updateRates1 = "update kpformsglobal.headercharges SET cEffective = 1 where currID = @curr";
                                        command.CommandText = updateRates1;
                                        command.Parameters.AddWithValue("curr", nextID);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String insertLog = "insert into kpadminlogsglobal.kpratesupdatelogs (ModifiedRatesID, NewRatesID, DateModified, Modifier) values (@ModifiedRatesID, @NewRatesID, NOW(), @Modifier);";
                                        command.CommandText = insertLog;
                                        command.Parameters.AddWithValue("ModifiedRatesID", nextID - 1);
                                        command.Parameters.AddWithValue("NewRatesID", nextID);
                                        command.Parameters.AddWithValue("Modifier", "boskpws");
                                        command.ExecuteNonQuery();

                                        trans.Commit();

                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.headercharges: SET cEffective: 2 WHERE cEffective: 1");
                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.headercharges: SET cEffective: 1 WHERE currID: " + nextID);
                                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.kpratesupdatelogs: ModifiedRatesID: " + (nextID - 1) + " " +
                                            "NewRatesID: " + nextID + " " +
                                            "Modifier: boskpws");


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT * FROM kpformsglobal.charges WHERE `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT * WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charges: " + listofCharge);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), listofcharges = listofCharge };


                        }
                        catch (MySqlException mex)
                        {
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + mex.ToString());
                            trans.Rollback();
                            conn.Close();
                            return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = mex.ToString() };
                        }
                    }

                }
                catch (Exception ex)
                {
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                    conn.Close();
                    return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                }
            }
        }


        private String ConvertDateTime(String date)
        {

            string month = "";
            string day = "";
            string year = "";

            year = date.Substring(6, 4);
            day = date.Substring(3, 2);
            month = date.Substring(0, 2);

            date = year + "-" + month + "-" + day;

            date = Convert.ToDateTime(date).ToString("yyyy-MM-dd 00:00:00");
            return date;



        }

        private DateTime getServerDateGlobal()
        {
            try
            {
                //throw new Exception(isOpenConnection.ToString());             
                using (MySqlConnection conn = new MySqlConnection(connection))
                {
                    conn.Open();
                    using (MySqlCommand command = conn.CreateCommand())
                    {
                        DateTime serverdate;

                        command.CommandText = "Select NOW() as serverdt;";
                        using (MySqlDataReader Reader = command.ExecuteReader())
                        {
                            Reader.Read();
                            serverdate = Convert.ToDateTime(Reader["serverdt"]);
                            Reader.Close();
                            conn.Close();


                            kplog.Info("SUCCESS:: Server Date: " + serverdate);
                            return serverdate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        private Boolean OfacMatch(String name)
        {
            Int32 Percentage = 100;

            using (MySqlConnection con = new MySqlConnection(dbconofac))
            {
                try
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {


                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT * FROM( " +
                        "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType,  split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) AS placeofbirth, a.fullName as alias, o.soundexvalue, " +
                        "ROUND(JaroWinkler((o.fullName),@FullName)*100,0) as score1, " +
                        "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0) as score2, " +
                        "ROUND(JaroWinkler((o.lastname),@FullName)*100,0) as score3, " +
                        "ROUND(JaroWinkler((o.firstname),@FullName)*100,0) as score4 " +
                        "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
                        "ROUND(JaroWinkler((o.fullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.firstName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.lastName),@FullName)*100,0)>=@Percent " +
                        " UNION DISTINCT " +
                        "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType, split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) " +
                        " AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) " +
                        " AS placeofbirth, a.fullName as alias, a.soundexvalue, " +
                        "ROUND(JaroWinkler((a.fullName),@FullName)*100,0) as score1, " +
                        "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0) as score2, " +
                        "ROUND(JaroWinkler((a.lastname),@FullName)*100,0) as score3, " +
                        "ROUND(JaroWinkler((a.firstname),@FullName)*100,0) as score4 " +
                        "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
                        "ROUND(JaroWinkler((a.fullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0)>=@Percent or " +
                        "ROUND(JaroWinkler((a.firstName),@FullName)*100,0)>=@Percent or " +
                        "ROUND(JaroWinkler((a.lastName),@FullName)*100,0)>=@Percent )as xx";

                        cmd.Parameters.AddWithValue("FullName", name);
                        cmd.Parameters.AddWithValue("Percent", Percentage);
                        MySqlDataReader rcvRdr = cmd.ExecuteReader();

                        if (rcvRdr.HasRows)
                        {
                            con.Close();
                            return true;
                        }
                        else
                        {
                            con.Close();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    kplog.Error("ERROR : '" + ex.ToString() + "'");
                    throw new Exception(ex.ToString());
                }
            }
        }

        private ChargeResponse calculateChargePerBranchGlobalMobile(String bcode, String zcode)
        {

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();

                        try
                        {

                            List<ChargeList> listofCharge = new List<ChargeList>();
                            String query = "SELECT nextID,currID,nDateEffectivity,cDateEffectivity,cEffective,nextID, NOW() as currentDate FROM kpformsglobal.ratesperbranchheader WHERE cEffective = 1 and branchcode = @bcode and zonecode = @zcode;";

                            command.CommandText = query;
                            command.Parameters.AddWithValue("bcode", bcode);
                            command.Parameters.AddWithValue("zcode", zcode);
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.Read())
                            {
                                Int32 nextID = Convert.ToInt32(Reader["nextID"]);
                                Int32 type = Convert.ToInt32(Reader["currID"]);
                                //String ndate = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? null : Convert.ToDateTime(Reader["nDateEffectivity"]).ToString();
                                DateTime nDateEffectivity = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? NullDate : Convert.ToDateTime(Reader["nDateEffectivity"]);
                                DateTime currentDate = Convert.ToDateTime(Reader["currentDate"]);
                                //throw new Exception(nDateEffectivity.ToString());
                                if (nextID == 0)
                                {
                                    Reader.Close();
                                    String queryRates = "SELECT * FROM kpformsglobal.ratesperbranchcharges WHERE  `type` = @type;";
                                    command.CommandText = queryRates;
                                    command.Parameters.AddWithValue("type", type);
                                    MySqlDataReader rdr = command.ExecuteReader();

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (rdr.HasRows)
                                    {
                                        while (rdr.Read())
                                        {
                                            listofCharge.Add(new ChargeList
                                            {
                                                minAmount = Convert.ToDouble(rdr["MinAmount"]),
                                                maxAmount = Convert.ToDouble(rdr["MaxAmount"]),
                                                chargeValue = Convert.ToDouble(rdr["ChargeValue"]) + pnmCharge,
                                            });
                                        }
                                        rdr.Close();
                                    }

                                }
                                else
                                {
                                    Reader.Close();

                                    int result = DateTime.Compare(nDateEffectivity, currentDate);

                                    if (result < 0)
                                    {


                                        command.Transaction = trans;
                                        command.Parameters.Clear();
                                        String updateRates = "update kpformsglobal.ratesperbranchheader SET  cEffective = 2 where cEffective = 1 and branchcode = @bcode and zonecode = @zcode";
                                        command.CommandText = updateRates;
                                        command.Parameters.AddWithValue("bcode", bcode);
                                        command.Parameters.AddWithValue("zcode", zcode);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String updateRates1 = "update kpformsglobal.ratesperbranchheader SET cEffective = 1 where currID = @curr and branchcode = @bcode and zonecode = @zcode";
                                        command.CommandText = updateRates1;
                                        command.Parameters.AddWithValue("curr", nextID);
                                        command.Parameters.AddWithValue("bcode", bcode);
                                        command.Parameters.AddWithValue("zcode", zcode);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String insertLog = "insert into kpadminlogsglobal.kpratesupdatelogs (ModifiedRatesID, NewRatesID, DateModified, Modifier) values (@ModifiedRatesID, @NewRatesID, NOW(), @Modifier);";
                                        command.CommandText = insertLog;
                                        command.Parameters.AddWithValue("ModifiedRatesID", nextID - 1);
                                        command.Parameters.AddWithValue("NewRatesID", nextID);
                                        command.Parameters.AddWithValue("Modifier", "boskpws");
                                        command.ExecuteNonQuery();

                                        trans.Commit();

                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.ratesperbranchheader: SET cEffective: 2 WHERE cEffective: 1 AND branchcode: " + bcode + " AND zonecode: " + zcode);
                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.ratesperbranchheader: SET cEffective: 1 WHERE currID: " + nextID + " AND branchcode: " + bcode + " AND zonecode: " + zcode);
                                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.kpratesupdatelogs: ModifiedRatesID: " + (nextID - 1) + " " +
                                            "NewRatesID: " + nextID + " " +
                                            "Modifier: boskpws");



                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.ratesperbranchcharges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {
                                        //ReaderNextRates.Close();


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT * FROM kpformsglobal.ratesperbranchcharges WHERE  `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            else
                            {
                                kplog.Error("FAILED:: respcode: 16 message: " + getRespMessage(16) + " charges: " + listofCharge);
                                Reader.Close();
                                conn.Close();
                                return new ChargeResponse { respcode = 16, message = getRespMessage(16), listofcharges = listofCharge };
                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charges: " + listofCharge);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), listofcharges = listofCharge };
                        }
                        catch (MySqlException mex)
                        {
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + mex.ToString());
                            trans.Rollback();
                            conn.Close();
                            return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = mex.ToString() };
                        }
                    }

                }
                catch (Exception ex)
                {
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                    conn.Close();
                    return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                }
            }
        }
        private String getcustomertable(String lastname)
        {
            String customers = "";
            lastname.ToUpper();
            if (lastname.StartsWith("A") || lastname.StartsWith("B") || lastname.StartsWith("C"))
            {
                customers = "AtoC";
            }
            else if (lastname.StartsWith("D") || lastname.StartsWith("E") || lastname.StartsWith("F"))
            {
                customers = "DtoF";
            }
            else if (lastname.StartsWith("G") || lastname.StartsWith("H") || lastname.StartsWith("I"))
            {
                customers = "GtoI";
            }
            else if (lastname.StartsWith("J") || lastname.StartsWith("K") || lastname.StartsWith("L"))
            {
                customers = "JtoL";
            }
            else if (lastname.StartsWith("M") || lastname.StartsWith("N") || lastname.StartsWith("O"))
            {
                customers = "MtoO";
            }
            else if (lastname.StartsWith("P") || lastname.StartsWith("Q") || lastname.StartsWith("R"))
            {
                customers = "PtoR";
            }
            else if (lastname.StartsWith("S") || lastname.StartsWith("T") || lastname.StartsWith("U"))
            {
                customers = "StoU";
            }
            else if (lastname.StartsWith("V") || lastname.StartsWith("W") || lastname.StartsWith("X"))
            {
                customers = "VtoX";
            }
            else if (lastname.StartsWith("Y") || lastname.StartsWith("Z"))
            {
                customers = "YtoZ";
            }

            kplog.Info("SUCCESS:: TableCustomer: " + customers);
            return customers;
        }


        private async Task<String> ExpectID_IQ_Check(CustomerModel model)//ExpectID_IQ_Fields fields)
        {
            #region inputs

            DateTime bDate = Convert.ToDateTime(model.BirthDate);

            var values = new Dictionary<string, string> {
                                                            { "username", iDologyUser },//*
                                                            { "password", iDologyPass },//*
                                                            { "invoice", "" },
                                                            { "amount", "" },
                                                            { "shipping", "" },
                                                            { "tax", "" },
                                                            { "total", "" },
                                                            { "idType", model.IDType },
                                                            { "idIssuer", "" },
                                                            { "idNumber", model.IDNo },
                                                            { "paymentMethod", "" },
                                                            { "firstName", model.firstName },//*
                                                            { "lastName", model.lastName },//*
                                                            { "address", model.Street },//*
                                                            { "city", model.City },//*
                                                            { "state", model.StateAbbr },//*
                                                            { "zip", model.ZipCode },//*
                                                            { "ssnLast4", "" },
                                                            { "ssn", "" },
                                                            { "dobMonth", bDate.ToString("MM") },
                                                            { "dobDay",bDate.ToString("dd")},
                                                            { "dobYear",bDate.ToString("yyyy") },
                                                            { "ipAddress", "" },
                                                            { "email", model.UserID },
                                                            { "telephone", model.PhoneNo },
                                                            { "sku", "" },
                                                            { "uid", "" },
                                                            { "altAddress", "" },
                                                            { "altCity", "" },
                                                            { "altState", "" },
                                                            { "altZip", "" },
                                                            { "purchaseDate", "" },
                                                            { "captureQueryId", "" },
                                                            { "score", "" },
                                                            { "c_custome_field_1", "" },
                                                            { "c_custome_field_2", "" },
                                                            { "c_custome_field_3", "" },
                                                        };
            #endregion

            String returnee = "FAIL";
            HttpContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(iDologyServer, content).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(responseString);
            XmlNode xn;

            List<String> errList = new List<String> { };
            try
            {
                xn = xml.SelectSingleNode("/response/summary-result")["key"];
                if (xn.InnerText == "id.success")
                {
                    returnee = "PASS";
                }
            }
            catch (Exception)
            {
                XmlNodeList xnList = xml.SelectNodes("/response/error");
                foreach (XmlNode xmlNode in xnList)
                {
                    errList.Add(xmlNode.InnerText);
                }
                kplog.Error(errList);
                returnee = "ERROR";
            }
            return returnee;
        }

        private String cleanString(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

                return myTI.ToTitleCase(str.Trim());

            }
            else
            {
                return "";
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private void generateReportMobile(List<TransactionDetailsM> TransactionDetailsM, String CustomerID)
        {

            try
            {
                CustomerModel model = getProfile(CustomerID).sender;
                String name = model.firstName + " " + model.lastName;
                DateTime dt = getServerDateGlobal(false);
                String email = model.UserID;
                String fileName = dt.ToString("yyyy-MM-dd") + "_" + generateActivationCode();
                MLRemitWebAPI.Reports.MobileTransReport rpt = new MLRemitWebAPI.Reports.MobileTransReport();
             

                rpt.SetDataSource(TransactionDetailsM);
                rpt.SetParameterValue("accountid", name);
                rpt.SetParameterValue("Date", dt.ToString("MM/dd/yyyy"));


                using (var stream = rpt.ExportToStream(ExportFormatType.PortableDocFormat))
                {

                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = smtpSsl;
                    client.UseDefaultCredentials = false;;
                    client.Host = smtpServer;
                    client.Port = 587;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    MailMessage msg = new MailMessage();
                    msg.To.Add(email);

                    msg.From = new MailAddress(smtpSender);
                    msg.Subject = "ML Remit - Transaction Report";

                    msg.Body = "Good day Ma'am/Sir " + model.firstName + " ,<br /><br />"
                               + "With Mlhuillier as easy to send money to your freinds and family around<br />"
                               + "different parts of the world in a fast, convenient and secure way.<br /><br />"
                               + "Attached is your Transaction Report.<br /><br />";

                    msg.Attachments.Add(new Attachment(stream, fileName + ".pdf"));
                    msg.IsBodyHtml = true;
                    client.Send(msg);

                }

            }
            catch (Exception ex)
            {

                kplog.Error(ex.ToString());
                throw;
            }


        }

        private String generateKPTNPayNearMe(String branchcode, Int32 zonecode, String TransactionType)
        {
            kplog.Info("START--- > PARAMS: bcode" + branchcode + " zcode" + zonecode + " TransactionType " + TransactionType);
            try
            {
                String guid = Guid.NewGuid().GetHashCode().ToString();
                Random rand = new Random();
                dt = getServerDateGlobal(false);
                jp.takel.PseudoRandom.MersenneTwister randGen = new jp.takel.PseudoRandom.MersenneTwister((uint)HiResDateTime.UtcNow.Ticks);

                string randNum = string.Empty;
                while (randNum.Length != 9)
                {
                    randNum = randGen.Next(1, int.MaxValue).ToString().PadLeft(9, '0');
                }


                if (TransactionType == "Web")
                {
                    kplog.Info("SUCCESS:: KPTN: " + ("MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "W" + randNum + dt.ToString("MM")));
                    return "MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "W" + randNum + dt.ToString("MM");
                }
                else
                {
                    kplog.Info("SUCCESS:: KPTN: " + ("MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "M" + randNum + dt.ToString("MM")));
                    return "MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "M" + randNum + dt.ToString("MM");
                }



            }
            catch (Exception a)
            {
                kplog.Fatal("FAILED:: message: " + a.Message + " ErrorDetail: " + a.ToString());
                throw new Exception(a.ToString());
            }
        }
        //done loggings
        private ControlResponse generateControlGlobal(String Username, String Password, String branchcode, Int32 type, String OperatorID, Int32 ZoneCode, String StationNumber)
        {
            kplog.Info("Username: " + Username + ", Password: " + Password + ", BranchCode: " + branchcode + "ZoneCode: " + ZoneCode + ", OperatorID: " + OperatorID);
            if (StationNumber.ToString().Equals("0"))
            {
                kplog.Error("FAILED:: respcode: 13 message: " + getRespMessage(13));
                return new ControlResponse { respcode = 13, message = getRespMessage(13) };
            }
         
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connection))
                {
                    using (command = conn.CreateCommand())
                    {
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                        command.Transaction = trans;
                        try
                        {
                            dt = getServerDateGlobal(true);
                            String control = "MLG";
                            Int32 nseries;
                            String nseries1 = string.Empty;
                            String getcontrolmax = string.Empty;



                            command.CommandText = "Select station, bcode, userid, nseries, zcode, type from kpformsglobal.control where station = @st and bcode = @bcode and zcode = @zcode and `type` = @tp FOR UPDATE";

                            command.Parameters.AddWithValue("st", StationNumber);
                            command.Parameters.AddWithValue("bcode", branchcode);
                            command.Parameters.AddWithValue("zcode", ZoneCode);
                            command.Parameters.AddWithValue("tp", type);
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.HasRows)
                            {

                                Reader.Read();

                                if (type == 0)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 1)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 2)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else if (type == 3)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else
                                {
                                    kplog.Error("FAILED:: message: Invalid type value");
                                    throw new Exception("Invalid type value");
                                }
                                String s = Reader["Station"].ToString();
                                nseries = Convert.ToInt32(Reader["nseries"].ToString().PadLeft(6, '0'));
                                Reader.Close();


                                command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("st", StationNumber);
                                command.Parameters.AddWithValue("bcode", branchcode);
                                command.Parameters.AddWithValue("series", nseries + 1 > 999999 ? 000001 : nseries + 1);
                                command.Parameters.AddWithValue("zcode", ZoneCode);
                                command.Parameters.AddWithValue("tp", type);
                                int abc101 = command.ExecuteNonQuery();


                                trans.Commit();
                                conn.Close();



                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " controlno: " + (control + "-" + dt.ToString("yy") + "-" + nseries.ToString().PadLeft(6, '0')) + " nseries: " + nseries);
                                return new ControlResponse { respcode = 1, message = getRespMessage(1), controlno = control + "-" + dt.ToString("yy") + "-" + nseries.ToString().PadLeft(6, '0'), nseries = nseries.ToString().PadLeft(6, '0') };

                            }
                            else
                            {
                                Reader.Close();
                                command.CommandText = "Insert into kpformsglobal.control (`station`,`bcode`,`userid`,`nseries`,`zcode`, `type`) values (@station,@branchcode,@uid,1,@zonecode,@type)";
                                if (type == 0)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 1)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 2)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else if (type == 3)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else
                                {
                                    kplog.Error("FAILED:: message: Invalid type value");
                                    throw new Exception("Invalid type value");
                                }
                                command.Parameters.AddWithValue("station", StationNumber);
                                command.Parameters.AddWithValue("branchcode", branchcode);
                                command.Parameters.AddWithValue("uid", OperatorID);
                                command.Parameters.AddWithValue("zonecode", ZoneCode);
                                command.Parameters.AddWithValue("type", type);
                                int x = command.ExecuteNonQuery();

                                trans.Commit();
                                conn.Close();

                                kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.control: station: " + StationNumber + " " +
                                "branchcode: " + branchcode + " " +
                                "uid: " + OperatorID + " " +
                                "zonecode: " + ZoneCode + " " +
                                "type: " + type);

                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " control: " + (control + "-" + dt.ToString("yy") + "-" + "000001") + " nseries: 000001");
                                return new ControlResponse { respcode = 1, message = getRespMessage(1), controlno = control + "-" + dt.ToString("yy") + "-" + "000001", nseries = "000001" };
                            }
                        }
                        catch (MySqlException ex)
                        {
                            trans.Rollback();
                            conn.Close();
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                            return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());

                return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
            }

            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());

                return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
            }
        }

        private String getRespMessage(Int32 code)
        {
            String x = "SYSTEM_ERROR";
            switch (code)
            {
                case 1:
                    return x = "Success";
                case 2:
                    return x = "Duplicate kptn";
                case 3:
                    return x = "KPTN already claimed";
                case 4:
                    return x = "KPTN not found";
                case 5:
                    return x = "Customer not found";
                case 6:
                    return x = "Customer already exist";
                case 7:
                    return x = "Invalid credentials";
                case 8:
                    return x = "KPTN already cancelled";
                case 9:
                    return x = "Transaction is not yet claimed";
                case 10:
                    return x = "Version does not match";
                case 11:
                    return x = "Problem occured during saving. Please resave the transaction.";
                case 12:
                    return x = "Problem saving transaction. Please close the sendout form and open it again. Thank you.";
                case 13:
                    return x = "Invalid station number.";
                case 14:
                    return x = "Error generating receipt number.";
                case 15:
                    return x = "Unable to save transaction. Invalid amount provided.";
                default:
                    return x;
            }


        }

        private DateTime getServerDateGlobal(Boolean isOpenConnection, MySqlCommand mycommand)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();

                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();


                                kplog.Info("SUCCESS: server = '" + serverdate + "'");
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {

                    DateTime serverdate = DateTime.Now;

                    mycommand.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = mycommand.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();
                        kplog.Info("SUCCESS: server = '" + serverdate + "'");
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Error("ERROR: message = '" + ex.ToString() + "'");
                throw new Exception(ex.Message);
            }
        }

        private DateTime getServerDateGlobal1(Boolean isOpenConnection)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();

                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();
                                kplog.Info("SUCCESS: server = '" + serverdate + "'");
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {

                    DateTime serverdate;

                    command.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = command.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();
                        kplog.Info("SUCCESS: server = '" + serverdate + "'");
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Error("ERROR: message = '" + ex.ToString() + "'");
                throw new Exception(ex.Message);
            }
        }

        private String generateCustIDGlobal(MySqlCommand command)
        {
            try
            {
                dt = getServerDateGlobal(true, command);

                String query = "select series from kpformsglobal.customerseries";
                command.CommandText = query;
                MySqlDataReader Reader = command.ExecuteReader();

                Reader.Read();
                String series = Reader["series"].ToString();
                Reader.Close();

                return "N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0');
            }
            catch (Exception ex)
            {
                //kplog.Fatal(ex.ToString());
                throw new Exception(ex.ToString());
            }

        }
        //done loggings
        private String generateBeneficiaryCustIDGlobal(MySqlCommand command)
        {
            try
            {

                dt = getServerDateGlobal(true, command);

                String query = "select series from kpformsglobal.beneficiaryseries";
                command.CommandText = query;
                MySqlDataReader Reader = command.ExecuteReader();

                Reader.Read();
                String series = Reader["series"].ToString();
                Reader.Close();
                kplog.Info("N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0'));
                return "N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0');

            }
            catch (Exception ex)
            {
                //kplog.Fatal(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }


        private String generateSignature(string query)
        {
            var x = query.Replace("=", string.Empty);
            var y = x.Replace("&", string.Empty);


            query = y + secretKey;

            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, query);

                return hash;
            }


        }



        private String SendRequest(Uri uri)
        {
            try
            {
                String res = null;
                HttpWebRequest web = (HttpWebRequest)WebRequest.Create(uri);
                web.Method = "GET";
                WebResponse webresponse = web.GetResponse();
                Stream response = webresponse.GetResponseStream();
                res = new StreamReader(response).ReadToEnd();
                webresponse.Close();


                return res;
            }
            catch (Exception ex)
            {
                //Kplog.Fatal(ex.ToString());
                return "Unable to process request. The system encountered some technical problem. Sorry for the inconvenience. " + ex.ToString() + "";
            }
        }

        private Int32 getTimeStamp()
        {

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT UNIX_TIMESTAMP() as tstamp;";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    return Convert.ToInt32(rdr["tstamp"]);
                }

            }
        }

        private String generateActivationCode()
        {
            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                custconn.Open();
                using (MySqlCommand custcommand = custconn.CreateCommand())
                {
                    custcommand.CommandText = "select now()+0 as serverdt";
                    MySqlDataReader rdrserverdt = custcommand.ExecuteReader();
                    rdrserverdt.Read();
                    string x = rdrserverdt["serverdt"].ToString().Substring(8, 6);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;

                }
            }
        }

        private String generateMobileToken()
        {
            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                custconn.Open();
                using (MySqlCommand custcommand = custconn.CreateCommand())
                {
                    custcommand.CommandText = "select now()+0 as serverdt";
                    MySqlDataReader rdrserverdt = custcommand.ExecuteReader();
                    rdrserverdt.Read();
                    string x = rdrserverdt["serverdt"].ToString().Substring(10, 4);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;

                }
            }
        }
        #endregion

        // Forgot Password API
        // Added: Khevin R. Tulang
        // Date : September 29, 2017
        [HttpGet]
        public ForgotPasswordModelResponse CheckEmail(String email, String token)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            String secCode = string.Empty;
            String cid = string.Empty;
            String fn = string.Empty;

            response.code = 0;

            if (token != secureToken)
            {
                response.message = "unidentified token...";
                return response;
            }

            if (!string.IsNullOrEmpty(email))
            {
                try
                {   using (MySqlConnection con = new MySqlConnection(connection))
                    {   con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "Select UserID, CustomerID, FullName, securityCode from kpcustomersglobal.PayNearMe where UserID = @email";
                            cmd.Parameters.AddWithValue("email", email);

                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Read();
                                secCode = rdr["securityCode"].ToString();
                                    cid = rdr["CustomerID"].ToString();
                                     fn = rdr["FullName"].ToString();
                                rdr.Close();
                                response = requestCode(email, cid, fn, secCode);
                            }
                            else
                                response.message = "The email address specified <br /> has not been registered...";
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.code = -1;
                    response.message = "A System Error has occured Please try again...";
                    kplog.Error("[CheckEmail] System Error: " + ex.Message);
                }
            }
            else
                response.message = "Please kindly provide an <br /> email address...";

            return response;
        }

        //generates 4 characters of alphanumeric combination at random as security code
        private String generateSecurityCode()
        {
            Random random = new Random();
            const string chars = "9AB8CD7EF6GH5IJ4KL3MN2OP1QR0ST9UV8WX7YZ6012345";
            return new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private ForgotPasswordModelResponse requestCode(String email, String cid, String fn, String securityCode)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        if (securityCode == string.Empty)
                        {
                            securityCode = generateSecurityCode();
                            cmd.CommandText = "UPDATE `kpcustomersglobal`.`PayNearMe` SET securityCode = @securityCode WHERE CustomerID = @custID AND FullName = @fullName";
                            cmd.Parameters.AddWithValue("securityCode", securityCode);
                            cmd.Parameters.AddWithValue("custID", cid);
                            cmd.Parameters.AddWithValue("fullName", fn);
                            if (cmd.ExecuteNonQuery() <= 0)
                                throw new Exception("Server Error : failed to update security code...");
                        }

                        if (sendSecurityCode(email, securityCode, cid, fn))
                        {
                            response.code = 1;
                            response.message = "An e-mail has been sent, please check your e-mail to proceed";
                        }
                        else
                        {
                            response.code = 0;
                            response.message = "Service error : Unable to mail code... Send request timeout";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Error("[RequestCode] where email = '" + email + "', encCID = '" + cid + "', encFN = '" + fn + "'; ErrorMessage = " + ex.ToString());
                response.code = -1;
                response.message = "Server error upon generating security code";
            }
            return response;
        }

        private string generateAutoForgotPasswordLink(String email, String securityCode, String custID, String fullName)
        {
            string baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            string controllerLink = "/fp?";
            string encData = "e=" + encdata.AESEncrypt(email.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "sc=" + encdata.AESEncrypt(securityCode.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "cid=" + encdata.AESEncrypt(custID.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "fn=" + encdata.AESEncrypt(fullName.ToString(), encStringKey).Replace(' ', '+');
            return baseUrl + controllerLink + encData;
        }

        private Boolean sendSecurityCode(String email, String SecurityCode, String CustID, String FullName)
        {
            String autolink = generateAutoForgotPasswordLink(email, SecurityCode, CustID, FullName);

            SmtpClient client = new SmtpClient();
            client.EnableSsl = smtpSsl;
            client.UseDefaultCredentials = true;
            client.Host = smtpServer;
            client.Port = 587;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            MailMessage msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress("ML Remit<" + smtpSender + ">");
            msg.Subject = "ML Remit - Request to reset password";
            msg.Body = "<div style=\"font-size: 16px; font-family: Consolas; text-align: justify; margin: 0 auto; width: 500px; color: black; padding: 20px; border-left: 1px solid #FFF0CA; border-right: 1px solid #FFF0CA; border-radius: 20px;\">"
                     + "<p> Good day Ma'am/Sir <b>" + FullName + "</b>,</p>"
                     + "<p>"
                     + "<b>M. Lhuillier</b> goes online with "
                     + "<b>ML Remit</b> - fast - convenient - safe."
                     + "</p><br />"
                     + "<p> You have requested for resetting your password. </p>"
                     + "Let's retrieve your account! <br />"
                     //+ "Security Code : <b>" + SecurityCode + "</b> <br />"
                     //+ "Copy and enter the code provided or just "
                     + "<a href=\"" + autolink + "\" target=\"_blank\" style=\"font-size: 20px;\"> Click Here </a>"
                     + "to proceed on on your request"
                     + "<br /><br />"
                     + "If for instance the above button link doesn't work please copy and paste the link below to your browser to proceed."
                     + "<br /><br /><code>" + autolink
                     + "</code><br /><br />"
                     + "<div style=\"font-size: 14px; background-color: lightgray; padding: 0 15px; \">"
                     + "If by in any chance you did not forget or you have remembered your password or "
                     + "you did not request this e-mail then please ignore this message. Thank You!"
                     + "</div><br /><br />"
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
