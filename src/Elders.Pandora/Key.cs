using System;
using System.Text.RegularExpressions;

namespace Elders.Pandora
{
    public class Key
    {
        public Key(string applicationName, string cluster, string machine, string settingKey)
        {
            ApplicationName = applicationName;
            Cluster = cluster;
            Machine = machine;
            SettingKey = settingKey;
        }

        public string Raw { get; private set; }
        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }
        public string SettingKey { get; private set; }

        public static Key Parse(string rawKey)
        {
            if (string.IsNullOrEmpty(rawKey)) throw new ArgumentNullException(nameof(rawKey));

            var rawKeyPattern = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");

            var mappedKey = rawKeyPattern.Match(rawKey);
            if (mappedKey.Success)
            {
                return new Key(
                        applicationName: mappedKey.Groups[1].Value,
                        cluster: mappedKey.Groups[2].Value,
                        machine: mappedKey.Groups[3].Value,
                        settingKey: mappedKey.Groups[4].Value);
            }
            else
            {
                throw new ArgumentException($"Invalid Pandora key: {rawKey}", nameof(rawKey));
            }
        }
    }
}
