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
    //[Authorize]
    public class ClustersController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClustersController));

        public List<Cluster> Get(string projectName, string applicationName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return new List<Cluster>();

                var applicationPath = Path.Combine(Folders.Projects, projectName, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Box.Box.Mistranslate(cfg);

                return box.Clusters;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public Cluster Get(string projectName, string applicationName, string clusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName))
                    return null;

                var applicationPath = Path.Combine(Folders.Projects, projectName, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Box.Box.Mistranslate(cfg);

                var cluster = box.Clusters.SingleOrDefault(x => x.Name == clusterName);

                if (cluster == null)
                    return null;

                return cluster;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Post(string projectName, string applicationName, [FromBody]string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Box.Box.Mistranslate(cfg);

                var newCluster = JsonConvert.DeserializeObject<Cluster>(value);

                var clusters = box.Clusters.ToList();

                if (!clusters.Any(x => x.Name == newCluster.Name))
                {
                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box));

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added cluster " + newCluster.Name + " in " + applicationName + " in project " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Put(string projectName, string applicationName, [FromBody]string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Box.Box.Mistranslate(cfg);

                var newCluster = JsonConvert.DeserializeObject<Cluster>(value);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == newCluster.Name);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated cluster " + newCluster.Name + " in " + applicationName + " in project " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Delete(string projectName, string applicationName, string clusterName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Box.Box.Mistranslate(cfg);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == clusterName);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed cluster " + clusterName + " from " + applicationName + " in " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configPath });
                    git.Commit(message, username, email);
                    git.Push();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }
    }
}
