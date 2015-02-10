using Elders.Pandora.Box;
using Elders.Pandora.UI.Security;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    public class UsersController : Controller
    {
        [ResourceAuthorize(Resources.Actions.Read, Resources.Admin)]
        public ActionResult Index(int count = 0, int start = 0, string filter = null)
        {
            var restClient = new RestSharp.RestClient(ConfigurationManager.AppSettings["IdentityManagerApiEndPoint"]);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            request.Resource = "users?count=" + count + "&start=" + start + "&filter=" + filter;

            var result = restClient.Execute<List<User>>(request);

            return View(result.Data);
        }

        [ResourceAuthorize(Resources.Actions.Manage, Resources.Admin)]
        public ActionResult Edit(Guid userId)
        {
            var restClient = new RestSharp.RestClient(ConfigurationManager.AppSettings["IdentityManagerApiEndPoint"]);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            request.Resource = "user?Id=" + userId;

            var result = restClient.Execute(request);

            var user = JsonConvert.DeserializeObject<User>(result.Content);

            var projects = GetProjects();

            return View(new Tuple<User, Dictionary<string, List<Jar>>>(user, projects));
        }

        [HttpPost]
        [ResourceAuthorize(Resources.Actions.Manage, Resources.Admin)]
        public ActionResult Edit(Guid userId, AccessRules[] access)
        {
            var restClient = new RestSharp.RestClient(ConfigurationManager.AppSettings["IdentityManagerApiEndPoint"]);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.POST;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            request.Resource = "user/updateclaim?Id=" + userId + "&type=SecurityAccess";

            var securityAccess = new SecurityAccess();

            if (access == null)
                access = new AccessRules[] { };

            foreach (var rule in access)
            {
                securityAccess.AddRule(rule);
            }

            request.AddBody(new ClaimValue() { Value = JsonConvert.SerializeObject(securityAccess) });

            var result = restClient.Execute(request);

            return RedirectToAction("Edit");
        }

        private Dictionary<string, List<Jar>> GetProjects()
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Projects";

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, List<Jar>>>(response.Content);
        }
    }
}