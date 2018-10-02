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
        public ApplicationContext(string applicationName = null, string cluster = null, string machine = null)
        {
            this.ApplicationName = applicationName ?? EnvVar.GetApplication();
            this.Cluster = cluster ?? EnvVar.GetCluster();
            this.Machine = machine ?? EnvVar.GetMachine();
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }

    public class ClusterContext : IPandoraContext
    {
        public ClusterContext(string applicationName = null, string cluster = null)
        {
            this.ApplicationName = applicationName ?? EnvVar.GetApplication();
            this.Cluster = cluster ?? EnvVar.GetCluster();
            this.Machine = Box.Machine.NotSpecified;
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }
}
