using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Owin;

namespace Elders.Pandora.UI.Security
{
    public class RoClientClaimsTransformer : ClaimsTransformationOptions
    {
        public RoClientClaimsTransformer()
        {
            ClaimsTransformation = ClaimsTransformer;
        }

        private Task<ClaimsPrincipal> ClaimsTransformer(ClaimsPrincipal principal)
        {
            var emailClaim = principal.Claims.SingleOrDefault(x => x.Type == "email");

            if (emailClaim != null && !string.IsNullOrWhiteSpace(emailClaim.Value))
            {
                var adminUsers = ApplicationConfiguration.Get("super_admin_users").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (adminUsers.Contains(emailClaim.Value))
                {
                    var identity = principal.Identities.First();

                    if (identity.HasClaim(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && x.Value == "superAdmin"))
                        return Task.FromResult<ClaimsPrincipal>(principal);

                    identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "superAdmin"));
                }
            }

            return Task.FromResult<ClaimsPrincipal>(principal);
        }
    }
}