using Elders.Pandora.Box;
using Elders.Pandora.UI.Security;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    public class UsersController : Controller
    {
        [ResourceAuthorize(Resources.Actions.Read, Resources.Users)]
        public ActionResult Index(int count = 0, int start = 0, string filter = null)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Users";

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var result = restClient.Execute<List<User>>(request);

            foreach (var user in result.Data)
            {
                GetUserInfo(user);
            }

            return View(result.Data);
        }

        [ResourceAuthorize(Resources.Actions.Manage, Resources.Users)]
        public ActionResult Edit(string userId)
        {
            var user = GetUser(userId);

            GetUserInfo(user);

            var projects = GetProjects();

            var allJars = new Dictionary<string, List<Jar>>();

            foreach (var project in projects)
            {
                var jars = GetJars(project);

                allJars.Add(project, jars);
            }

            return View(new Tuple<User, Dictionary<string, List<Jar>>>(user, allJars));
        }

        [HttpPost]
        [ResourceAuthorize(Resources.Actions.Manage, Resources.Users)]
        public ActionResult Edit(string userId, AccessRules[] access)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Users?Id=" + userId;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.PUT;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var securityAccess = new SecurityAccess();

            if (access == null)
                access = new AccessRules[] { };

            foreach (var rule in access)
            {
                securityAccess.AddRule(rule);
            }

            var user = GetUser(userId);

            user.Access = securityAccess;

            request.AddBody(user);

            var result = restClient.Execute(request);

            return RedirectToAction("Edit");
        }

        private User GetUser(string userId)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Users?Id=" + userId;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var result = restClient.Execute(request);

            return JsonConvert.DeserializeObject<User>(result.Content);
        }

        private List<string> GetProjects()
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Projects";

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

            return JsonConvert.DeserializeObject<List<string>>(response.Content);
        }

        private List<Jar> GetJars(string projectName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Jars?projectName=" + projectName;

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

            return JsonConvert.DeserializeObject<List<Jar>>(response.Content);
        }

        private void GetUserInfo(User user)
        {
            var url = "https://www.googleapis.com/plus/v1/people/" + user.Id;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var result = restClient.Execute<GoogleUserInfo>(request);

            var info = result.Data;
            if (info != null)
            {
                user.AvatarUrl = info.Image.Url;
                user.FullName = info.DisplayName;
                user.FirstName = info.Name.GivenName;
                user.LastName = info.Name.FamilyName;

                if (info.Organizations != null)
                {
                    var organization = info.Organizations.FirstOrDefault(x => x.Primay == true);

                    if (organization == null)
                        organization = info.Organizations.FirstOrDefault();

                    if (organization != null)
                        user.Organization = organization.Name;
                }
            }
        }

        public class GoogleUserInfo
        {
            public string DisplayName { get; set; }

            public string Gender { get; set; }

            public GoogleImage Image { get; set; }

            public GoogleName Name { get; set; }

            public List<GoogleOrganization> Organizations { get; set; }
        }

        public class GoogleImage
        {
            public string Url { get; set; }
        }

        public class GoogleName
        {
            public string FamilyName { get; set; }

            public string GivenName { get; set; }
        }

        public class GoogleOrganization
        {
            public string Name { get; set; }

            public bool Primay { get; set; }

            public string Type { get; set; }
        }
    }
}