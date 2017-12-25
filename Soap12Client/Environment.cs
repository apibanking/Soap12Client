using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;

namespace APIBanking
{
    public interface Environment
    {
        Boolean needsHTTPBasicAuth();
        String getUser();
        String getPassword();
        String getClientId();
        String getClientSecret();
        EndpointAddress getEndpointAddress(String serviceName);
        Hashtable getHeaders();
        System.Net.SecurityProtocolType getSecurityProtocol();
        Uri getProxyAddress();
        X509Certificate2 getClientCertificate();
        Boolean needsClientCertificate();

    }

    public abstract class _Environment : Environment
    {
        protected String user;
        protected String password;
        protected String client_id;
        protected String client_secret;
        protected Uri proxyAddress;
        protected String pkcs12FilePath;
        protected String pkcs12Password;

        protected _Environment(String user, String password, String client_id, String client_secret, String pkcs12FilePath = null, String pkcs12Password = null, Uri proxyAddress = null)
        {
            this.user = user;
            this.password = password;
            this.client_id = client_id;
            this.client_secret = client_secret;
            this.proxyAddress = proxyAddress;
            this.pkcs12FilePath = pkcs12FilePath;
            this.pkcs12Password = pkcs12Password;
        }

        public Uri getProxyAddress()
        {
            return this.proxyAddress;
        }
        public Boolean needsHTTPBasicAuth()
        {
            return true;
        }
        public String getUser()
        {
            return this.user;
        }
        public String getPassword()
        {
            return this.password;
        }
        public String getClientId()
        {
            return this.client_id;
        }
        public String getClientSecret()
        {
            return this.client_secret;
        }
        public Boolean needsClientCertificate()
        {
            return (this.pkcs12FilePath != null ) ? true : false;
        }

        public System.Net.SecurityProtocolType getSecurityProtocol()
        {
            return System.Net.SecurityProtocolType.Tls;
        }
        public X509Certificate2 getClientCertificate()
        {
            return new X509Certificate2(this.pkcs12FilePath, this.pkcs12Password);
        }

        public abstract EndpointAddress getEndpointAddress(string serviceName);
        public abstract Hashtable getHeaders();
    }

    namespace Environments.YBL
    {
        public abstract class _YBLEnvironment : _Environment
        {

  
            protected _YBLEnvironment(String user, String password, String client_id, String client_secret, String pkcs12FilePath = null, String pkcs12Password = null, Uri proxyAddress = null) :
                base(user, password, client_id, client_secret, pkcs12FilePath, pkcs12Password, proxyAddress)
            {
            }
            protected _YBLEnvironment(String user, String password, String client_id, String client_secret, Uri proxyAddress = null)
                : base(user, password, client_id, client_secret, null, null, proxyAddress)
            {
            }
            public override Hashtable getHeaders()
            {
                Hashtable headers = new Hashtable();
                headers.Add("X-IBM-Client-Id", client_id);
                headers.Add("X-IBM-Client-Secret", client_secret);
                if (needsClientCertificate())
                {
                    headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password)));
                }
                return headers;
            }
        }
        public class UAT : _YBLEnvironment
        {
            

            public UAT(String user, String password, String client_id, String client_secret, String pkcs12FilePath = null, String pkcs12Password = null, Uri proxyAddress = null) :
                base(user, password, client_id, client_secret, pkcs12FilePath, pkcs12Password, proxyAddress)
            {
            }
            public UAT(String user, String password, String client_id, String client_secret, Uri proxyAddress = null)
                : base(user, password, client_id, client_secret, null, null, proxyAddress)
            {
            }

            public override EndpointAddress getEndpointAddress(String serviceName)
            {
                String baseURL = "https://uatsky.yesbank.in";
                if (needsClientCertificate())
                {
                    baseURL += ":444";
                }
                if (serviceName == "fundsTransferByCustomerService")
                {
                    return new EndpointAddress(baseURL + "/app/uat/fundsTransferByCustomerServiceHttpService");
                }
                else
                if (serviceName == "fundsTransferByCustomerService2")
                {
                  if (needsClientCertificate())
                  {
                    return new EndpointAddress(baseURL + "/app/uat/ssl/fundsTransferByCustomerSevice2");
                  else
                    return new EndpointAddress(baseURL + "/app/uat/fundsTransferByCustomerService2");
                  }
                }
                else
                if (serviceName == "InwardRemittanceByPartnerService")
                {
                    return new EndpointAddress(baseURL + "/app/uat/InwardRemittanceByPartnerServiceHttpService");
                }
                else
                if (serviceName == "DomesticRemittanceByPartnerService")
                {
                    return new EndpointAddress(baseURL + "/app/uat/DomesticRemittanceService");
                }
                else
                if (serviceName == "IMTService")
                {
                    return new EndpointAddress(baseURL + "/app/uat/IMTService");
                }
                else
                {
                    return new EndpointAddress(baseURL + "/app/uat/ssl/" + serviceName);
                }
            }
        }
        public class PRD : _YBLEnvironment
        {

            public PRD(String user, String password, String client_id, String client_secret, String pkcs12FilePath = null, String pkcs12Password = null, Uri proxyAddress = null) :
                base(user, password, client_id, client_secret, pkcs12FilePath, pkcs12Password, proxyAddress)
            {
            }
            public PRD(String user, String password, String client_id, String client_secret, Uri proxyAddress = null)
                : base(user, password, client_id, client_secret, null, null, proxyAddress)
            {
            }

            public override EndpointAddress getEndpointAddress(String serviceName)
            {
                String baseURL = "https://sky.yesbank.in";
                if (needsClientCertificate())
                {
                    baseURL += ":444";
                }
                if (serviceName == "fundsTransferByCustomerService")
                {
                    return new EndpointAddress(baseURL + "/app/live/fundsTransferByCustomerServiceHttpService");
                }
                else
                if (serviceName == "fundsTransferByCustomerService2")
                {
                    return new EndpointAddress(baseURL + "/app/live/fundsTransferByCustomerService2");
                }
                else
                if (serviceName == "InwardRemittanceByPartnerService")
                {
                    return new EndpointAddress(baseURL + "/app/live/InwardRemittanceByPartnerServiceHttpService");
                }
                else
                if (serviceName == "DomesticRemittanceByPartnerService")
                {
                    return new EndpointAddress(baseURL + "/app/live/DomesticRemittanceService");
                }
                else
                if (serviceName == "IMTService")
                {
                    return new EndpointAddress(baseURL + "/app/live/IMTService");
                }
                else
                {
                    return new EndpointAddress(baseURL + "/app/live/ssl/" + serviceName);
                }
            }
        }

    }

}
