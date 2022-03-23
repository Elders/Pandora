using Microsoft.Extensions.Primitives;
using System;

namespace Elders.Pandora
{
    public interface IPandoraWatcher : IDisposable
    {
        IChangeToken Watch();
    }
}
