using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    public class DeployedSetting
    {
        public DeployedSetting(string raw, string applicationName, string cluster, string machine, string key, string value)
        {
            Raw = raw;
            ApplicationName = applicationName;
            Cluster = cluster;
            Machine = machine;
            Key = key;
            Value = value;
        }

        public string Raw { get; private set; }
        public string ApplicationName { get; private set; }
        public string Cluster { get; private set; }
        public string Machine { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}