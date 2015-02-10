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
    public class MachinesController : Controller
    {
        public ActionResult Index(string projectName, string applicationName, string clusterName)
        {
            var jar = GetConfig(projectName, applicationName);

            var config = new Configuration(jar, projectName);

            return View(new Tuple<string, Configuration>(clusterName, config));
        }

        [HttpPost]
        public ActionResult Index(string projectName, string applicationName, string clusterName, Dictionary<string, string> config)
        {
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Clusters?projectName=" + projectName + "&applicationName=" + applicationName;

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
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Machines?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var jar = GetConfig(projectName, applicationName);

            request.AddBody(JsonConvert.SerializeObject(new Elders.Pandora.Box.Machine(machineName, jar.Defaults)));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName)
        {
            var jar = GetConfig(projectName, applicationName);

            var config = new Configuration(jar, projectName);

            return View(new Tuple<string, string, Configuration>(clusterName, machineName, config));
        }

        [HttpPost]
        public ActionResult Machine(string projectName, string applicationName, string clusterName, string machineName, Dictionary<string, string> config)
        {
            if (config.ContainsKey("controller"))
                return RedirectToAction("Index");

            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Machines?projectName=" + projectName + "&applicationName=" + applicationName;

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