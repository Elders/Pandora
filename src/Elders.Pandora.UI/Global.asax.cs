using Elders.Pandora.UI.Tcp;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

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

            string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

            if (!Directory.Exists(storageFolder))
                Directory.CreateDirectory(storageFolder);

            tcpServer = new TcpServer();
        }
    }
}
