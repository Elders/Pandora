using System;
using System.Collections.Generic;

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
        private readonly List<Cluster> clusters;
        private readonly List<Machine> machines;
        private readonly Configuration defaults;

        public Box(string applicationName, Dictionary<string, string> defaultSettings)
        {
            Name = applicationName;
            clusters = new List<Cluster>();
            machines = new List<Machine>();
            defaults = new Configuration(defaultSettings);
        }

        public string Name { get; private set; }

        public Configuration Defaults { get { return defaults; } }

        public IEnumerable<Cluster> Clusters { get { return clusters.AsReadOnly(); } }

        public void AddCluster(string name, Dictionary<string, string> settings)
        {
            var cluster = new Cluster(name, settings);
            AddCluster(cluster);
        }

        public void AddCluster(Cluster cluster)
        {
            Guard_SettingMustBeDefinedInDefaults(cluster.AsDictionary());

            if (!clusters.Contains(cluster))
                clusters.Add(cluster);
        }


        public IEnumerable<Machine> Machines { get { return machines.AsReadOnly(); } }

        public void AddMachine(string name, Dictionary<string, string> settings)
        {
            var machine = new Machine(name, settings);
            AddMachine(machine);
        }

        public void AddMachine(Machine machine)
        {
            Guard_SettingMustBeDefinedInDefaults(machine.AsDictionary());

            if (!machines.Contains(machine))
                machines.Add(machine);
        }

        private void Guard_SettingMustBeDefinedInDefaults(Dictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                Guard_SettingMustBeDefinedInDefaults(setting.Key);
            }
        }

        private void Guard_SettingMustBeDefinedInDefaults(string settingKey)
        {
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
            Box box = new Box(jar.Name, jar.Defaults);

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
    }
}