using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
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
        Elders.Pandora.Box.Box box;

        public Pandora(Elders.Pandora.Box.Box box)
        {
            this.box = new Box.Box(box);
        }

        public Elders.Pandora.Box.Configuration Open(PandoraOptions options)
        {
            options = options ?? PandoraOptions.Defaults;

            foreach (var reference in box.References)
            {
                var refJarFile = reference.Values.First();
                var referenceJar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(refJarFile));
                var referenceBox = Box.Box.Mistranslate(referenceJar);

                box.Merge(referenceBox);
            }

            if (String.IsNullOrEmpty(options.ClusterName) && String.IsNullOrEmpty(options.MachineName))
                throw new ArgumentNullException("clusterName", "When getting configuraion for a machine the clusterName is required");

            var result = box.Defaults.AsDictionary();

            Cluster cluster = null;
            if (TryFindCluster(options.ClusterName, out cluster))
            {
                result = Merge(result, cluster.AsDictionary());
            }

            Machine machine = null;
            if (TryFindMachine(options.MachineName, out machine))
            {
                result = Merge(result, machine.AsDictionary());
            }

            if (options.UseRawSettingsNames)
                return new Elders.Pandora.Box.Configuration(box.Name, result);
            else
                return new Elders.Pandora.Box.Configuration(box.Name, NamenizeConfiguration(result, options.ClusterName, options.MachineName));
        }

        private Dictionary<string, string> NamenizeConfiguration(Dictionary<string, string> settings, string clusterName, string machineName)
        {
            return settings.ToDictionary(x => NameBuilder.GetSettingName(box.Name, clusterName, machineName, x.Key), y => y.Value);
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

    public class PandoraOptions
    {
        public PandoraOptions()
        {

        }

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