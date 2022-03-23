using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Pandora
{
    public class PandoraConfigurationProvider : ConfigurationProvider
    {
        private readonly Pandora pandora;

        public PandoraConfigurationProvider(IPandoraConfigurationSource source)
        {
            this.pandora = source.Pandora;

            if (source.ReloadOnChange)
            {
                ChangeToken.OnChange(() => source.ReloadWatcher.Watch(), Load);
            }
        }

        public override void Load()
        {
            List<DeployedSetting> newState = pandora.GetAll(pandora.ApplicationContext).ToList();

            Data = newState.ToDictionary(key => key.Key.SettingKey, value => value.Value, StringComparer.OrdinalIgnoreCase);
            Data.Add(EnvVar.ApplicationKey, pandora.ApplicationContext.ApplicationName);
            Data.Add(EnvVar.MachineKey, pandora.ApplicationContext.Machine);
            Data.Add(EnvVar.ClusterKey, pandora.ApplicationContext.Cluster);
        }
    }
}
