using Elders.Pandora.Box;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Elders.Pandora.UI.ViewModels
{
    public class Configuration
    {
        private Jar jar;

        public Configuration(Jar jar, string projectName)
        {
            this.jar = jar;
            SecurityAccess = GetSecurityAccess();
            ApplicationName = jar.Name;
            ProjectName = projectName;
        }

        public SecurityAccess SecurityAccess { get; set; }

        public string ProjectName { get; set; }

        public string ApplicationName { get; set; }

        public Dictionary<Cluster, Dictionary<string, string>> GetAllClusters()
        {
            if (!this.HasAccess())
                return new Dictionary<Cluster, Dictionary<string, string>>();

            var box = Box.Box.Mistranslate(jar);

            var pandora = new Pandora(box);

            var clusters = new Dictionary<Cluster, Dictionary<string, string>>();


            foreach (var env in SecurityAccess.Projects.SingleOrDefault(x => x.Name == this.ProjectName).Applications.SingleOrDefault(x => x.Name == ApplicationName).Clusters)
            {
                if (box.Clusters.Select(x => x.Name).Any(x => x == env.Name))
                {
                    clusters.Add(env, pandora.Open(new PandoraOptions(env.Name, string.Empty, true)).AsDictionary());
                }
            }

            return clusters;
        }

        public KeyValuePair<Cluster, Dictionary<string, string>> GetCluster(string clusterName)
        {
            if (!this.HasAccess(clusterName))
                return new KeyValuePair<Cluster, Dictionary<string, string>>();

            var box = Box.Box.Mistranslate(jar);

            var pandora = new Pandora(box);

            return new KeyValuePair<Cluster, Dictionary<string, string>>(SecurityAccess.Projects
                .SingleOrDefault(x => x.Name == this.ProjectName)
                .Applications
                .SingleOrDefault(x => x.Name == this.ApplicationName)
                .Clusters
                .SingleOrDefault(x => x.Name == clusterName), pandora.Open(new PandoraOptions(clusterName, string.Empty, true)).AsDictionary());
        }

        public Dictionary<string, Dictionary<string, string>> GetAllMachines(string clusterName)
        {
            if (!this.HasAccess(clusterName))
                return new Dictionary<string, Dictionary<string, string>>();

            var box = Box.Box.Mistranslate(jar);

            var pandora = new Pandora(box);

            var machines = new Dictionary<string, Dictionary<string, string>>();

            foreach (var machine in box.Machines)
            {
                machines.Add(machine.Name, pandora.Open(new PandoraOptions(clusterName, machine.Name, true)).AsDictionary());
            }

            return machines;
        }

        public KeyValuePair<string, Dictionary<string, string>> GetMachine(string clusterName, string machineName)
        {
            if (!this.HasAccess(clusterName))
                return new KeyValuePair<string, Dictionary<string, string>>();

            var box = Box.Box.Mistranslate(jar);

            var pandora = new Pandora(box);

            return new KeyValuePair<string, Dictionary<string, string>>(machineName, pandora.Open(new PandoraOptions(clusterName, machineName, true)).AsDictionary());
        }

        public KeyValuePair<Application, Dictionary<string, string>> GetDefaults()
        {
            if (!this.HasAccess())
                return new KeyValuePair<Application, Dictionary<string, string>>();

            var app = SecurityAccess.Projects.SingleOrDefault(x => x.Name == this.ProjectName).Applications.SingleOrDefault(x => x.Name == this.ApplicationName);

            if (!app.Access.HasAccess(Access.ReadAcccess))
                return new KeyValuePair<Application, Dictionary<string, string>>();

            var box = Box.Box.Mistranslate(jar);

            return new KeyValuePair<Application, Dictionary<string, string>>(app, box.Defaults.AsDictionary());
        }

        private SecurityAccess GetSecurityAccess()
        {
            var securityAccessClaim = ClaimsPrincipal.Current.Claims.SingleOrDefault(x => x.Type == "SecurityAccess");

            SecurityAccess security;

            if (securityAccessClaim == null || string.IsNullOrWhiteSpace(securityAccessClaim.Value))
                security = new SecurityAccess();
            else
                security = JsonConvert.DeserializeObject<SecurityAccess>(securityAccessClaim.Value);

            if (security == null)
                security = new SecurityAccess();

            return security;
        }

        public bool HasAccess()
        {
            if (SecurityAccess.Projects.Any(x => x.Name == this.ProjectName) && SecurityAccess.Projects.SingleOrDefault(x => x.Name == this.ProjectName).Applications.Any(x => x.Name == this.ApplicationName))
                return true;

            else
                return false;
        }

        public bool HasAccess(string cluster)
        {
            if (this.HasAccess())
            {
                if (SecurityAccess
                    .Projects.SingleOrDefault(x => x.Name == this.ProjectName)
                    .Applications.SingleOrDefault(x => x.Name == this.ApplicationName)
                    .Clusters.Any(x => x.Name == cluster))
                    return true;

                else
                    return false;
            }
            else
                return false;
        }
    }
}