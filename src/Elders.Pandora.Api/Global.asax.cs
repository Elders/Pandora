using Elders.Pandora.Api.Tcp;
using System;
using System.IO;
using System.Web.Http;

namespace Elders.Pandora.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static Server tcpServer;

        public static Server TcpServer
        {
            get
            {
                return tcpServer;
            }
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            GlobalConfiguration.Configure(WebApiConfig.Register);

            string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);

            tcpServer = new Server();
        }
    }
}