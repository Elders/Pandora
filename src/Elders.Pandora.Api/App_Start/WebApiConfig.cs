using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Elders.Pandora.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var fomratter = config.Formatters.JsonFormatter;
            config.Formatters.Clear();
            fomratter.Indent = true;
            fomratter.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() };
            config.Formatters.Add(fomratter);
        }
    }
}
