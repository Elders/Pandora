using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;

namespace Elders.Pandora.Api.AuthenticationMiddleware
{
    public class GoogleJwtBearerMiddleware : JwtBearerMiddleware
    {
        public GoogleJwtBearerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IUrlEncoder encoder, JwtBearerOptions options)
            : base(next, loggerFactory, encoder, options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (Options.Events == null)
            {
                Options.Events = new JwtBearerEvents();
            }

            if (string.IsNullOrEmpty(Options.TokenValidationParameters.ValidAudience) && !string.IsNullOrEmpty(Options.Audience))
            {
                Options.TokenValidationParameters.ValidAudience = Options.Audience;
            }

            if (Options.ConfigurationManager == null)
            {
                if (Options.Configuration != null)
                {
                    Options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(Options.Configuration);
                }
                else if (!(string.IsNullOrEmpty(Options.MetadataAddress) && string.IsNullOrEmpty(Options.Authority)))
                {
                    if (string.IsNullOrEmpty(Options.MetadataAddress) && !string.IsNullOrEmpty(Options.Authority))
                    {
                        Options.MetadataAddress = Options.Authority;
                        if (!Options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            Options.MetadataAddress += "/";
                        }

                        Options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    if (Options.RequireHttpsMetadata && !Options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                    }

                    var httpClient = new HttpClient(Options.BackchannelHttpHandler ?? new HttpClientHandler());
                    httpClient.Timeout = Options.BackchannelTimeout;
                    httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB

                    Options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Options.MetadataAddress, new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(httpClient) { RequireHttps = Options.RequireHttpsMetadata });
                }
            }
        }

        protected override AuthenticationHandler<JwtBearerOptions> CreateHandler()
        {
            return new GoogleJwtBearerHandler();
        }

        class GoogleJwtBearerHandler : AuthenticationHandler<JwtBearerOptions>
        {
            private OpenIdConnectConfiguration _configuration;

            /// <summary>
            /// Searches the 'Authorization' header for a 'Bearer' token. If the 'Bearer' token is found, it is validated using <see cref="TokenValidationParameters"/> set in the options.
            /// </summary>
            /// <returns></returns>
            protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                string token = null;
                try
                {
                    // Give application opportunity to find from a different location, adjust, or reject token
                    var receivingTokenContext = new ReceivingTokenContext(Context, Options);

                    // event can set the token
                    await Options.Events.ReceivingToken(receivingTokenContext);
                    if (receivingTokenContext.HandledResponse)
                    {
                        return AuthenticateResult.Success(receivingTokenContext.AuthenticationTicket);
                    }
                    if (receivingTokenContext.Skipped)
                    {
                        return AuthenticateResult.Success(receivingTokenContext.AuthenticationTicket);
                    }

                    // If application retrieved token from somewhere else, use that.
                    token = receivingTokenContext.Token;

                    if (string.IsNullOrEmpty(token))
                    {
                        string authorization = Request.Headers["Authorization"];

                        // If no authorization header found, nothing to process further
                        if (string.IsNullOrEmpty(authorization))
                        {
                            return AuthenticateResult.Failed("No authorization header.");
                        }

                        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            token = authorization.Substring("Bearer ".Length).Trim();
                        }

                        // If no token found, no further work possible
                        if (string.IsNullOrEmpty(token))
                        {
                            return AuthenticateResult.Failed("Token is missing.");
                        }
                    }

                    // notify user token was received
                    var receivedTokenContext = new ReceivedTokenContext(Context, Options)
                    {
                        Token = token,
                    };

                    await Options.Events.ReceivedToken(receivedTokenContext);
                    if (receivedTokenContext.HandledResponse)
                    {
                        return AuthenticateResult.Success(receivedTokenContext.AuthenticationTicket);
                    }
                    if (receivedTokenContext.Skipped)
                    {
                        return AuthenticateResult.Success(receivedTokenContext.AuthenticationTicket);
                    }

                    if (_configuration == null && Options.ConfigurationManager != null)
                    {
                        _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                    }

                    var validationParameters = Options.TokenValidationParameters.Clone();
                    if (_configuration != null)
                    {
                        if (validationParameters.ValidIssuer == null && !string.IsNullOrEmpty(_configuration.Issuer))
                        {
                            validationParameters.ValidIssuer = _configuration.Issuer;
                        }
                        else
                        {
                            var issuers = new[] { _configuration.Issuer };
                            validationParameters.ValidIssuers = (validationParameters.ValidIssuers == null ? issuers : validationParameters.ValidIssuers.Concat(issuers));
                        }

                        validationParameters.IssuerSigningKeys = (validationParameters.IssuerSigningKeys == null ? _configuration.SigningKeys : validationParameters.IssuerSigningKeys.Concat(_configuration.SigningKeys));
                    }

                    List<Exception> validationFailures = null;
                    SecurityToken validatedToken;

                    JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();

                    ClaimsPrincipal principal = null;
                    try
                    {
                        principal = jwt.ValidateToken(token, validationParameters, out validatedToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation("Failed to validate the token: " + token, ex);

                        // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
                        if (Options.RefreshOnIssuerKeyNotFound && ex.GetType().Equals(typeof(SecurityTokenSignatureKeyNotFoundException)))
                        {
                            Options.ConfigurationManager.RequestRefresh();
                        }

                        if (validationFailures == null)
                        {
                            validationFailures = new List<Exception>(1);
                        }
                        validationFailures.Add(ex);
                        //continue;
                    }

                    Logger.LogInformation("Successfully validated the token");

                    var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme);
                    var validatedTokenContext = new ValidatedTokenContext(Context, Options)
                    {
                        AuthenticationTicket = ticket
                    };

                    await Options.Events.ValidatedToken(validatedTokenContext);
                    if (validatedTokenContext.HandledResponse)
                    {
                        return AuthenticateResult.Success(validatedTokenContext.AuthenticationTicket);
                    }
                    if (validatedTokenContext.Skipped)
                    {
                        return AuthenticateResult.Success(validatedTokenContext.AuthenticationTicket);
                    }

                    return AuthenticateResult.Success(ticket);

                    if (validationFailures != null)
                    {
                        var authenticationFailedContext = new AuthenticationFailedContext(Context, Options)
                        {
                            Exception = (validationFailures.Count == 1) ? validationFailures[0] : new AggregateException(validationFailures)
                        };

                        await Options.Events.AuthenticationFailed(authenticationFailedContext);
                        if (authenticationFailedContext.HandledResponse)
                        {
                            return AuthenticateResult.Success(authenticationFailedContext.AuthenticationTicket);
                        }
                        if (authenticationFailedContext.Skipped)
                        {
                            return AuthenticateResult.Success(authenticationFailedContext.AuthenticationTicket);
                        }

                        return AuthenticateResult.Failed(authenticationFailedContext.Exception);
                    }

                    return AuthenticateResult.Failed("No SecurityTokenValidator available for token: " + token ?? "[null]");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Exception occurred while processing message", ex);

                    var authenticationFailedContext = new AuthenticationFailedContext(Context, Options)
                    {
                        Exception = ex
                    };

                    await Options.Events.AuthenticationFailed(authenticationFailedContext);
                    if (authenticationFailedContext.HandledResponse)
                    {
                        return AuthenticateResult.Success(authenticationFailedContext.AuthenticationTicket);
                    }
                    if (authenticationFailedContext.Skipped)
                    {
                        return AuthenticateResult.Success(authenticationFailedContext.AuthenticationTicket);
                    }

                    throw;
                }
            }

            protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
            {
                var eventContext = new JwtBearerChallengeContext(Context, Options);
                await Options.Events.Challenge(eventContext);
                if (eventContext.HandledResponse)
                {
                    return true;
                }
                if (eventContext.Skipped)
                {
                    return false;
                }

                Response.StatusCode = 401;
                Response.Headers.Append(HeaderNames.WWWAuthenticate, Options.Challenge);

                return false;
            }

            protected override Task HandleSignOutAsync(SignOutContext context)
            {
                throw new NotSupportedException();
            }

            protected override Task HandleSignInAsync(SignInContext context)
            {
                throw new NotSupportedException();
            }
        }
    }
}