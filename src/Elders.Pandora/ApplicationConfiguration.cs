using System;
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
            if (String.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException("Please use 'SetContext' method first.");

            string longKey = NameBuilder.GetSettingName(applicationName, cluster, machine, key);
            return Environment.GetEnvironmentVariable(longKey, EnvironmentVariableTarget.Machine);
        }
    }
}
