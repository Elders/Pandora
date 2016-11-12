using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    public class PandoraBoxOpener
    {
        Elders.Pandora.Box.Box box;

        public PandoraBoxOpener(Elders.Pandora.Box.Box box)
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

            Dictionary<string, string> result = new Dictionary<string, string>();
            var confDefault = box.Defaults.AsDictionary();

            Cluster cluster = null;
            Dictionary<string, string> confCluster = new Dictionary<string, string>();
            if (TryFindCluster(options.ClusterName, out cluster))
            {
                confCluster = cluster.AsDictionary();
            }

            Machine machine = null;
            Dictionary<string, string> confMachine = new Dictionary<string, string>();
            if (TryFindMachine(options.MachineName, out machine))
            {
                confMachine = machine.AsDictionary();
            }

            if (options.UseRawSettingsNames)
                return new Elders.Pandora.Box.Configuration(box.Name, Merge(confDefault, Merge(confCluster, confMachine)));
            else
            {
                var namanizedDefaltConfigs = NamenizeClusterConfiguration(confDefault, options.ClusterName);
                var namanizedMachineConfigs = NamenizeMachineConfiguration(confMachine, options.ClusterName, options.MachineName);
                var namanizedClusterConfigs = NamenizeClusterConfiguration(confCluster, options.ClusterName);

                return new Elders.Pandora.Box.Configuration(box.Name, Merge(namanizedDefaltConfigs, Merge(namanizedMachineConfigs, namanizedClusterConfigs)));
            }

        }

        Dictionary<string, string> NamenizeClusterConfiguration(Dictionary<string, string> settings, string clusterName)
        {
            return settings.ToDictionary(x => NameBuilder.GetSettingClusterName(box.Name, clusterName, x.Key), y => y.Value);
        }

        Dictionary<string, string> NamenizeMachineConfiguration(Dictionary<string, string> settings, string clusterName, string machineName)
        {
            return settings.ToDictionary(x => NameBuilder.GetSettingName(box.Name, clusterName, machineName, x.Key), y => y.Value);
        }

        bool TryFindCluster(string clusterName, out Cluster cluster)
        {
            cluster = box.Clusters.Where(x => x.Name == clusterName).SingleOrDefault();
            return cluster != null;
        }

        bool TryFindMachine(string machineName, out Machine machine)
        {
            machine = box.Machines.Where(x => x.Name == machineName).SingleOrDefault();
            return machine != null;
        }

        Dictionary<T1, T2> Merge<T1, T2>(Dictionary<T1, T2> first, Dictionary<T1, T2> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            var merged = new Dictionary<T1, T2>();
            first.ToList().ForEach(kv => merged[kv.Key] = kv.Value);
            second.ToList().ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }
    }
}