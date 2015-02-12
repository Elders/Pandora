using Elders.Pandora.UI.App_Start;
using Elders.Pandora.UI.Tcp;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Thinktecture.IdentityModel.Oidc;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.Configuration;

namespace Elders.Pandora.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static TcpServer tcpServer;

        public static TcpServer TcpServer
        {
            get
            {
                return tcpServer;
            }
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);
            OpenIdConnectAuthenticationModule.ClaimsTransformed += OnClaimsTransformed;
            //System.Diagnostics.Debugger.Launch();

            //if (tcpServer == null)
            //{
            //    tcpServer = new TcpServer();
            //    tcpServer.Start();
            //}
        }

        void OnClaimsTransformed(object sender, ClaimsIdentity args)
        {
            var user = GetUser(args);

            var access = JsonConvert.SerializeObject(user.Access, Formatting.Indented);

            args.AddClaim(new Claim("SecurityAccess", access));
        }

        private User GetUser(ClaimsIdentity args)
        {
            var claims = args.Claims;

            var userId = claims.Where(x => x.Type == "sub").FirstOrDefault().Value;

            string token = claims.Where(x => x.Type == "id_token").FirstOrDefault().Value;

            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Users?Id=" + userId;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.AddHeader("Authorization", "Bearer " + token);

            var result = restClient.Execute(request);

            User user = null;

            if (string.IsNullOrWhiteSpace(result.Content) || result.Content.ToLowerInvariant() == "null")
            {
                user = new User();
                user.Id = userId;
                user.Access = new SecurityAccess();

                CreateUser(user, token);
            }
            else
            {
                user = JsonConvert.DeserializeObject<User>(result.Content);
            }

            return user;
        }

        private void CreateUser(User user, string token)
        {
            var url = ConfigurationManager.AppSettings["BaseUrl"] + "/api/Users?Id=" + user.Id;

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.POST;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + token);

            request.AddBody(user);

            restClient.Execute(request);
        }
    }
}