using Elders.Pandora.Box;
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
    [Authorize]
    public class ProjectsController : Controller
    {
        public ActionResult Index()
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Projects";

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.AddHeader("Authorization", "Bearer " + User.Token());
            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            var projects = JsonConvert.DeserializeObject<Dictionary<string, List<Jar>>>(response.Content);

            return View(projects.Keys.ToList());
        }

        [HttpPost]
        public ActionResult Index(string projectName, string gitUrl)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Projects?projectName=" + projectName + "&gitUrl=" + gitUrl;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            var response = client.Execute(request);

            return Redirect("/Projects/" + projectName);
        }

        public ActionResult Applications(string projectName)
        {
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", ConfigurationManager.AppSettings["BaseUrl"] + "/Projects"));
            ViewBag.Breadcrumbs = breadcrumbs;

            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Jars?projectName=" + projectName;

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

            var jars = JsonConvert.DeserializeObject<List<Elders.Pandora.Box.Jar>>(response.Content);

            var configs = new List<Elders.Pandora.UI.ViewModels.Configuration>();

            foreach (var jar in jars)
            {
                configs.Add(new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName));
            }

            return View(configs);
        }

        [HttpPost]
        public ActionResult Applications(string projectName, string applicationName)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Jars?projectName=" + projectName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var jar = new Jar();
            jar.Name = applicationName;

            request.AddBody(JsonConvert.SerializeObject(jar));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return Redirect("/Projects/" + projectName + "/" + applicationName);
        }
    }
}