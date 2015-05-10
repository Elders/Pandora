using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Pandora.Box
{
    public class Configuration : ValueObject<Configuration>
    {
        private readonly Dictionary<string, string> settings;

        public Configuration(Dictionary<string, string> settings) : this("defaults", settings) { }

        public Configuration(string name, Dictionary<string, string> settings)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (settings != null)
                this.settings = settings.ToDictionary(key => key.Key.ToLowerInvariant(), val => val.Value);
            else
                this.settings = new Dictionary<string, string>();

            this.Name = name;
        }

        public string Name { get; private set; }

        public string this[string settingName]
        {
            get
            {
                string value = String.Empty;
                if (settings.TryGetValue(settingName.ToLowerInvariant(), out value))
                {
                    return value;
                }
                else
                {
                    throw new System.Collections.Generic.KeyNotFoundException("SettingName does not exist in the collection");
                }
            }
        }

        public Dictionary<string, string> AsDictionary()
        {
            return new Dictionary<string, string>(settings);
        }

        public bool ContainsKey(string key)
        {
            return settings.ContainsKey(key.ToLowerInvariant());
        }

        public void DeleteKey(string key)
        {
            if (settings.ContainsKey(key.ToLowerInvariant()))
                settings.Remove(key.ToLowerInvariant());
        }
    }

    public static class ConfigurationExtensions
    {
        public static Configuration Join(this Configuration self, Configuration configurationToJoin)
        {
            return self.Join(new List<Configuration>() { configurationToJoin });
        }

        public static Configuration Join(this Configuration self, IEnumerable<Configuration> configurationsToJoin)
        {
            var settings = self.AsDictionary();
            foreach (var cfgToJoin in configurationsToJoin)
            {
                if (self.Name != cfgToJoin.Name) continue;
                settings = settings.Union(cfgToJoin.AsDictionary()).ToDictionary(key => key.Key, val => val.Value);
            }

            var cfg = new Configuration(self.Name, settings);
            return cfg;
        }
    }
}