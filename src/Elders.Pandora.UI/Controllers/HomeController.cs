using System.Web.Mvc;
using Thinktecture.IdentityModel.Oidc;

namespace Elders.Pandora.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            OidcClient.SignOut();
            return Redirect("/Login");
        }
    }
}