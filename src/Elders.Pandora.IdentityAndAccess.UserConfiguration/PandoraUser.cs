using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraUser : IdentityUser<string, PandoraUserLogin, PandoraUserRole, PandoraUserClaim>
    {
        public PandoraUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}