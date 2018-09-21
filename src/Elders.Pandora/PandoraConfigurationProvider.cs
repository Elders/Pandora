using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Elders.Pandora
{
    public class PandoraConfigurationProvider : ConfigurationProvider
    {
        private readonly Pandora pandora;

        public PandoraConfigurationProvider(Pandora pandora)
        {
            this.pandora = pandora;
        }

        public override void Load()
        {
            Data = pandora.GetAll().ToDictionary(key => key.Key.SettingKey, value => value.Value);
        }
    }
}
