using Microsoft.AspNet.Identity.EntityFramework;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraUserStore : UserStore<PandoraUser, PandoraRole, string, PandoraUserLogin, PandoraUserRole, PandoraUserClaim>
    {
        public PandoraUserStore(PandoraDbContext ctx) : base(ctx) { }
    }
}