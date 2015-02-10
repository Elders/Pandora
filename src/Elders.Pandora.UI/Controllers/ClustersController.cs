using Elders.Pandora.UI.Security;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Mvc;

namespace Elders.Pandora.UI.Controllers
{
    [Authorize]
    public class ClustersController : Controller
    {
        public ActionResult Index(string projectName, string applicationName)
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName;

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

            var config = new Configuration(jar, projectName);

            return View(config);
        }

        [HttpPost]
        public ActionResult Index(string projectName, string applicationName, string clusterName)
        {
            var newCluster = new Elders.Pandora.Box.Cluster(clusterName, new Dictionary<string, string>());

            var jar = GetConfig(projectName, applicationName);
            jar.Clusters.Add(newCluster.Name, newCluster.AsDictionary());

            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Clusters?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());
            request.AddBody(JsonConvert.SerializeObject(newCluster));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            var config = new Configuration(jar, projectName);

            return View(config);
        }

        [HttpPost]
        public ActionResult Defaults(string projectName, string applicationName, Dictionary<string, string> config)
        {
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Defaults?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());
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
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName;

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