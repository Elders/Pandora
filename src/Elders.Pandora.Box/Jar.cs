using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public class Jar
    {
        public Jar()
        {
            References = new List<Dictionary<string, string>>();
            Defaults = new Dictionary<string, string>();
            Clusters = new Dictionary<string, Dictionary<string, string>>();
            Machines = new Dictionary<string, Dictionary<string, string>>();
        }

        public string Name { get; set; }

        public List<Dictionary<string, string>> References { get; set; }
        public Dictionary<string, string> Defaults { get; set; }
        public Dictionary<string, Dictionary<string, string>> Clusters { get; set; }
        public Dictionary<string, Dictionary<string, string>> Machines { get; set; }
    }
}