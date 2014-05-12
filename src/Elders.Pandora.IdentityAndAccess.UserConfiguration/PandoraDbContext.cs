using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraDbContext : IdentityDbContext<PandoraUser, PandoraRole, string, PandoraUserLogin, PandoraUserRole, PandoraUserClaim>
    {
        public PandoraDbContext(string connString)
            : base(connString)
        {
            var initializer = new CreateDatabaseIfNotExists<PandoraDbContext>();

            Database.SetInitializer(initializer);

            Database.Initialize(false);

            var admin = this.Users.Where(x => x.UserName == "admin@mm.com").SingleOrDefault();
            if (admin == null)
            {
                admin = new PandoraUser();
                admin.UserName = "admin@mm.com";
                admin.Email = "admin@mm.com";
                var role = new PandoraUserClaim() { UserId = admin.Id, ClaimType = ClaimTypes.Role, ClaimValue = "superAdmin" };
                admin.Claims.Add(role);
                admin.PasswordHash = new PasswordHasher().HashPassword("123qwe!@#QWE");
                admin.EmailConfirmed = true;

                this.Users.Add(admin);
                this.SaveChanges();
            }
        }
    }
}