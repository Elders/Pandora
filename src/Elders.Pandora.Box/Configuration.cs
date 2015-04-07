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
}