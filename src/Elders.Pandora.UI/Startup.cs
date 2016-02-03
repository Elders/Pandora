using System.Collections.Generic;
using System.IO;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;
using Thinktecture.IdentityModel.Tokens;
using Elders.Pandora.UI.Security;

[assembly: OwinStartup(typeof(Elders.Pandora.UI.Startup))]
namespace Elders.Pandora.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ApplicationConfiguration.SetContext("Elders.Pandora.Api");
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;

            var certBytes = getCertBytes();

            var certificates = certBytes.Select(x => new X509Certificate2(x)).ToList();
            var tokenProviders = certificates.Select(x => new X509CertificateSecurityTokenProvider(ApplicationConfiguration.Get("issuer"), x)).ToList();

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = GetTvp(certificates),
                AllowedAudiences = new[] { ApplicationConfiguration.Get("audience") },
                IssuerSecurityTokenProviders = tokenProviders
            });

            app.UseResourceAuthorization(new ResourceRoleManager());
            app.UseClaimsTransformation(new RoClientClaimsTransformer());
        }

        private static TokenValidationParameters GetTvp(List<X509Certificate2> certs)
        {
            TokenValidationParameters tvp = new TokenValidationParameters();
            tvp.ValidAudience = ApplicationConfiguration.Get("audience");
            tvp.ValidIssuer = ApplicationConfiguration.Get("issuer");
            tvp.ValidateIssuer = true;
            tvp.ValidateAudience = true;
            tvp.ValidateIssuerSigningKey = true;
            tvp.ValidateLifetime = true;

            var tokens = new List<SecurityToken>();
            var keys = new List<X509SecurityKey>();

            foreach (var certificate in certs)
            {
                X509SecurityToken certToken = new X509SecurityToken(certificate);

                var key = new X509SecurityKey(certificate);

                tokens.Add(certToken);
                keys.Add(key);
            }
            tvp.IssuerSigningKeyResolver = (a, b, c, d) =>
            {
                var key = keys.Where(x => b.ToString().ToLower().Contains(x.Certificate.Thumbprint.ToLower())).First();
                return key;
            };
            tvp.IssuerSigningTokens = tokens;
            tvp.IssuerSigningKeys = keys;

            return tvp;
        }

        // Used for string parsing the Certificates from Google
        private const string beginCert = "-----BEGIN CERTIFICATE-----\\n";
        private const string endCert = "\\n-----END CERTIFICATE-----\\n";
        public static byte[][] getCertBytes()
        {
            // The request will be made to the authentication server.
            WebRequest request = WebRequest.Create(
                "https://www.googleapis.com/oauth2/v1/certs"
            );

            StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream());

            string responseFromServer = reader.ReadToEnd();

            string[] split = responseFromServer.Split(':');

            // There are two certificates returned from Google
            byte[][] certBytes = new byte[2][];
            int index = 0;
            UTF8Encoding utf8 = new UTF8Encoding();
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].IndexOf(beginCert) > 0)
                {
                    int startSub = split[i].IndexOf(beginCert);
                    int endSub = split[i].IndexOf(endCert) + endCert.Length;
                    certBytes[index] = utf8.GetBytes(split[i].Substring(startSub, endSub).Replace("\\n", "\n"));
                    index++;
                }
            }
            return certBytes;
        }
    }
}