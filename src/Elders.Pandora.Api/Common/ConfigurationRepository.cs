using System;
using System.IO;

namespace Elders.Pandora.Api.Common
{
    public static class ConfigurationRepository
    {
        public static string GetConfigurationFile(string projectName, string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName))
                return null;

            var configurationPath = Path.Combine(Folders.Projects, projectName, "src", projectName + ".Configuration", "public", configurationName);

            if (configurationPath.EndsWith(".json", StringComparison.Ordinal) == false)
                configurationPath += ".json";

            if (File.Exists(configurationPath) == false)
                throw new InvalidOperationException("There is no configuration file: " + configurationName);

            return configurationPath;
        }
    }
}