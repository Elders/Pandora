using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Pandora.Box;

namespace Elders.Configuration.Console
{
    /// <summary>
    /// In Greek mythology, Pandora was the first human woman created by the gods, specifically by Hephaestus and Athena on the 
    /// instructions of Zeus. As Hesiod related it, each god helped create her by giving her unique gifts. Zeus ordered Hephaestus to 
    /// mold her out of earth as part of the punishment of humanity for Prometheus' theft of the secret of fire, and all the gods joined in 
    /// offering her "seductive gifts". Her other name—inscribed against her figure on a white-ground kylix in the British Museum—is Anesidora, 
    /// "she who sends up gifts" (up implying "from below" within the earth)
    /// </summary>
    /// <remarks>http://en.wikipedia.org/wiki/Pandora</remarks>
    public class Pandora
    {
        Box box;

        public Pandora(Box box)
        {
            this.box = box;
        }

        public Elders.Pandora.Box.Configuration Open(string clusterName = "", string machineName = "")
        {
            if (String.IsNullOrEmpty(clusterName) && String.IsNullOrEmpty(machineName))
                throw new ArgumentNullException("clusterName", "When getting configuraion for a machine the clusterName is required");

            var result = box.Defaults.AsDictionary();

            Cluster cluster = null;
            if (TryFindCluster(clusterName, out cluster))
            {
                result = Merge(result, cluster.AsDictionary());
            }

            Machine machine = null;
            if (TryFindMachine(machineName, out machine))
            {
                result = Merge(result, machine.AsDictionary());
            }



            return new Elders.Pandora.Box.Configuration(box.Name, NamenizeConfiguration(result, clusterName, machineName));
        }

        private Dictionary<string, string> NamenizeConfiguration(Dictionary<string, string> settings, string clusterName, string machineName)
        {
            string theName = (box.Name + "@@" + clusterName + "^" + machineName).Replace("^^", "^");
            return settings.ToDictionary(x => theName + "~~" + x.Key, y => y.Value);
        }

        private bool TryFindCluster(string clusterName, out Cluster cluster)
        {
            cluster = box.Clusters.Where(x => x.Name == clusterName).SingleOrDefault();
            return cluster != null;
        }

        private bool TryFindMachine(string machineName, out Machine machine)
        {
            machine = box.Machines.Where(x => x.Name == machineName).SingleOrDefault();
            return machine != null;
        }

        private Dictionary<T1, T2> Merge<T1, T2>(Dictionary<T1, T2> first, Dictionary<T1, T2> second)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");

            var merged = new Dictionary<T1, T2>();
            first.ToList().ForEach(kv => merged[kv.Key] = kv.Value);
            second.ToList().ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }

    }
}