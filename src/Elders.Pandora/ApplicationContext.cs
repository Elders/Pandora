using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    public class ApplicationContext
    {
        public ApplicationContext(string applicationName, string cluster = null, string machine = null)
        {
            this.ApplicationName = applicationName;
            this.Cluster = cluster ?? Environment.GetEnvironmentVariable("CLUSTER_NAME", EnvironmentVariableTarget.Machine);
            this.Machine = machine ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }

        public string ApplicationName { get; private set; }

        public string Cluster { get; private set; }

        public string Machine { get; private set; }
    }
}