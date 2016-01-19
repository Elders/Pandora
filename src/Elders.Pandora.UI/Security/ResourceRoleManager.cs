using System.Security.Claims;
using System.Linq;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;
using System.Collections.Generic;
using Thinktecture.IdentityModel.Owin;

namespace Elders.Pandora.UI.Security
{
    public class ResourceRoleManager : ResourceAuthorizationManager
    {
        public async override System.Threading.Tasks.Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            if (!context.Principal.Identity.IsAuthenticated)
            {
                return await Nok();
            }

            var res = context.Resource.FirstOrDefault();
            if (res == null)
                return await Ok();

            var action = context.Action.FirstOrDefault();
            var ownedRes = resources.Where(x => x.Name == res.Value).Single();
            var ownedAction = ownedRes.Actions.Where(x => x.Name == action.Value).Single();

            if (ownedAction.CanAccess(context.Principal))
                return await Ok();
            else
                return await Nok();
        }

        private List<Resource> resources = AuthorizationRules.ResourceRules;
    }

    public class ClaimsTransformation : ClaimsTransformationOptions
    {
        public ClaimsTransformation()
        {
            ClaimsTransformation = x =>
            {
                return new System.Threading.Tasks.Task<ClaimsPrincipal>(() =>
                {
                    return x;
                });
            };
        }
    }

    public class Resource
    {
        public Resource(string name, params ResourceAction[] actions)
        {
            Name = name;
            Actions = actions.ToList();
        }

        public string Name { get; set; }

        public List<ResourceAction> Actions { get; set; }
    }

    public class ResourceAction
    {
        public ResourceAction(string name, params string[] roles)
        {
            Name = name;
            Roles = roles;
        }

        public string Name { get; set; }

        public string[] Roles { get; set; }

        public bool CanAccess(ClaimsPrincipal principal)
        {
            if (Roles == null || Roles.Length == 0 || Roles.Contains("*"))
                return true;

            foreach (var allowedRole in Roles)
            {
                if (principal.IsInRole(allowedRole))
                    return true;
            }

            return false;
        }
    }
}
