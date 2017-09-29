using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace PayNearMe.Models.Response
{
    public class AuthorizationResponseBuilder : AbstractXmlResponseBuilder
    {
        public string SitePaymentIdentifier { get; set; }
        public bool AcceptPayment { get; set; }
        public string Receipt { get; set; }
        public string Memo { get; set; }

        public AuthorizationResponseBuilder(string version)
            : base("payment_authorization_response", version)
        {
        }

        public override XmlDocument Build()
        {
            XmlElement authorization = createElement(Root, "authorization");
            createElement(authorization, "pnm_order_identifier", PnmOrderIdentifier);
            createElement(authorization, "accept_payment", AcceptPayment ? "yes" : "no");
            if (Receipt != null)
                createElement(authorization, "receipt", Receipt);
            if (Memo != null)
                createElement(authorization, "memo", Memo);
            createElement(authorization, "site_payment_identifier", SitePaymentIdentifier);
            return Document;
        }

     

  

    
       
    }


     public abstract class AbstractXmlResponseBuilder
	{
        protected XmlDocument Document { get; set; }
        private XmlElement root;
        protected XmlElement Root { get { return root; } }
        private string pnmNamespace;

        // Common field
        public string PnmOrderIdentifier { get; set; }

        public AbstractXmlResponseBuilder (string rootElement, string version)
		{
            string versionCode = version.Replace('.', '_');
            pnmNamespace = "http://www.paynearme.com/api/pnm_xmlschema_v" + versionCode;

            Document = new XmlDocument();
            root = Document.CreateElement("t", rootElement, pnmNamespace);
            root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            root.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.paynearme.com/api/pnm_xmlschema_" + versionCode + " pnm_xmlschema_" + versionCode + ".xsd");
            root.SetAttribute("version", version);
            Document.AppendChild(root);
		}

        protected XmlElement createElement(XmlElement parent, string name, string value) {
            XmlElement e = Document.CreateElement("t", name, pnmNamespace);
            if (value != null) e.InnerText = value;
            parent.AppendChild(e);
            return e;
        }

        protected XmlElement createElement(XmlElement parent, string name) {
            return createElement(parent, name, null);
        }

        public abstract XmlDocument Build();
        
	}

     public class ConfirmationResponsebuilder : AbstractXmlResponseBuilder
     {
         public string OrderIdentifier { get; set; }

         public ConfirmationResponsebuilder(string version)
             : base("payment_confirmation_response", version)
         {
         }

         public override XmlDocument Build()
         {
             XmlElement confirmation = createElement(Root, "confirmation");
             createElement(confirmation, "pnm_order_identifier", PnmOrderIdentifier);
             return Document;
         }
     }


     public class SignatureUtils
     {
         private static List<string> REJECT = new List<string>(new string[] { "signature" });

         private SignatureUtils()
         {
         }

         public static string Signature(IEnumerable<KeyValuePair<string,string>> dict, string secret)
         {
             ILog kplog = LogManager.GetLogger(typeof(SignatureUtils));

             StringBuilder buffer = new StringBuilder();
             List<string> keys = (from kvp in dict select kvp.Key).Distinct().ToList();
             keys.Sort();
             kplog.Debug("Signature Params: ");
             foreach (KeyValuePair<string, string> key in dict)
             {
                 if (!REJECT.Contains(key.Key))
                 {
                     buffer.Append(key.Key)
                         .Append(key.Value);
                     kplog.Debug("  " + key + ": " + key.Key);
                 }
             }
             kplog.Debug("secret: '" + secret + "'");
             buffer.Append(secret);

             kplog.Debug("Signing String: " + buffer.ToString());

             MD5 md5 = MD5.Create();
             byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(buffer.ToString()));
             StringBuilder hexdigest = new StringBuilder();
             for (int i = 0; i < hash.Length; i++)
             {
                 hexdigest.Append(hash[i].ToString("x2"));
             }

             string sig = hexdigest.ToString();
             kplog.Debug("Signature: " + sig);
             return hexdigest.ToString();
         }
     }

}

