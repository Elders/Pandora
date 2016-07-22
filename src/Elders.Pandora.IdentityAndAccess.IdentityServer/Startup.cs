using Elders.Pandora.IdentityAndAccess.UserConfiguration;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Owin;
using System.Collections.Generic;
using System.Configuration;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Logging.LogProviders;

[assembly: OwinStartup("startup", typeof(Elders.Pandora.IdentityAndAccess.IdentityServer.Startup))]
namespace Elders.Pandora.IdentityAndAccess.IdentityServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();
            LogProvider.SetCurrentLogProvider(new Log4NetLogProvider());

            var factory = PandoraIdentityServerServiceFactory.Create(Clients.Get(), Scopes.Get());

            var opts = new IdentityServerOptions
            {
                Factory = factory,
                IssuerUri = ConfigurationManager.AppSettings["Issuer"],
                SiteName = ConfigurationManager.AppSettings["SiteName"],
                SigningCertificate = Cert.Load(ConfigurationManager.AppSettings["Thumbprint"]),
                PublicOrigin = ConfigurationManager.AppSettings["PublicHostName"],
                AuthenticationOptions = new AuthenticationOptions()
                {
                    EnableSignOutPrompt = false,
                    IdentityProviders = ConfigureAdditionalIdentityProviders
                },
            };

            app.UseIdentityServer(opts);
        }

        public static void ConfigureAdditionalIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                Caption = "Google",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "360040534003-0vf9mn4f2lp8ldokcpoieiraa8hdruil.apps.googleusercontent.com",
                ClientSecret = "RvDo_jRH9T6HjfrmJ8-ccJPS",
            };

            foreach (var scope in GoogleScopes())
            {
                google.Scope.Add(scope);
            }


            app.UseGoogleAuthentication(google);
        }

        public static List<string> GoogleScopes()
        {
            return new List<string>()
                {
                    Constants.StandardScopes.OpenId,
                    Constants.StandardScopes.Profile,
                    Constants.StandardScopes.Email,
                    //Constants.StandardScopes.Address,
                    //Constants.StandardScopes.OfflineAccess,
                    //"read",
                    //"write",
                    //"access",
                    //"name"
                };
        }
    }
}