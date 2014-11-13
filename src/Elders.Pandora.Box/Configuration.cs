using System;
using System.Collections.Generic;

namespace Elders.Padnora.Box
{
    public class Configuration : ValueObject<Configuration>
    {
        private readonly Dictionary<string, string> settings;

        public Configuration(Dictionary<string, string> settings) : this("defaults", settings) { }

        public Configuration(string name, Dictionary<string, string> settings)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (settings == null || settings.Count == 0) throw new ArgumentNullException("name");

            this.settings = new Dictionary<string, string>(settings);
            this.Name = name;
        }

        public string Name { get; private set; }

        public string this[string settingName]
        {
            get
            {
                string value = String.Empty;
                if (settings.TryGetValue(settingName, out value))
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
            return settings.ContainsKey(key);
        }
    }
}