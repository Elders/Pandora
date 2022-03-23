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
        TimeSpan ReloadDelay { get; set; }

        /// <summary>
        /// A token which tracks if there are configuration source changes.
        /// </summary>
        IChangeToken ChangeToken { get; set; }

        /// <summary>
        /// Most probably this is used when a reload happens. You need to tripple check.
        /// </summary>
        Action<PandoraConfigurationProvider> ChangeTokenConsumer { get; set; }

        /// <summary>
        /// A trigger notification that the configuration source has changed and a reload is required.
        /// </summary>
        IPandoraWatcher ReloadWatcher { get; set; }
    }
}
