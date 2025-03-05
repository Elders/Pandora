using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public class Jar
    {
        public Jar()
        {
            References = new List<Dictionary<string, string>>();
            Defaults = new Dictionary<string, object>();
            Dynamics = new List<string>();
            Clusters = new Dictionary<string, Dictionary<string, object>>();
            Machines = new Dictionary<string, Dictionary<string, object>>();
        }

        public string Name { get; set; }

        public List<Dictionary<string, string>> References { get; set; }
        public Dictionary<string, object> Defaults { get; set; }
        public List<string> Dynamics { get; set; }
        public Dictionary<string, Dictionary<string, object>> Clusters { get; set; }
        public Dictionary<string, Dictionary<string, object>> Machines { get; set; }
    }
}
