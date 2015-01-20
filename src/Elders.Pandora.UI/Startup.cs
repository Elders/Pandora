using Elders.Pandora.UI.App_Start;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Elders.Pandora.UI.Startup))]

namespace Elders.Pandora.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebApi(WebApiBuilder.Build());
        }
    }
}