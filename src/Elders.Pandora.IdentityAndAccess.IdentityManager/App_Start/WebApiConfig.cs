using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Elders.Pandora.IdentityAndAccess.IdentityManager
{
    public class WebApiConfig
    {
        public static HttpConfiguration Build()
        {
            // Web API configuration and services
            var config = new HttpConfiguration();

            // Web API routes
            WebApiConfig.Configure(config);
            return config;
        }

        public static JsonMediaTypeFormatter Formatter { get; private set; }

        private static void Configure(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.SuppressDefaultHostAuthentication();

            //config.Filters.Add(new HostAuthenticationAttribute("imgr.local"));
            config.Filters.Add(new HostAuthenticationAttribute("Bearer"));

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Remove(config.Formatters.FormUrlEncodedFormatter);
            config.Formatters.JsonFormatter.Indent = true;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            Formatter = config.Formatters.JsonFormatter;

            config.BindParameter(typeof(DateTime?), new UTCDateTimeModelBinder());
            config.BindParameter(typeof(DateTime), new UTCDateTimeModelBinder());
        }
    }

    public class UTCDateTimeModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // Check if the DateTime property being parsed is not null or "" (for JSONO
            if (value != null && value.AttemptedValue != null && value.AttemptedValue != "")
            {
                // Parse the datetime then convert it back to universal time.
                var dt = DateTime.Parse(value.AttemptedValue);
                var newDt = DateTime.FromFileTimeUtc(dt.ToFileTimeUtc());
                bindingContext.Model = newDt;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
