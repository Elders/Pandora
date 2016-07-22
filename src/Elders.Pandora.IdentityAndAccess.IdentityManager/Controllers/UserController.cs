using Elders.Pandora.IdentityAndAccess.IdentityManager.Filters;
using Elders.Pandora.IdentityAndAccess.IdentityManager.ViewModels;
using Elders.Pandora.IdentityAndAccess.UserConfiguration;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityManager;

namespace Elders.Pandora.IdentityAndAccess.IdentityManager.Controllers
{
    public class UserManagerAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var controller = actionContext.ControllerContext.Controller.GetType();
            var mngr = controller.GetProperty("UserManager");
            if (mngr != null)
                mngr.SetValue(actionContext.ControllerContext.Controller, new UserManager<PandoraUser, string>(new PandoraUserStore(new PandoraDbContext("DefaultConnection"))));
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var controller = actionExecutedContext.ActionContext.ControllerContext.Controller.GetType();
            var mngr = controller.GetProperty("UserManager");
            if (mngr != null)
            {
                var value = mngr.GetValue(actionExecutedContext.ActionContext.ControllerContext.Controller);
                if (value as IDisposable != null)
                    (value as IDisposable).Dispose();
            }
        }

    }
    [NoCache]
    [Authorize]
    [RoutePrefix("api")]
    [UserManager]
    public class UserController : ApiController
    {
        public UserManager<PandoraUser, string> UserManager { get; set; }



        [HttpGet]
        [Route("users")]
        public IEnumerable<User> Users(int count = 0, int start = 0, string filter = null)
        {
            IEnumerable<User> users;

            var query =
                from user in UserManager.Users
                let display = (from claim in user.Claims
                               where claim.ClaimType == Thinktecture.IdentityManager.Constants.ClaimTypes.Name
                               select claim.ClaimValue).FirstOrDefault()
                orderby display ?? user.Email
                select user;


            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.Trim().ToLower();

                query =
                    from user in query
                    let claims = (from claim in user.Claims
                                  where (claim.ClaimType == "family_name" || claim.ClaimType == "given_name") && claim.ClaimValue.ToLower().Contains(filter)
                                  select claim)
                    where user.UserName.ToLower().Contains(filter) || user.Email.ToLower().Contains(filter) || user.Id.ToLower().Contains(filter) || claims.Any()
                    select user;
            }

            if (count == 0)
                users = query.Skip(start).ToList().Select(x => UserBuilder.ToUserViewModel(x));
            else
                users = query.Skip(start).Take(count).ToList().Select(x => UserBuilder.ToUserViewModel(x));

            return users;
        }

        [HttpGet]
        [Route("user")]
        public User GetUser(Guid id)
        {
            return UserManager.Users.SingleOrDefault(x => x.Id == id.ToString()).ToUserViewModel();
        }

        [HttpPost]
        [Route("user/addclaim")]
        public async Task<IdentityManagerResult> AddClaim(string id, string type, string value)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new IdentityManagerResult("Subject is required.");
            if (string.IsNullOrWhiteSpace(type))
                return new IdentityManagerResult("Claim type is required.");
            if (string.IsNullOrWhiteSpace(value))
                return new IdentityManagerResult("Claim value is required.");

            var claims = await UserManager.GetClaimsAsync(id);

            var claim = claims.SingleOrDefault(x => x.Type == type);

            if (claim != null)
                return new IdentityManagerResult("Claim with type " + type + " already exists.");
            else
            {
                var addResult = await UserManager.AddClaimAsync(id, new System.Security.Claims.Claim(type, value));

                if (!addResult.Succeeded)
                    return new IdentityManagerResult(addResult.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        [HttpPost]
        [Route("user/removeclaim")]
        public async Task<IdentityManagerResult> RemoveClaim(string id, string type)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new IdentityManagerResult("Subject is required.");
            if (string.IsNullOrWhiteSpace(type))
                return new IdentityManagerResult("Claim type is required.");

            var claims = await UserManager.GetClaimsAsync(id);

            var claim = claims.SingleOrDefault(x => x.Type == type);

            if (claim != null)
            {
                var removeResult = await UserManager.RemoveClaimAsync(id, claim);

                if (!removeResult.Succeeded)
                    return new IdentityManagerResult(removeResult.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        [HttpPost]
        [Route("user/updateclaim")]
        public async Task<IdentityManagerResult> UpdateClaim(string id, string type, [FromBody]ClaimValue value)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new IdentityManagerResult("Subject is required.");
            if (string.IsNullOrWhiteSpace(type))
                return new IdentityManagerResult("Claim type is required.");
            if (string.IsNullOrWhiteSpace(value.Value))
                return new IdentityManagerResult("Claim value is required.");

            var claims = await UserManager.GetClaimsAsync(id);

            var claim = claims.SingleOrDefault(x => x.Type == type);

            if (claim == null)
                await UserManager.AddClaimAsync(id, new System.Security.Claims.Claim(type, value.Value));
            else if (claim.Value != value.Value)
            {
                var deleteResult = await UserManager.RemoveClaimAsync(id, claim);

                if (!deleteResult.Succeeded)
                    return new IdentityManagerResult(deleteResult.Errors.ToArray());
                else
                {
                    var addResult = await UserManager.AddClaimAsync(id, new System.Security.Claims.Claim(type, value.Value));

                    if (!addResult.Succeeded)
                        return new IdentityManagerResult(addResult.Errors.ToArray());
                }
            }

            return IdentityManagerResult.Success;
        }

        [HttpGet]
        [Route("user/getClaim")]
        public ClaimValue GetClaim(string id, string type)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            if (string.IsNullOrWhiteSpace(type))
                return null;

            var claims = UserManager.GetClaims(id);

            var claim = claims.SingleOrDefault(x => x.Type == type);

            if (claim == null)
                return new ClaimValue();

            return new ClaimValue() { Value = claim.Value };
        }

        public class ClaimValue
        {
            public string Value { get; set; }
        }
    }
}