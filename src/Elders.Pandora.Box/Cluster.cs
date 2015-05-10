using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public sealed class Cluster : Configuration
    {
        public Cluster(string name, Dictionary<string, string> settings) : base(name, settings) { }
        public Cluster(Configuration configuration) : base(configuration.Name, configuration.AsDictionary()) { }
    }
}