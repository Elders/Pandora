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
    public class MachinesController : Controller
    {
        public ActionResult Index(string projectName, string applicationName, string clusterName)
        {
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", ConfigurationManager.AppSettings["BaseUrl"] + "/Projects"));
            breadcrumbs.Add(new KeyValuePair<string, string>(projectName, ConfigurationManager.AppSettings["BaseUrl"] + "/Projects/" + projectName));
            breadcrumbs.Add(new KeyValuePair<string, string>(applicationName, ConfigurationManager.AppSettings["BaseUrl"] + "/Projects/" + projectName + "/" + applicationName + "/Clusters"));
            ViewBag.Breadcrumbs = breadcrumbs;

            var jar = GetConfig(projectName, applicationName);

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(new Tuple<string, Elders.Pandora.UI.ViewModels.Configuration>(clusterName, config));
        }

        [HttpPost]
        public ActionResult Index(string projectName, string applicationName, string clusterName, Dictionary<string, string> config)
        {
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Clusters?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            request.AddBody(JsonConvert.SerializeObject(new Elders.Pandora.Box.Cluster(clusterName, config)));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddMachine(string projectName, string applicationName, string clusterName, string machineName)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Machines?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var jar = GetConfig(projectName, applicationName);

            request.AddBody(JsonConvert.SerializeObject(new Elders.Pandora.Box.Machine(machineName, new Dictionary<string, string>())));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName)
        {
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", ConfigurationManager.AppSettings["BaseUrl"] + "/Projects"));
            breadcrumbs.Add(new KeyValuePair<string, string>(projectName, ConfigurationManager.AppSettings["BaseUrl"] + "/Projects/" + projectName));
            breadcrumbs.Add(new KeyValuePair<string, string>(applicationName, ConfigurationManager.AppSettings["BaseUrl"] + "/Projects/" + projectName + "/" + applicationName + "/Clusters"));
            breadcrumbs.Add(new KeyValuePair<string, string>(clusterName, ConfigurationManager.AppSettings["BaseUrl"] + "/Projects/" + projectName + "/" + applicationName + "/" + clusterName + "/Machines"));
            ViewBag.Breadcrumbs = breadcrumbs;

            var jar = GetConfig(projectName, applicationName);

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(new Tuple<string, string, Elders.Pandora.UI.ViewModels.Configuration>(clusterName, machineName, config));
        }

        [HttpPost]
        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName, Dictionary<string, string> config)
        {
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Machines?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            request.AddBody(JsonConvert.SerializeObject(new Elders.Pandora.Box.Machine(machineName, config)));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Machine");
        }

        private Elders.Pandora.Box.Jar GetConfig(string projectName, string applicationName)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName;

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

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(response.Content);

            return jar;
        }
    }
}