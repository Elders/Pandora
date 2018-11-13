using System;

namespace Elders.Pandora
{
    public static class EnvVar
    {
        public const string ClusterOldKey = "CLUSTER_NAME";
        public const string MachineOldKey = "COMPUTERNAME";
        public const string ApplicationOldKey = "APPLICATION_NAME";

        public const string ClusterKey = "pandora_cluster";
        public const string ApplicationKey = "pandora_application";
        public const string MachineKey = "pandora_machine";


        /// <summary>
        /// Gets the application name stored in <see cref="ApplicationKey"/> environment variable
        /// </summary>
        /// <returns>Returns the application name</returns>
        public static string GetApplication()
        {
            return GetEnvironmentVariable(ApplicationKey, ApplicationOldKey);
        }

        /// <summary>
        /// Gets the cluster stored in <see cref="ClusterKey"/> or <see cref="ClusterOldKey"/> environment variable
        /// </summary>
        /// <returns>Returns the clsuter name</returns>
        public static string GetCluster()
        {
            return GetEnvironmentVariable(ClusterKey, ClusterOldKey);
        }

        /// <summary>
        /// Gets the machine/node name stored in <see cref="MachineKey"/> or <see cref="MachineOldKey"/> environment variable
        /// </summary>
        /// <returns>Returns the clsuter name</returns>
        public static string GetMachine()
        {
            return GetEnvironmentVariable(MachineKey, MachineOldKey);
        }

        public static string GetEnvironmentVariable(string key, string fallback)
        {
            return GetEnvironmentVariable(key) ?? GetEnvironmentVariable(fallback);
        }

        public static string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
        }
    }
}
