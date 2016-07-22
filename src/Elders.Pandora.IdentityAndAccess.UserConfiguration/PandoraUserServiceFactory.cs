using System.Data.Entity;
using Thinktecture.IdentityServer.AspNetIdentity;
using Thinktecture.IdentityServer.Core.Services;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraUserServiceFactory
    {
        static PandoraUserServiceFactory()
        {
            Database.SetInitializer<PandoraDbContext>(null);
        }

        public static IUserService Factory()
        {
            var db = new PandoraDbContext("DefaultConnection");
            var store = new PandoraUserStore(db);
            var mgr = new PandoraUserManager(store);
            var userSvc = new AspNetIdentityUserService<PandoraUser, string>(mgr);
            return userSvc;
        }
    }
}