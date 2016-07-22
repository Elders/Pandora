using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Code Flow Client",
                    Enabled = true,
                    ClientId = ConfigurationManager.AppSettings["WebClientId"],
                    ClientSecrets =  new List<ClientSecret>(){ new ClientSecret(ConfigurationManager.AppSettings["WebClientSecret"].Sha256()) },
                    Flow = Flows.AuthorizationCode,

                    RequireConsent = false,
                    AllowRememberConsent = false,

                    ClientUri = "https://pandora.local.devsmm.com",

                    RedirectUris = ConfigurationManager.AppSettings["WebClientRedirectUri"].Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    ScopeRestrictions = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.Address,
                        Constants.StandardScopes.OfflineAccess,
                        "read",
                        "write",
                        "access",
                        "name"
                    },

                    AccessTokenType = AccessTokenType.Jwt,

                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    IdentityTokenLifetime = 3600,
                    AccessTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 120
                },
                new Client
                {
                    ClientName = "Resource Owner Flow Client",
                    Enabled = true,
                    ClientId = ConfigurationManager.AppSettings["MobileClientId"],
                    ClientSecrets = new List<ClientSecret>(){ new ClientSecret(ConfigurationManager.AppSettings["MobileClientSecret"].Sha256()) },
                    Flow = Flows.ResourceOwner,

                    ScopeRestrictions = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        Constants.StandardScopes.Email,
                        Constants.StandardScopes.OfflineAccess,
                        "read",
                        "write",
                        "access",
                        "name",
                    },

                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AccessTokenType = AccessTokenType.Jwt,
                    AccessTokenLifetime = 2592000,
                }
            };
        }
    }
}