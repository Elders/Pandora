using Elders.Pandora.Box;
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

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        public List<Cluster> Get(string projectName, string applicationName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return new List<Cluster>();

                var cfgPath = Path.Combine(storageFolder, projectName, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

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

                var cfgPath = Path.Combine(storageFolder, projectName, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

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

                var workingDir = Path.Combine(storageFolder, projectName);

                var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var newCluster = JsonConvert.DeserializeObject<Cluster>(value);

                var clusters = box.Clusters.ToList();

                if (!clusters.Any(x => x.Name == newCluster.Name))
                {
                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box));

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added cluster " + newCluster.Name + " in " + applicationName + " in project " + projectName;

                    var git = new Git(workingDir);
                    git.Stage(new List<string>() { cfgPath });
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

                var workingDir = Path.Combine(storageFolder, projectName);

                var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var newCluster = JsonConvert.DeserializeObject<Cluster>(value);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == newCluster.Name);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    clusters.Add(newCluster);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated cluster " + newCluster.Name + " in " + applicationName + " in project " + projectName;

                    var git = new Git(workingDir);
                    git.Stage(new List<string>() { cfgPath });
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

                var workingDir = Path.Combine(storageFolder, projectName);

                var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var clusters = box.Clusters.ToList();

                var existing = clusters.FirstOrDefault(x => x.Name == clusterName);

                if (existing != null)
                {
                    clusters.Remove(existing);

                    box.Clusters = clusters;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed cluster " + clusterName + " from " + applicationName + " in " + projectName;

                    var git = new Git(workingDir);
                    git.Stage(new List<string>() { cfgPath });
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
