using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Elders.Pandora
{
    public class WindowsEnvironmentVariables : IConfigurationRepository
    {
        public void Delete(string key)
        {
            throw new NotSupportedException($"This operation is not supported for {nameof(WindowsEnvironmentVariables)}");
        }

        public bool Exists(string key)
        {
            var setting = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            return ReferenceEquals(null, setting) == false;
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

            var setting = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            if (setting == null)
                throw new KeyNotFoundException("Unable to find environment variable " + key);

            return setting;
        }

        public IEnumerable<DeployedSetting> GetAll()
        {
            var regex = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");

            var all = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);

            foreach (DictionaryEntry item in all)
            {
                var result = regex.Match(item.Key.ToString());
                if (result.Success)
                {
                    yield return new DeployedSetting(
                        raw: result.Groups[0].Value,
                        applicationName: result.Groups[1].Value,
                        cluster: result.Groups[2].Value,
                        machine: result.Groups[3].Value,
                        settingKey: result.Groups[4].Value,
                        value: Environment.GetEnvironmentVariable(result.Groups[0].Value, EnvironmentVariableTarget.Machine));
                }
            }
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

            if (string.IsNullOrEmpty(value) == false)
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Machine);
        }
    }
}
