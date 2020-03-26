using Elders.Pandora.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Elders.Pandora
{
    public class PandoraConfigurationProvider : ConfigurationProvider
    {
        private readonly IPandoraConfigurationSource pandoraConfigurationSource;
        private readonly Pandora pandora;

        public PandoraConfigurationProvider(IPandoraConfigurationSource source)
        {
            this.pandoraConfigurationSource = source;
            this.pandora = source.Pandora;

            if (source.ReloadOnChange)
            {
                ChangeToken.OnChange(
                    () => source.ChangeToken ?? GetReloadToken(),
                    () =>
                    {
                        if (source.ChangeTokenConsumer is null)
                        {
                            Thread.Sleep(source.ReloadDelay);
                            Load(reload: true);
                        }
                        else
                        {
                            source.ChangeTokenConsumer(this);
                        }
                    });
            }
        }

        public override void Load()
        {
            Load(reload: false);
        }

        List<DeployedSetting> currentState = new List<DeployedSetting>();

        public void Load(bool reload)
        {
            if (reload || currentState is null || currentState.Any() == false)
            {
                List<DeployedSetting> newState = pandora.GetAll(pandora.ApplicationContext).ToList();
                if (newState.Any())
                    currentState = newState;
            }
            Data = currentState.ToDictionary(key => key.Key.SettingKey, value => value.Value);
            Data.Add(EnvVar.ApplicationKey, pandora.ApplicationContext.ApplicationName);
            Data.Add(EnvVar.MachineKey, pandora.ApplicationContext.Machine);
            Data.Add(EnvVar.ClusterKey, pandora.ApplicationContext.Cluster);

            OnReload();
        }
    }
}
