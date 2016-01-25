using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    [Authorize]
    public class ClustersController : Controller
    {
        public ActionResult Index(string projectName, string applicationName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");

            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", hostName + "/Projects"));
            breadcrumbs.Add(new KeyValuePair<string, string>(projectName, hostName + "/Projects/" + projectName));
            ViewBag.Breadcrumbs = breadcrumbs;

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

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(config);
        }

        [HttpPost]
        public ActionResult Index(string projectName, string applicationName, string clusterName)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var newCluster = new Elders.Pandora.Box.Cluster(clusterName, new Dictionary<string, string>());

            var jar = GetConfig(projectName, applicationName);
            jar.Clusters.Add(newCluster.Name, newCluster.AsDictionary());

            var url = hostName + "/api/Clusters/" + projectName + "/" + applicationName + "/" + clusterName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());
            request.AddBody(JsonConvert.SerializeObject(new Dictionary<string, string>()));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            Elders.Pandora.UI.ViewModels.User.GiveAccess(projectName, applicationName, clusterName, Access.WriteAccess);

            var config = new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName);

            return View(config);
        }

        [HttpPost]
        public ActionResult Defaults(string projectName, string applicationName, Dictionary<string, string> config)
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = hostName + "/api/Defaults/" + projectName + "/" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.IdToken());
            request.AddBody(config);

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
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