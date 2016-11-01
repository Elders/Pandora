using System;
using System.Collections.Generic;

namespace Elders.Pandora
{
    public class WindowsEnvironmentVariables : IConfigurationRepository
    {
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));

            var setting = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            if (setting == null)
                throw new KeyNotFoundException("Unable to find environment variable " + key);

            return setting;
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(nameof(value));

            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Machine);
        }
    }
}
