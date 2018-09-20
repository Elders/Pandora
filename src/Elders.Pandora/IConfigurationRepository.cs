using System;
using System.Collections.Generic;

namespace Elders.Pandora
{
    public interface IConfigurationRepository
    {
        string Get(string key);
        void Set(string key, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine);
        void Delete(string key);
        IEnumerable<DeployedSetting> GetAll(EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine);
        bool Exists(string key);
    }
}
