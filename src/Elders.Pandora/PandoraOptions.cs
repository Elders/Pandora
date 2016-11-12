using System;

namespace Elders.Pandora
{
    public class PandoraOptions
    {
        public PandoraOptions() { }

        public PandoraOptions(string clusterName, string machineName)
        {
            ClusterName = clusterName;
            MachineName = machineName;
        }

        public string ClusterName { get; set; }

        public string MachineName { get; set; }

        public static PandoraOptions Defaults = new PandoraOptions() { ClusterName = "local", MachineName = Environment.MachineName };
    }
}