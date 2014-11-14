using System;
using Elders.Pandora.Box;

namespace Elders.Pandora
{
    public class ApplicationConfiguration
    {
        private readonly string applicationName;
        private readonly string cluster;
        private readonly string machine;

        public ApplicationConfiguration(string applicationName, string cluster = null, string machine = null)
        {
            this.applicationName = applicationName;
            this.cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME", EnvironmentVariableTarget.Machine);
            this.machine = machine ?? Environment.GetEnvironmentVariable("COMPUTERNAME", EnvironmentVariableTarget.Machine);
        }

        public string Get(string key)
        {
            string longKey = NameBuilder.GetSettingName(applicationName, cluster, machine, key);
            return Environment.GetEnvironmentVariable(longKey, EnvironmentVariableTarget.Machine);
        }
    }
}
