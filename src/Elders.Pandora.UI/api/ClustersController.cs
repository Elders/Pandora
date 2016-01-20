using Elders.Pandora.Box;
using Elders.Pandora.UI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Elders.Pandora.UI.api
{
    [Authorize]
    public class ClustersController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClustersController));

        public List<Cluster> Get(string projectName, string configurationName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName))
                    return new List<Cluster>();

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                return box.Clusters;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        public Cluster Get(string projectName, string configurationName, string clusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName))
                    return null;

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var cluster = box.Clusters.SingleOrDefault(x => x.Name == clusterName);

                if (cluster == null)
                    return null;

                return cluster;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        public void Post(string projectName, string configurationName, string clusterName, [FromBody]string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var newConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

                var newCluster = new Cluster(clusterName, newConfig);

                var clusters = box.Clusters.ToList();

                if (!clusters.Any(x => x.Name == newCluster.Name))
                {
                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box));

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added cluster " + newCluster.Name + " in " + configurationName + " in project " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configurationPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        public void Put(string projectName, string configurationName, string clusterName, [FromBody]string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var newConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

                var newCluster = new Cluster(clusterName, newConfigs);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == newCluster.Name);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated cluster " + newCluster.Name + " in " + configurationName + " in project " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configurationPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        public void Delete(string projectName, string configurationName, string clusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == clusterName);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed cluster " + clusterName + " from " + configurationName + " in " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configurationPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        private string GetConfigurationFile(string projectName, string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName))
                return null;

            var configurationPath = Path.Combine(Folders.Projects, projectName, "src", projectName + ".Configuration", "public", configurationName);

            if (configurationPath.EndsWith(".json", StringComparison.Ordinal) == false)
                configurationPath += ".json";

            if (System.IO.File.Exists(configurationPath) == false)
                throw new InvalidOperationException("There is no configuration file: " + configurationName);

            return configurationPath;
        }
    }
}
