using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Pandora.Box
{
    public class Configuration : ValueObject<Configuration>
    {
        private readonly Dictionary<string, object> settings;

        public Configuration(Dictionary<string, object> settings) : this("defaults", settings) { }

        public Configuration(string name, Dictionary<string, object> settings)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            if (settings != null)
                this.settings = settings.ToDictionary(key => key.Key.ToLower(), val => val.Value);
            else
                this.settings = new Dictionary<string, object>();

            this.Name = name;
        }

        public string Name { get; private set; }

        public object this[string settingName]
        {
            get
            {
                object value = null;
                if (settings.TryGetValue(settingName.ToLower(), out value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException("SettingName does not exist in the collection");
                }
            }
        }

        public Dictionary<string, object> AsDictionary()
        {
            return new Dictionary<string, object>(settings);
        }

        public bool ContainsKey(string key)
        {
            return settings.ContainsKey(key.ToLower());
        }

        public void DeleteKey(string key)
        {
            if (settings.ContainsKey(key.ToLower()))
                settings.Remove(key.ToLower());
        }
    }

    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Joins a configuration with a collection of configurations. If the collection configurations such does NOT contain
        /// a set of configurations such as 'Cluster' it will NOT appear within the result set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">The self.</param>
        /// <param name="configurationToJoin">The configuration to join.</param>
        /// <returns></returns>
        public static T Join<T>(this T self, T configurationToJoin) where T : Configuration
        {
            return self.Join(new List<T>() { configurationToJoin });
        }

        /// <summary>
        /// Joins a configuration with a collection of configurations. If the collection configurations such does NOT contain
        /// a set of configurations such as 'Cluster' it will NOT appear within the result set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">The self.</param>
        /// <param name="configurationsToJoin">The configurations to join.</param>
        /// <returns></returns>
        public static T Join<T>(this T self, IEnumerable<T> configurationsToJoin) where T : Configuration
        {
            var settings = self.AsDictionary();
            foreach (var cfgToJoin in configurationsToJoin)
            {
                if (self.Name != cfgToJoin.Name) continue;
                settings = settings.Union(cfgToJoin.AsDictionary()).ToDictionary(key => key.Key, val => val.Value);
            }

            var cfg = (T)Activator.CreateInstance(typeof(T), new object[] { self.Name, settings });
            return cfg;
        }

        /// <summary>
        /// Merges two collections of configurations. If one set of configurations such as 'Cluster' does NOT exists within
        /// one of the collections it WILL appear with the result set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">The self.</param>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<T> self, IEnumerable<T> other) where T : Configuration
        {
            var tracker = new HashSet<string>();

            foreach (var cfg in self)
            {
                tracker.Add(cfg.Name);
                yield return cfg.Join(other);
            }

            foreach (var cfg in other)
            {
                if (tracker.Contains(cfg.Name) == false)
                    yield return cfg.Join(self);
            }
        }
    }
}
