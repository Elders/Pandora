﻿using Elders.Pandora.Box;
using Elders.Pandora.UI.Security;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    public class UsersController : Controller
    {
        [ResourceAuthorize(Resources.Actions.Read, Resources.Users)]
        public ActionResult Index(int count = 0, int start = 0, string filter = null)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Users";

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

            return View(new Tuple<User, Dictionary<string, List<Jar>>>(user, projects));
        }

        [HttpPost]
        [ResourceAuthorize(Resources.Actions.Manage, Resources.Users)]
        public ActionResult Edit(string userId, AccessRules[] access)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Users?Id=" + userId;

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
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Users?Id=" + userId;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var result = restClient.Execute(request);

            return JsonConvert.DeserializeObject<User>(result.Content);
        }

        private Dictionary<string, List<Jar>> GetProjects()
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Projects";

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

        private void GetUserInfo(User user)
        {
            var url = "https://www.googleapis.com/plus/v1/people/" + user.Id;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var result = restClient.Execute<GoogleUserInfo>(request);

            var info = result.Data;

            user.AvatarUrl = info.Image.Url;
            user.FullName = info.DisplayName;
            user.FirstName = info.Name.GivenName;
            user.LastName = info.Name.FamilyName;

            var organization = info.Organizations.FirstOrDefault(x => x.Primay == true);

            if (organization == null)
                organization = info.Organizations.FirstOrDefault();

            if (organization != null)
                user.Organization = organization.Name;
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