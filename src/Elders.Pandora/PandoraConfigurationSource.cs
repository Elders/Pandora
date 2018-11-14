using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace Elders.Pandora
{
    public abstract class PandoraConfigurationSource : IPandoraConfigurationSource
    {
        public Pandora Pandora { get; set; }
        public bool ReloadOnChange { get; set; } = true;
        public int ReloadDelay { get; set; } = 250;
        public IChangeToken ChangeToken { get; set; }
        public Action<PandoraConfigurationProvider> ChangeTokenConsumer { get; set; }

        public abstract IConfigurationProvider Build(IConfigurationBuilder builder);
    }
}
