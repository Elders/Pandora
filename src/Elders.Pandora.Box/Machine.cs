using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public sealed class Machine : Configuration
    {
        public const string ClusterKey = "cluster";

        public Machine(string name, Dictionary<string, string> settings) : base(name, settings) { }
        public Machine(Configuration configuration) : base(configuration.Name, configuration.AsDictionary()) { }
    }
}