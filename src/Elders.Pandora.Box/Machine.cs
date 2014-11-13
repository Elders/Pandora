using System.Collections.Generic;

namespace Elders.Padnora.Box
{
    public sealed class Machine : Configuration
    {
        public Machine(string name, Dictionary<string, string> settings) : base(name, settings) { }
    }
}