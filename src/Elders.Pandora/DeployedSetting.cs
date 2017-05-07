using System;
using System.Text.RegularExpressions;

namespace Elders.Pandora
{
    public class DeployedSetting
    {
        public DeployedSetting(string raw, string applicationName, string cluster, string machine, string key, string value)
        {
            Raw = raw;
            ApplicationName = applicationName;
            Cluster = cluster;
            Machine = machine;
            Key = key;
            Value = value;
        }

        public string Raw { get; private set; }
        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }

        public static DeployedSetting Parse(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

            var regexMachine = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");
            var regexCluster = new Regex(@"([^@]+)@@([^\^]+)~~(.+)");

            var mappedKey = regexMachine.Match(key);
            if (mappedKey.Success)
            {
                return new DeployedSetting(
                        raw: mappedKey.Groups[0].Value,
                        applicationName: mappedKey.Groups[1].Value,
                        cluster: mappedKey.Groups[2].Value,
                        machine: mappedKey.Groups[3].Value,
                        key: mappedKey.Groups[4].Value,
                        value: value);
            }
            else
            {
                mappedKey = regexCluster.Match(key);

                return new DeployedSetting(
                       raw: mappedKey.Groups[0].Value,
                       applicationName: mappedKey.Groups[1].Value,
                       cluster: mappedKey.Groups[2].Value,
                       machine: Elders.Pandora.Box.Machine.NotSpecified,
                       key: mappedKey.Groups[3].Value,
                       value: value);
            }
        }
    }
}
