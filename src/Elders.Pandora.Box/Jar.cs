using System.Collections.Generic;

namespace Elders.Padnora.Box
{
    public class Jar
    {
        public string Name { get; set; }

        public Dictionary<string, string> Defaults { get; set; }
        public Dictionary<string, Dictionary<string, string>> Clusters { get; set; }
        public Dictionary<string, Dictionary<string, string>> Machines { get; set; }
    }
}