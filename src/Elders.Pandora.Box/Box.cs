using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Pandora.Box
{
    /// <summary>
    /// Pandora's box is an artifact in Greek mythology, taken from the myth of Pandora's creation in Hesiod's Works and Days. 
    /// The "box" was actually a large jar given to Pandora, which contained all the evils of the world. Today the phrase 
    /// "to open Pandora's box" means to perform an action that may seem small or innocent, but that turns out to have severely 
    /// detrimental and far-reaching consequences.
    /// </summary>
    /// <remarks>http://en.wikipedia.org/wiki/Pandora%27s_box</remarks>
    public class Box
    {
        private readonly List<string> reservedKeys;

        public Box(Jar jar)
        {
            Name = jar.Name;
            References = jar.References;
            Clusters = new List<Cluster>();
            Machines = new List<Machine>();
            Defaults = new Configuration(jar.Defaults);
            reservedKeys = new List<string>() { Machine.ClusterKey };
        }

        public Box(Box box)
        {
            Name = box.Name;
            References = new List<Dictionary<string, string>>(box.References.Select(x => new Dictionary<string, string>(x)));
            Clusters = new List<Cluster>(box.Clusters);
            Machines = new List<Machine>(box.Machines);
            Defaults = new Configuration(box.Defaults.AsDictionary());
            reservedKeys = new List<string>(box.reservedKeys);
        }

        public string Name { get; private set; }

        public Configuration Defaults { get; set; }

        public List<Cluster> Clusters { get; set; }

        public List<Dictionary<string, string>> References { get; set; }

        public void Merge(Box box)
        {
            Defaults = Defaults.Join(box.Defaults);
            Clusters = Clusters.Merge(box.Clusters).ToList();
            Machines = Machines.Merge(box.Machines).ToList();
        }

        public void AddCluster(string name, Dictionary<string, string> settings)
        {
            var cluster = new Cluster(name, settings);
            AddCluster(cluster);
        }

        public void AddCluster(Cluster cluster)
        {
            Guard_SettingMustBeDefinedInDefaults(cluster.AsDictionary());

            if (!Clusters.Contains(cluster))
                Clusters.Add(cluster);
        }

        public List<Machine> Machines { get; set; }

        public void AddMachine(string name, Dictionary<string, string> settings)
        {
            var machine = new Machine(name, settings);
            AddMachine(machine);
        }

        public void AddMachine(Machine machine)
        {
            Guard_SettingMustBeDefinedInDefaults(machine.AsDictionary());
            Guard_MachineClusterConfiguration(machine);

            if (!Machines.Contains(machine))
                Machines.Add(machine);
        }

        private void Guard_SettingMustBeDefinedInDefaults(Dictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                Guard_SettingMustBeDefinedInDefaults(setting.Key);
            }
        }

        private void Guard_MachineClusterConfiguration(Machine machine)
        {
            if (machine.ContainsKey(Machine.ClusterKey))
            {
                var clusterName = machine[Machine.ClusterKey];
                var isValid = Clusters.Any(x => x.Name == clusterName);
                if (isValid == false)
                    throw new ArgumentException(string.Format("Invalid machine configuration. The machine '{0}' is explicitly configured in cluster '{1}' but cluster configuration with that name does not exist.", machine.Name, clusterName));
            }
        }

        private void Guard_SettingMustBeDefinedInDefaults(string settingKey)
        {
            if (reservedKeys.Contains(settingKey))
                return;

            if (!Defaults.ContainsKey(settingKey))
                throw new ArgumentException(String.Format("The setting key '{0}' was not found in the Default settings for application '{1}'. You can override only settings inside the default settings", settingKey, Name));
        }

        /// <summary>
        /// According to the myth, Pandora opened a jar (pithos), in modern accounts sometimes mistranslated as "Pandora's box" (see below), 
        /// releasing all the evils of humanity—although the particular evils, aside from plagues and diseases, are not specified in detail 
        /// by Hesiod—leaving only Hope inside once she had closed it again. She opened the jar out of simple curiosity and not as a malicious act.
        /// </summary>
        /// <remarks>http://en.wikipedia.org/wiki/Pandora</remarks>
        /// <param name="jar"></param>
        /// <returns>Returns Pandora's box</returns>
        public static Box Mistranslate(Jar jar)
        {
            Box box = new Box(jar);

            if (jar.Clusters != null)
            {
                foreach (var cluster in jar.Clusters)
                {
                    box.AddCluster(cluster.Key, cluster.Value);
                }
            }

            if (jar.Machines != null)
            {
                foreach (var machine in jar.Machines)
                {
                    box.AddMachine(machine.Key, machine.Value);
                }
            }

            return box;
        }

        public static Jar Mistranslate(Box box)
        {
            Jar jar = new Jar();

            jar.Name = box.Name;
            jar.References = box.References;

            if (box.Defaults != null)
                jar.Defaults = box.Defaults.AsDictionary();

            if (box.Clusters != null)
            {
                foreach (var cluster in box.Clusters)
                {
                    jar.Clusters.Add(cluster.Name, cluster.AsDictionary());
                }
            }

            if (box.Machines != null)
            {
                foreach (var machine in box.Machines)
                {
                    jar.Machines.Add(machine.Name, machine.AsDictionary());
                }
            }

            return jar;
        }
    }
}