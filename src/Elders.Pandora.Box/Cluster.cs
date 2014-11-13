using System.Collections.Generic;

namespace Elders.Padnora.Box
{
    public sealed class Cluster : Configuration
    {
        public Cluster(string name, Dictionary<string, string> settings) : base(name, settings) { }
    }
}