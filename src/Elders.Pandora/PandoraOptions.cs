using System;

namespace Elders.Pandora
{
    public class PandoraOptions
    {
        public PandoraOptions() { }

        public PandoraOptions(string clusterName, string machineName, bool useRawSettingsNames)
        {
            ClusterName = clusterName;
            MachineName = machineName;
            UseRawSettingsNames = useRawSettingsNames;
        }

        public string ClusterName { get; set; }

        public string MachineName { get; set; }

        public bool UseRawSettingsNames { get; set; }

        public static PandoraOptions Defaults = new PandoraOptions() { ClusterName = "local", MachineName = Environment.MachineName, UseRawSettingsNames = false };
    }
}