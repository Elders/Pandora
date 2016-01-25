using System.Security.Claims;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Oidc;

namespace Elders.Pandora.UI.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Oauth()
        {
            if (ClaimsPrincipal.Current != null && ClaimsPrincipal.Current.Identity != null && ClaimsPrincipal.Current.Identity.IsAuthenticated)
                return Redirect("/Home");

            var loginUrl = OpenIdConnectAuthenticationModule.LoginUrl();
            return Redirect(loginUrl);
        }
    }
}