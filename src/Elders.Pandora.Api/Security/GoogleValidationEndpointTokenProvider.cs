using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;

namespace Elders.Pandora.Api.Security
{
    public class GoogleValidationEndpointTokenProvider : AuthenticationTokenProvider
    {
        private readonly HttpClient _client;
        private readonly string _tokenValidationEndpoint;
        private readonly IdentityServerBearerTokenAuthenticationOptions _options;
        private readonly ILogger _logger;

        public GoogleValidationEndpointTokenProvider(IdentityServerBearerTokenAuthenticationOptions options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(this.GetType().FullName);

            if (string.IsNullOrWhiteSpace(options.Authority))
            {
                throw new Exception("Authority must be set to use validation endpoint.");
            }

            var baseAddress = options.Authority.EnsureTrailingSlash();
            baseAddress += "connect/accesstokenvalidation";
            _tokenValidationEndpoint = baseAddress;

            var handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException("Invalid certificate validator");
                }

                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            _client = new HttpClient(handler);
            _options = options;
        }

        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            if (_options.EnableValidationResultCache)
            {
                var cachedClaims = await _options.ValidationResultCache.GetAsync(context.Token);
                if (cachedClaims != null)
                {
                    SetAuthenticationTicket(context, cachedClaims);
                    return;
                }
            }

            var form = new Dictionary<string, string>
            {
                { "token", context.Token }
            };

            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync("https://www.googleapis.com/oauth2/v1/tokeninfo?id_token=" + context.Token);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.WriteInformation("Error returned from token validation endpoint: " + response.ReasonPhrase);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("Exception while contacting token validation endpoint: " + ex.ToString());
                return;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            var claims = new List<Claim>();

            foreach (var item in dictionary)
            {
                var values = item.Value as IEnumerable<object>;

                if (values == null)
                {
                    claims.Add(new Claim(item.Key, item.Value.ToString()));
                }
                else
                {
                    foreach (var value in values)
                    {
                        claims.Add(new Claim(item.Key, value.ToString()));
                    }
                }
            }

            if (_options.EnableValidationResultCache)
            {
                await _options.ValidationResultCache.AddAsync(context.Token, claims);
            }

            SetAuthenticationTicket(context, claims);
        }

        private void SetAuthenticationTicket(AuthenticationTokenReceiveContext context, IEnumerable<Claim> claims)
        {
            var id = new ClaimsIdentity(
                            claims,
                            _options.AuthenticationType,
                            _options.NameClaimType,
                            _options.RoleClaimType);

            context.SetTicket(new AuthenticationTicket(id, new AuthenticationProperties()));
        }
    }
}