using Elders.Pandora.IdentityAndAccess.UserConfiguration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using Thinktecture.IdentityManager;
using Thinktecture.IdentityManager.AspNetIdentity;
using Thinktecture.IdentityManager.Configuration;
using Thinktecture.IdentityManager.Core.Logging;
using Thinktecture.IdentityManager.Logging;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

[assembly: OwinStartup("startup", typeof(Elders.Pandora.IdentityAndAccess.IdentityManager.Startup))]
namespace Elders.Pandora.IdentityAndAccess.IdentityManager
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();
            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            var options = new IdentityManagerOptions
            {
                DisableUserInterface = true,
                Factory = new IdentityManagerServiceFactory() { IdentityManagerService = new Registration<IIdentityManagerService>(CreateForWeb) },
                SecurityMode = SecurityMode.OAuth2,
                OAuth2Configuration = new OAuth2Configuration
                {
                    AuthorizationUrl = ConfigurationManager.AppSettings["AuthorizationUrl"],
                    Issuer = ConfigurationManager.AppSettings["Issuer"],
                    Audience = ConfigurationManager.AppSettings["Audiance"],
                    ClientId = "idmgr",
                    SigningCert = Cert.Load(ConfigurationManager.AppSettings["Thumbprint"]),
                    Scope = "idmgr"
                }
            };

            app.UseJwtBearerAuthentication(new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
            {
                AllowedAudiences = new[] { options.OAuth2Configuration.Audience },
                IssuerSecurityTokenProviders = new[]
                        {
                            new X509CertificateSecurityTokenProvider(
                                options.OAuth2Configuration.Issuer,
                                options.OAuth2Configuration.SigningCert)
                        },

            });

            app.UseResourceAuthorization(new MyManager());

            SignatureConversions.AddConversions(app);

            //app.UseIdentityManager(options);

            app.UseWebApi(WebApiConfig.Build());

            app.UseStageMarker(PipelineStage.MapHandler);
        }

        public static IIdentityManagerService CreateForWeb(IDependencyResolver asd)
        {
            var db = new PandoraDbContext("DefaultConnection");
            var store = new PandoraUserStore(db);
            var manager = new UserManager<PandoraUser, string>(store);
            manager.EmailService = new EmailService();
            var admin = manager.Users.Where(x => x.UserName == "admin@mm.com").SingleOrDefault();
            if (admin == null)
            {
                admin = new PandoraUser();
                admin.UserName = "admin@mm.com";
                admin.Email = "admin@mm.com";
                var role = new PandoraUserClaim() { UserId = admin.Id, ClaimType = ClaimTypes.Role, ClaimValue = "superAdmin" };
                admin.Claims.Add(role);
                manager.Create(admin, "123qwe!@#QWE");
            }
            var mgr = new AspNetIdentityManagerService<PandoraUser, string, PandoraRole, string>(manager, new RoleManager<PandoraRole, string>(new PandoraRoleStore()));

            return mgr;
        }
    }

    public class MyManager : ResourceAuthorizationManager
    {
        public override System.Threading.Tasks.Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            if (!context.Principal.Identity.IsAuthenticated)
            {
                return Nok();
            }
            var resource = context.Resource;
            var action = context.Action;

            //Custom Logic
            return Nok();
        }
    }

    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return incomingPrincipal;
            }

            //MarketVisionIntegration.CreateOrUpdateProfile(incomingPrincipal);
            //  //System.Console.WriteLine(date);
            //System.Web.HttpContext.Current.User.Identity.Name = incomingPrincipal.Claims

            //incomingPrincipal.Identities.First().AddClaim(
            //    new Claim("localclaim", "localvalue"));

            return incomingPrincipal;
        }
    }
}