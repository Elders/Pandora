using Elders.Pandora.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace Elders.Pandora.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars";

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            var jars = JsonConvert.DeserializeObject<List<Elders.Pandora.Box.Jar>>(response.Content);

            var configs = new List<Configuration>();

            foreach (var jar in jars)
            {
                configs.Add(new Configuration(jar.Name, JsonConvert.SerializeObject(jar, Formatting.Indented)));
            }

            return View(configs);
        }

        public ActionResult Config(string name)
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars?name=" + name;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(response.Content);

            var config = new Configuration(jar.Name, JsonConvert.SerializeObject(jar, Formatting.Indented));

            return View(config);
        }

        [HttpPost]
        public ActionResult Config(string gitUrl, string email, string username, string password, string message, string content)
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars?gitUrl=" + gitUrl + "&email=" + email + "&username=" + username + "&password=" + password + "&message=" + message;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.PUT);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(content);

            request.AddBody(JsonConvert.SerializeObject(jar));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Config", new { name = jar.Name });
        }

        public ActionResult AddConfig()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddConfig(string gitUrl, string email, string username, string password, string message, string config)
        {
            var url = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port + "/api/Jars?gitUrl=" + gitUrl + "&email=" + email + "&username=" + username + "&password=" + password + "&message=" + message;

            var client = new RestSharp.RestClient(url);
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");

            var jar = JsonConvert.DeserializeObject<Elders.Pandora.Box.Jar>(config);

            request.AddBody(JsonConvert.SerializeObject(jar));

            var response = client.Execute(request);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                throw response.ErrorException;
            }

            return RedirectToAction("Config", new { name = jar.Name });
        }
    }
}