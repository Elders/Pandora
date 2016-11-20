using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public sealed class Machine : Configuration
    {
        /// <summary>
        /// Use this name for a machine when your intent is to work with all the machines in the cluster
        /// </summary>
        public const string NotSpecified = "*";
        public const string ClusterKey = "cluster";

        public Machine(string name, Dictionary<string, string> settings) : base(name, settings) { }
        public Machine(Configuration configuration) : base(configuration.Name, configuration.AsDictionary()) { }
    }
}
