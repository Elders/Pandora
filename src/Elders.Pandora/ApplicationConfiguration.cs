using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elders.Pandora.Box;

namespace Elders.Pandora
{
    public static class ApplicationConfiguration
    {
        private static string applicationName;
        private static string cluster;
        private static string machine;

        public static void SetContext(string applicationName, string cluster = null, string machine = null)
        {
            ApplicationConfiguration.applicationName = applicationName;
            ApplicationConfiguration.cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME", EnvironmentVariableTarget.Machine);
            ApplicationConfiguration.machine = machine ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }

        public static string Get(string key)
        {
            Guard_ValidPandoraContext();

            string longKey = NameBuilder.GetSettingName(applicationName, cluster, machine, key);
            var setting = Environment.GetEnvironmentVariable(longKey, EnvironmentVariableTarget.Machine);
            if (setting == null)
                throw new KeyNotFoundException("Unable to find environment variable " + longKey);
            return setting;
        }

        public static IEnumerable<DeployedSetting> GetAll()
        {
            Guard_ValidPandoraContext();

            return from setting in GetAllOnMachine()
                   where setting.Cluster == cluster && setting.Machine == machine && setting.ApplicationName == applicationName
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
            if (String.IsNullOrEmpty(cluster) ||
                String.IsNullOrEmpty(machine) ||
                String.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException("Please use 'SetContext' method first.");
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
