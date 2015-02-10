using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Clients;
using Thinktecture.IdentityModel.Web;
using System.Web;
using System.Text;
using Thinktecture.IdentityModel.SystemWeb;

namespace Thinktecture.IdentityModel.Oidc
{
    public class OidcClient
    {
        public static Uri AuthorizeEndpoint;
        public static Uri GetAuthorizeUrl(Uri authorizeEndpoint, Uri redirectUri, string clientId, string scopes, string state, string responseType = "code")
        {
            AuthorizeEndpoint = authorizeEndpoint;
            var queryString = string.Format("?client_id={0}&scope={1}&redirect_uri={2}&state={3}&response_type={4}",
                clientId,
                scopes,
                redirectUri,
                state,
                responseType);

            return new Uri(authorizeEndpoint.AbsoluteUri + queryString);
        }

        public static OidcAuthorizeResponse HandleAuthorizeResponse(NameValueCollection query)
        {
            var response = new OidcAuthorizeResponse
            {
                Error = query["error"],
                Code = query["code"],
                State = query["state"]
            };

            response.IsError = !string.IsNullOrWhiteSpace(response.Error);
            return response;
        }

        public static OidcTokenResponse CallTokenEndpoint(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri.AbsoluteUri }
                };

            var response = client.PostAsync("", new FormUrlEncodedContent(parameter)).Result;
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return json.ToObject<OidcTokenResponse>();
        }

        public async static Task<OidcTokenResponse> CallTokenEndpointAsync(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri.AbsoluteUri }
                };

            var response = await client.PostAsync("", new FormUrlEncodedContent(parameter));
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            return json.ToObject<OidcTokenResponse>();
        }

        public static OidcTokenResponse RefreshAccessToken(Uri tokenEndpoint, string clientId, string clientSecret, string refreshToken)
        {
            var client = new OAuth2Client(
                tokenEndpoint,
                clientId,
                clientSecret);

            var response = client.RequestAccessTokenRefreshToken(refreshToken);

            return new OidcTokenResponse
            {
                AccessToken = response.AccessToken,
                ExpiresIn = response.ExpiresIn,
                TokenType = response.TokenType,
                RefreshToken = refreshToken
            };
        }

        public static IEnumerable<Claim> ValidateIdentityToken(string token, string issuer, string audience, X509Certificate2 signingCertificate)
        {
            var idToken = new JwtSecurityToken(token);
            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningToken = new X509SecurityToken(signingCertificate)
            };

            SecurityToken securityToken = null;
            return handler.ValidateToken(token, parameters, out securityToken).Claims;
        }

        public async static Task<IEnumerable<Claim>> GetUserInfoClaimsAsync(Uri userInfoEndpoint, string accessToken)
        {
            var client = new HttpClient
            {
                BaseAddress = userInfoEndpoint
            };

            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("");
            response.EnsureSuccessStatusCode();

            var dictionary = await response.Content.ReadAsAsync<Dictionary<string, object>>();

            var claims = new List<Claim>();
            foreach (var pair in dictionary)
            {

                if (pair.Value.ToString().Contains(',') && !IsJson(pair.Value.ToString()))
                {
                    foreach (var item in pair.Value.ToString().Split(','))
                    {
                        claims.Add(new Claim(pair.Key, item));
                    }
                }
                else
                    claims.Add(new Claim(pair.Key, pair.Value.ToString()));

            }
            if (!dictionary.ContainsKey("name"))
            {
                var token = new System.IdentityModel.Tokens.JwtSecurityToken(accessToken);
                var name = token.Claims.Where(x => x.Type == "name").FirstOrDefault() ?? new Claim("name", "");
                claims.Add(name);
            }
            return claims;
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
        public static IEnumerable<Claim> GetUserInfoClaims(Uri userInfoEndpoint, string accessToken)
        {
            var client = new HttpClient
            {
                BaseAddress = userInfoEndpoint
            };

            client.SetBearerToken(accessToken);

            var response = client.GetAsync("").Result;
            response.EnsureSuccessStatusCode();

            var dictionary = response.Content.ReadAsAsync<Dictionary<string, string>>().Result;

            var claims = new List<Claim>();
            foreach (var pair in dictionary)
            {
                if (pair.Value.Contains(',') && !IsJson(pair.Value))
                {
                    foreach (var item in pair.Value.Split(','))
                    {
                        claims.Add(new Claim(pair.Key, item));
                    }
                }
                else
                {
                    claims.Add(new Claim(pair.Key, pair.Value));
                }
            }

            return claims;
        }

        public static void SignOut()
        {
            SelfProtectedCookie.Delete("oidcstate");
            var exipire = new SelfProtectedCookie(ProtectionMode.MachineKey, HttpContext.Current.Request.IsSecureConnection);

            SelfProtectedCookie.Delete("tryGoOp");
            var cks = HttpContext.Current.Request.Cookies.Keys.Cast<string>().ToList();
            foreach (string item in cks)
            {
                if (HttpContext.Current.Request.Cookies[item] != null)
                {
                    var c = new HttpCookie(item);
                    c.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(c);
                }
            }

            if (HttpContext.Current.Session != null)
            {
                foreach (string name in HttpContext.Current.Session.Keys.Cast<string>().ToList())
                {
                    HttpContext.Current.Session[name] = null;
                }
            }
        }
    }
}
