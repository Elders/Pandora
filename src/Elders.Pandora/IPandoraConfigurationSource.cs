using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace Elders.Pandora
{
    public interface IPandoraConfigurationSource : IConfigurationSource
    {
        Pandora Pandora { get; }

        /// <summary>
        /// Determines whether the source will be loaded if the underlying file changes.
        /// </summary>
        bool ReloadOnChange { get; set; }

        /// <summary>
        /// Number of milliseconds that reload will wait before calling Load.  This helps
        /// avoid triggering reload before a file is completely written. Default is 250.
        /// </summary>
        int ReloadDelay { get; set; }

        IChangeToken ChangeToken { get; set; }

        Action<PandoraConfigurationProvider> ChangeTokenConsumer { get; set; }
    }
}
