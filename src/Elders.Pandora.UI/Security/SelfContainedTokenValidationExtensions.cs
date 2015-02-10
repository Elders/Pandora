using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
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
            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AllowedAudiences = new[] { options.Audiance },
                IssuerSecurityTokenProviders = new[]
                        {
                            new X509CertificateSecurityTokenProvider(
                                options.IssuerName,
                                options.SigningCertificate)
                        },

            });
            app.UseResourceAuthorization(new ResourceRoleManager());

            app.UseClaimsTransformation(new RoClientClaimsTransformer());
            return app;
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

            if (principal.HasClaim("scope", "email") && principal.HasClaim(x => x.Type == "email") == false)
            {
                //Open Id Spec

                var config = Thinktecture.IdentityModel.Oidc.OidcClientConfigurationSection.Instance;

                var client = new RestSharp.RestClient(config.Endpoints.UserInfo);
                var request = new RestSharp.RestRequest(RestSharp.Method.GET);

                request.AddHeader("Authorization", HttpContext.Current.Request.Headers["Authorization"]);
                var response = client.Execute<Dictionary<string, string>>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Data != null)
                {
                    var claims = new List<Claim>();
                    var claimsTypes = principal.Claims.Select(x => x.Type);
                    foreach (var pair in response.Data)
                    {
                        if (claimsTypes.Contains(pair.Key))
                            continue;
                        if (pair.Value.Contains(',') && !IsJson(pair.Value))
                        {
                            foreach (var item in pair.Value.Split(','))
                            {
                                claims.Add(new Claim(pair.Key, item));
                            }
                        }
                        else
                            claims.Add(new Claim(pair.Key, pair.Value));
                    }



                    principal.Identities.First().AddClaims(claims);
                    //var identity = new ClaimsIdentity(claims);
                    //principal.AddIdentity(identity);
                }
                return Task.FromResult<ClaimsPrincipal>(principal);
            }
            else
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