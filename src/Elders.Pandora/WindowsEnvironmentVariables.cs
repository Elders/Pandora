using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elders.Pandora
{
    public class WindowsEnvironmentVariables : IConfigurationRepository
    {
        private readonly EnvironmentVariableTarget target;

        public WindowsEnvironmentVariables(EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            this.target = target;
        }

        public Task DeleteAsync(string key)
        {
            throw new NotSupportedException($"This operation is not supported for {nameof(WindowsEnvironmentVariables)}");
        }

        public Task<bool> ExistsAsync(string key)
        {
            var setting = Environment.GetEnvironmentVariable(key) ?? Environment.GetEnvironmentVariable(key, target);
            return Task.FromResult(setting is not null);
        }

        public Task<string> GetAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

            var setting = Environment.GetEnvironmentVariable(key) ?? Environment.GetEnvironmentVariable(key, target);
            if (setting == null)
                throw new KeyNotFoundException("Unable to find environment variable " + key);

            return Task.FromResult(setting);
        }

        public IEnumerable<DeployedSetting> GetAll(IPandoraContext context)
        {
            var regex = new Regex(@"([^@]+)@@([^\^]+)\^([^~]+)~~(.+)");

            var all = Environment.GetEnvironmentVariables(target);

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
                        value: Environment.GetEnvironmentVariable(result.Groups[0].Value, target));
                }
            }
        }

        public Task SetAsync(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

            if (string.IsNullOrEmpty(value) == false)
                Environment.SetEnvironmentVariable(key, value, target);

            return Task.CompletedTask;
        }
    }
}
