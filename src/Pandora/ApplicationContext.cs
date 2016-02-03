using System;

namespace Elders.Pandora
{
    public class ApplicationContext
    {
        public ApplicationContext(string applicationName, string cluster = null, string machine = null)
        {
            this.ApplicationName = applicationName;
            this.Cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME");
            this.Machine = machine ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }
}
