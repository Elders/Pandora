using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static ApplicationContext CreateContext(string applicationName)
        {
            return new ApplicationContext(applicationName);
        }

        public static string Get(string key)
        {
            Guard_ValidPandoraContext();
            return Get(key, context);
        }

        public static string Get(string key, ApplicationContext context)
        {
            var sanitizedKey = key.ToLower();
            string longKey = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, sanitizedKey);
            var setting = GetRepository().Get(longKey);
            return setting;
        }

        public static T Get<T>(string key)
        {
            var json = Get(key);
            if (json == null)
                return default(T);
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
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
            Guard_ValidPandoraContext();

            return from setting in GetAllOnMachine()
                   where setting.Cluster == context.Cluster &&
                         setting.Machine == context.Machine &&
                         setting.ApplicationName == context.ApplicationName
                   select setting;
        }

        public static IEnumerable<DeployedSetting> GetAllOnMachine()
        {
            var regex = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");

            var all = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);

            foreach (DictionaryEntry item in all)
            {
                var result = regex.Match(item.Key.ToString());
                if (result.Success)
                {
                    yield return new DeployedSetting(
                        raw: result.Groups[0].Value,
                        applicationName: result.Groups[1].Value,
                        cluster: result.Groups[2].Value,
                        machine: result.Groups[3].Value,
                        key: result.Groups[4].Value,
                        value: Environment.GetEnvironmentVariable(result.Groups[0].Value, EnvironmentVariableTarget.Machine));
                }
            }
        }

        private static void Guard_ValidPandoraContext()
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Please use 'SetContext' method first.");
        }
    }

    public class DeployedSetting
    {
        public DeployedSetting(string raw, string applicationName, string cluster, string machine, string key, string value)
        {
            Raw = raw;
            ApplicationName = applicationName;
            Cluster = cluster;
            Machine = machine;
            Key = key;
            Value = value;
        }

        public string Raw { get; private set; }
        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
