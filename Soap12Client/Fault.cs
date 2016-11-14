using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace APIBanking
{

    public class Fault : ApplicationException
    {
        public readonly String Code;
        public readonly String SubCode;
        public readonly String MessageInserts;
        public readonly String responseText;

        public Fault(MessageSecurityException e) : base(e.Message, e)
        {
            WebException we = (WebException)e.GetBaseException();
            WebResponse response = we.Response;

            this.Code = "qc:E401"; // 401 : unauthorised

            if (response.ContentType == "application/xml")
            {
                XmlDocument document = new XmlDocument();
                using (StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                {
                    using (XmlReader xmlReader = XmlReader.Create(streamReader))
                    {
                        document.Load(xmlReader);
                        XmlNodeList moreInfo = document.GetElementsByTagName("moreInformation");
                        if (moreInfo.Count > 0)
                        {
                            MessageInserts = moreInfo[0].InnerText;
                        }
                        if (MessageInserts == "Invalid client id or secret.")
                        {
                            SubCode = "qc:dp100";
                        }
                        else if (MessageInserts == "Client id not registered.")
                        {
                            SubCode = "qc:dp101";
                        }
                        else if (MessageInserts == "Not Registered to Plan")
                        {
                            SubCode = "qc:dp102";
                        }
                        else if (MessageInserts == "Authentication Failure, Unable to Validate Credentials")
                        {
                            SubCode = "qc:dp103";
                        }
                        else if (MessageInserts == "Rate Limit - Rate Limit Exceeded")
                        {
                            SubCode = "qc:dp104";
                        }
                        else if (MessageInserts.StartsWith("Internal Server Error"))
                        {
                            SubCode = "qc:dp105";
                        }
                        else if (MessageInserts == "Authentication Failure, Unable to Validate Credentials")
                        {
                            SubCode = "qc:dp106";
                        }
                        else if (MessageInserts == "Client id missing.")
                        {
                            SubCode = "qc:dp107";
                        }
                        responseText = document.OuterXml;
                    }
                }
            }
            else if (response.ContentType.StartsWith("text/html"))
            {
                // this comes when the user/password fails to authenticate
                SubCode = "qc:ldap401";
            }
            else
            {
                using (StreamReader reader = new System.IO.StreamReader(we.Response.GetResponseStream(), ASCIIEncoding.ASCII))
                {
                    MessageInserts = reader.ReadToEnd();
                }
            }

        }

        public Fault(FaultException e) : base(e.Reason.ToString(), e)
        {
            this.Code = "qc:E502"; // 502 : by default a bad gateway

            if (e.Code.SubCode != null)
            {
                Code = formatFaultCode(e.Code.SubCode);
            }

            if (e.Code.SubCode.SubCode != null)
            {
                SubCode = formatFaultCode(e.Code.SubCode.SubCode);
            }

            MessageFault msgFault = e.CreateMessageFault();
            if (msgFault.HasDetail)
            {
                MessageInserts = msgFault.GetReaderAtDetailContents().ReadOuterXml();
                XmlReader reader = msgFault.GetReaderAtDetailContents();
                if (reader.ReadToFollowing("messageInserts"))
                {
                    MessageInserts = reader.ReadOuterXml();
                }
            }
        }

        public Fault(Exception e) : base(e.Message, e)
        {
            // no reply
            if (e is TimeoutException)
            {
                Code = "qc:E504";
            }
            // transport failure
            else if (e is CommunicationException)
            {
                Code = "qc:E503";
            }
            // generic failure
            else
            {
                Code = "qc:E500";
            }
        }


        public override String ToString()
        {
            String prettyPrint;

            prettyPrint = "Fault: Code " + this.Code +
                          " SubCode: " + this.SubCode +
                          " ReasonText: " + this.Message +
                          " messageInserts: " + this.MessageInserts;

            return prettyPrint;
        }

        private static String formatFaultCode(FaultCode faultCode)
        {
            switch (faultCode.Namespace)
            {
                case "http://www.quantiguous.com/services":
                    return "ns:" + faultCode.Name;
                default:
                    return faultCode.Namespace + ":" + faultCode.Name;
            }
        }
    }
}