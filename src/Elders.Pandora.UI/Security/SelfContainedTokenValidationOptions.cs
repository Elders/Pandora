using Microsoft.Owin.Security;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Elders.Pandora.UI.Security
{
    public class SelfContainedTokenValidationOptions : AuthenticationOptions
    {
        public SelfContainedTokenValidationOptions()
            : base("IdSrvSelfContainedToken")
        {
            Audiance = ConfigurationManager.AppSettings["Audiance"];
            IssuerName = ConfigurationManager.AppSettings["Issuer"];
            Thumbprint = ConfigurationManager.AppSettings["Thumbprint"];

            var store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2 mvCert = null;
            foreach (var cert in store.Certificates)
            {
                if (System.String.Compare(cert.Thumbprint, Thumbprint, true) == 0)
                {
                    mvCert = cert;
                    break;
                }
            }
            SigningCertificate = mvCert;

        }

        public string IssuerName { get; set; }
        public string Audiance { get; set; }
        public string Thumbprint { get; set; }
        public X509Certificate2 SigningCertificate { get; set; }
    }
}