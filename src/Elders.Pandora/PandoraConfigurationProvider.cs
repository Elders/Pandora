using Elders.Pandora.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Elders.Pandora
{
    public class PandoraConfigurationProvider : ConfigurationProvider
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(PandoraConfigurationProvider));

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

        IEnumerable<DeployedSetting> currentState = null;

        public void Load(bool reload)
        {
            if (reload || currentState is null || currentState.Any() == false)
            {
                log.Debug(() => $"Reloading Pandora configuration source {pandoraConfigurationSource.GetType().Name} | Reload: {reload} | CurrentStateCount: {currentState?.Count()}");
                currentState = pandora.GetAll(pandora.ApplicationContext);
            }
            Data = currentState.ToDictionary(key => key.Key.SettingKey, value => value.Value);
            Data.Add(EnvVar.ApplicationKey, pandora.ApplicationContext.ApplicationName);
            Data.Add(EnvVar.MachineKey, pandora.ApplicationContext.Machine);
            Data.Add(EnvVar.ClusterKey, pandora.ApplicationContext.Cluster);

            OnReload();
        }
    }
}
