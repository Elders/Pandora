using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Elders.Pandora.UI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "UsersRout",
                url: "Users/{userId}",
                defaults: new { controller = "Users", action = "Edit" }
            );

            routes.MapRoute(
                name: "DefaultsRoute",
                url: "Projects/{projectName}/{applicationName}/Clusters/Defaults",
                defaults: new { controller = "Clusters", action = "Defaults" }
            );

            routes.MapRoute(
                name: "AddMachineRoute",
                url: "Projects/{projectName}/{applicationName}/{clusterName}/Machines/AddMachine",
                defaults: new { controller = "Machines", action = "AddMachine" }
            );

            routes.MapRoute(
                name: "AplicationRoute",
                url: "Projects/{projectName}/{applicationName}/{clusterName}/Machines",
                defaults: new { controller = "Machines", action = "Index" }
            );

            routes.MapRoute(
                name: "MachineRoute",
                url: "Projects/{projectName}/{applicationName}/{clusterName}/{machineName}",
                defaults: new { controller = "Machines", action = "Machine" }
            );

            routes.MapRoute(
                name: "ClustersRoute",
                url: "Projects/{projectName}/{applicationName}/Clusters",
                defaults: new { controller = "Clusters", action = "Index" }
            );

            routes.MapRoute(
                 name: "ProjectRoute",
                 url: "Projects/{projectName}",
                 defaults: new { controller = "Projects", action = "Applications" }
             );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
