using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Thinktecture.IdentityModel.Owin;

namespace Elders.Pandora.UI.Security
{
    public static class SelfContainedTokenValidationExtensions
    {
        public static IAppBuilder UseIdentitiyServerSelfContainedToken(this IAppBuilder app, SelfContainedTokenValidationOptions options)
        {
            Byte[][] certBytes = getCertBytes();

            var certificates = certBytes.Select(x => new X509Certificate2(x)).ToList();
            var tokenProviders = certificates.Select(x => new X509CertificateSecurityTokenProvider("accounts.google.com", x)).ToList();
            tokenProviders.Add(new X509CertificateSecurityTokenProvider(options.IssuerName, options.SigningCertificate));
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = GetTvp(certificates),
                AllowedAudiences = new[] { options.Audiance },
                IssuerSecurityTokenProviders = tokenProviders
            });
            app.UseResourceAuthorization(new ResourceRoleManager());

            app.UseClaimsTransformation(new RoClientClaimsTransformer());
            return app;
        }

        private static TokenValidationParameters GetTvp(List<X509Certificate2> certs)
        {
            TokenValidationParameters tvp = new TokenValidationParameters();
            tvp.ValidIssuer = "accounts.google.com";
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

            String[] split = responseFromServer.Split(':');

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
    public class ProfileAuthorizeAttribute : AuthorizeAttribute
    {
        public string Profile { get; private set; }

        public ProfileAuthorizeAttribute(string profile)
        {
            Profile = profile;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var ctx = HttpContext.Current.GetOwinContext();
            var claim = ctx.Request.Context.Authentication.User.Claims.Where(x => x.Type == "profile").SingleOrDefault();
            if (claim == null || String.Compare(claim.Value, Profile, true, CultureInfo.InvariantCulture) != 0)
                return false;

            return base.IsAuthorized(actionContext);
        }
    }
    public class RoClientClaimsTransformer : ClaimsTransformationOptions
    {
        public RoClientClaimsTransformer()
        {
            ClaimsTransformation = ClaimsTransformer;
        }

        private Task<ClaimsPrincipal> ClaimsTransformer(ClaimsPrincipal principal)
        {
            var emailClaim = principal.Claims.SingleOrDefault(x => x.Type == "email");

            if (emailClaim != null && !string.IsNullOrWhiteSpace(emailClaim.Value))
            {
                var adminUsers = ConfigurationManager.AppSettings["SuperAdminUsers"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (adminUsers.Contains(emailClaim.Value))
                {
                    principal.Identities.First().AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "superAdmin"));
                }
            }

            return Task.FromResult<ClaimsPrincipal>(principal);
        }
        private static bool IsJson(string str)
        {
            if (str.Trim().StartsWith("{") || str.Trim().StartsWith("["))
            {
                try
                {
                    var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(str);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return false;
        }
    }
    public class UserInfoEndpointResponse
    {
        public string Profile { get; set; }
    }
}