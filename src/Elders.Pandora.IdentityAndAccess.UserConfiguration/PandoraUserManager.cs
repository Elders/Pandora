using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraUserManager : UserManager<PandoraUser, string>
    {
        public PandoraUserManager(PandoraUserStore store)
            : base(store)
        {
            UserLockoutEnabledByDefault = false;
            UserValidator = new UserValidator<PandoraUser>(this) { AllowOnlyAlphanumericUserNames = false };
            PasswordValidator = new PasswordValidator() { RequireDigit = true, RequiredLength = 6, RequireLowercase = true, RequireNonLetterOrDigit = false, RequireUppercase = true };
        }

        public async override Task<IList<Claim>> GetClaimsAsync(string userId)
        {
            //THIS IS A DAMN HACK CUZ I DONT WANT OT EXPLAIN AGAIN TO BRIAN WHY ITS NOT A GOOD IDEA TO SHOW THE SECURITY QUESTION!!!!!!!!
            var claims = await base.GetClaimsAsync(userId);
            //var user = this.Users.Where(x => x.Id == userId).FirstOrDefault();
            //if (user.SecurityQuestion != null)
            //    claims.Add(new Claim("question", user.SecurityQuestion));
            return claims;
        }
    }
}