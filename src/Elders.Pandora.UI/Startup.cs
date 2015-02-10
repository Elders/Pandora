using Elders.Pandora.UI.App_Start;
using Elders.Pandora.UI.Security;
using Microsoft.Owin;
using Owin;
using System.IdentityModel.Tokens;
using Thinktecture.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(Elders.Pandora.UI.Startup))]

namespace Elders.Pandora.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None;
            app.UseIdentitiyServerSelfContainedToken(new SelfContainedTokenValidationOptions());
            app.UseWebApi(WebApiBuilder.Build());
        }
    }
}