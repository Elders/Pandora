using System;

namespace Elders.Pandora
{
    public interface IPandoraContext
    {
        string ApplicationName { get; }

        string Cluster { get; }

        string Machine { get; }
    }

    public class ApplicationContext : IPandoraContext
    {
        public ApplicationContext(string applicationName, string cluster = null, string machine = null)
        {
            this.ApplicationName = applicationName;
            this.Cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME", EnvironmentVariableTarget.Machine);
            this.Machine = machine ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }

    public class ClusterContext : IPandoraContext
    {
        public ClusterContext(string applicationName, string cluster = null)
        {
            this.ApplicationName = applicationName;
            this.Cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME", EnvironmentVariableTarget.Machine);
            this.Machine = Box.Machine.NotSpecified;
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }
}
