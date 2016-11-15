using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    public static class ApplicationConfiguration
    {
        static ApplicationContext context = null;
        static IConfigurationRepository cfgRepo = null;

        static IConfigurationRepository GetRepository()
        {
            return cfgRepo ?? new WindowsEnvironmentVariables();
        }

        public static void SetContext(string applicationName, string cluster = null, string machine = null)
        {
            SetContext(new ApplicationContext(applicationName, cluster, machine));
        }

        public static void SetContext(ApplicationContext applicationContext)
        {
            context = applicationContext;
        }

        public static void SetRepository(IConfigurationRepository configurationRepository)
        {
            cfgRepo = configurationRepository ?? new WindowsEnvironmentVariables();
        }

        public static ApplicationContext CreateContext(string applicationName,string cluster = null, string machine = null)
        {
            return new ApplicationContext(applicationName.ToLower(), cluster.ToLower(), machine.ToLower());
        }

        public static string Get(string key)
        {
            return Get(key, context);
        }

        public static string Get(string key, ApplicationContext applicationContext)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ReferenceEquals(null, applicationContext)) throw new ArgumentNullException(nameof(applicationContext));

            var sanitizedKey = key.ToLower();
            string longKey = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, applicationContext.Machine, sanitizedKey);
            var setting = GetRepository().Get(longKey);
            return setting;
        }

        public static T Get<T>(string key)
        {
            return Get<T>(key, context);
        }

        public static T Get<T>(string key, ApplicationContext context)
        {
            var json = Get(key, context);
            if (json == null)
                return default(T);
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public static IEnumerable<DeployedSetting> GetAll()
        {
            return GetAll(context);
        }

        public static IEnumerable<DeployedSetting> GetAll(ApplicationContext applicationContext)
        {
            return from setting in GetRepository().GetAll()
                   where setting.Cluster == applicationContext.Cluster &&
                         setting.Machine == applicationContext.Machine &&
                         setting.ApplicationName == applicationContext.ApplicationName
                   select setting;
        }
    }
}
