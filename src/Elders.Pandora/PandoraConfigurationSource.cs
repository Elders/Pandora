using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace Elders.Pandora
{
    public abstract class PandoraConfigurationSource : IPandoraConfigurationSource
    {
        public Pandora Pandora { get; set; }
        public bool ReloadOnChange { get; set; } = true;
        public TimeSpan ReloadDelay { get; set; } = TimeSpan.FromMinutes(1);
        public IChangeToken ChangeToken { get; set; }
        public Action<PandoraConfigurationProvider> ChangeTokenConsumer { get; set; }
        public abstract IPandoraWatcher ReloadWatcher { get; set; }

        public abstract IConfigurationProvider Build(IConfigurationBuilder builder);
    }
}
