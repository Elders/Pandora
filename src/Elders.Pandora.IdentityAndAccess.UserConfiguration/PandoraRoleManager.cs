using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraRoleManager : RoleManager<PandoraRole, string>
    {
        public PandoraRoleManager() : base(new PandoraRoleStore()) { }
    }

    public class PandoraRole : IdentityRole<string, PandoraUserRole> { }
    public class PandoraUserLogin : IdentityUserLogin<string> { }
    public class PandoraUserRole : IdentityUserRole<string> { }
    public class PandoraUserClaim : IdentityUserClaim<string> { }
}