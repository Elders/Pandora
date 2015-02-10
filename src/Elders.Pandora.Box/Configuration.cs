using System;
using System.Collections.Generic;

namespace Elders.Pandora.Box
{
    public class Configuration : ValueObject<Configuration>
    {
        public Dictionary<string, string> Settings { get; private set; }

        public Configuration(Dictionary<string, string> settings) : this("defaults", settings) { }

        public Configuration(string name, Dictionary<string, string> settings)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (settings != null)
                this.Settings = new Dictionary<string, string>(settings);
            else
                this.Settings = new Dictionary<string, string>();

            this.Name = name;
        }

        public string Name { get; private set; }

        public string this[string settingName]
        {
            get
            {
                string value = String.Empty;
                if (Settings.TryGetValue(settingName, out value))
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
            return new Dictionary<string, string>(Settings);
        }

        public bool ContainsKey(string key)
        {
            return Settings.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            if (Settings.ContainsKey(key))
                Settings.Remove(key);
        }
    }
}