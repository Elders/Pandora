using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    [Authorize]
    public class MachinesController : Controller
    {
        public ActionResult Index(string projectName, string applicationName, string clusterName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", hostName + "/Projects"));
            breadcrumbs.Add(new KeyValuePair<string, string>(projectName, hostName + "/Projects/" + projectName));
            breadcrumbs.Add(new KeyValuePair<string, string>(applicationName, hostName + "/Projects/" + projectName + "/" + applicationName + "/Clusters"));
            ViewBag.Breadcrumbs = breadcrumbs;

            var jar = GetConfig(projectName, applicationName);

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(new Tuple<string, Elders.Pandora.UI.ViewModels.Configuration>(clusterName, config));
        }

        [HttpPost]
        public ActionResult Index(string projectName, string applicationName, string clusterName, Dictionary<string, string> config)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = hostName + "/api/Clusters/" + projectName + "/" + applicationName + "/" + clusterName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());

            request.AddBody(JsonConvert.SerializeObject(config));

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
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Machines/" + projectName + "/" + applicationName + "/" + machineName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());

            var jar = GetConfig(projectName, applicationName);

            request.AddBody(JsonConvert.SerializeObject(new Dictionary<string, string>()));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", hostName + "/Projects"));
            breadcrumbs.Add(new KeyValuePair<string, string>(projectName, hostName + "/Projects/" + projectName));
            breadcrumbs.Add(new KeyValuePair<string, string>(applicationName, hostName + "/Projects/" + projectName + "/" + applicationName + "/Clusters"));
            breadcrumbs.Add(new KeyValuePair<string, string>(clusterName, hostName + "/Projects/" + projectName + "/" + applicationName + "/" + clusterName + "/Machines"));
            ViewBag.Breadcrumbs = breadcrumbs;

            var jar = GetConfig(projectName, applicationName);

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(new Tuple<string, string, Elders.Pandora.UI.ViewModels.Configuration>(clusterName, machineName, config));
        }

        [HttpPost]
        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName, Dictionary<string, string> config)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = hostName + "/api/Machines/" + projectName + "/" + applicationName + "/" + machineName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());

            request.AddBody(JsonConvert.SerializeObject(config));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Machine");
        }

        private Elders.Pandora.Box.Jar GetConfig(string projectName, string applicationName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Jars/" + projectName + "/" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());
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