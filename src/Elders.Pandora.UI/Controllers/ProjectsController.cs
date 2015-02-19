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
using System.Collections;
using System.Web;
using System.Net.Mime;

namespace Elders.Pandora.UI.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        public ActionResult Index()
        {
            var hostName = ApplicationConfiguration.Get("host_name");
            var url = hostName + "/api/Projects";

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
            var hostName = ApplicationConfiguration.Get("host_name");
            var url = hostName + "/api/Projects?projectName=" + projectName + "&gitUrl=" + gitUrl;

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
            var hostName = ApplicationConfiguration.Get("host_name");
            var breadcrumbs = new List<KeyValuePair<string, string>>();
            breadcrumbs.Add(new KeyValuePair<string, string>("Projects", hostName + "/Projects"));
            ViewBag.Breadcrumbs = breadcrumbs;

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

            var jars = JsonConvert.DeserializeObject<List<Elders.Pandora.Box.Jar>>(response.Content);

            var configs = new List<Elders.Pandora.UI.ViewModels.Configuration>();

            foreach (var jar in jars)
            {
                configs.Add(new Elders.Pandora.UI.ViewModels.Configuration(jar, projectName));
            }

            return View(configs);
        }

        [HttpPost]
        public ActionResult Applications(string projectName, string applicationName, string fileName, string config)
        {
            var hostName = ApplicationConfiguration.Get("host_name");
            var url = hostName + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName + "&fileName=" + fileName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            if (!string.IsNullOrWhiteSpace(config))
            {
                try
                {
                    var jar = JsonConvert.DeserializeObject<Jar>(config);
                    var box = Box.Box.Mistranslate(jar);
                }
                catch (Exception)
                {
                    var jar = new Jar();
                    jar.Name = applicationName;

                    config = JsonConvert.SerializeObject(jar);
                }
            }
            else
            {
                var jar = new Jar();
                jar.Name = applicationName;

                config = JsonConvert.SerializeObject(jar);
            }

            request.AddBody(config);

            var response = client.Execute(request);

            Elders.Pandora.UI.ViewModels.User.GiveAccess(projectName, applicationName, "Defaults", Access.WriteAccess);

            return Redirect("/Projects/" + projectName + "/" + applicationName + "/Clusters");
        }

        public ActionResult Update(string projectName)
        {
            var hostName = ApplicationConfiguration.Get("host_name");
            var url = hostName + "/api/Projects/Update?projectName=" + projectName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Index");
        }

        public ActionResult OpenJson(string projectName, string applicationName)
        {
            var hostName = ApplicationConfiguration.Get("host_name");

            var url = hostName + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var response = client.Execute(request);

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(response.Content);

            return View("_ApplicationJsonView", model: response.Content);
        }

        public FileResult DownloadJson(string projectName, string applicationName)
        {
            var hostName = ApplicationConfiguration.Get("host_name");

            var url = hostName + "/api/Jars?projectName=" + projectName + "&applicationName=" + applicationName;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.AddHeader("Authorization", "Bearer " + User.Token());

            var response = client.Execute(request);

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(response.Content);

            string fileName = applicationName + ".json";

            byte[] bytes = new byte[response.Content.Length * sizeof(char)];
            System.Buffer.BlockCopy(response.Content.ToCharArray(), 0, bytes, 0, bytes.Length);

            return File(bytes, MimeMapping.GetMimeMapping(fileName), fileName);
        }
    }
}