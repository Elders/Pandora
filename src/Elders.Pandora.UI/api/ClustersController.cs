using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace Elders.Pandora.UI.api
{
    public class ClustersController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClustersController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        // GET api/clusters
        public IEnumerable<KeyValuePair<string, string>> Get(string appName, string clusterName)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var pandora = new Pandora(box).Open(clusterName);

                return pandora.AsDictionary();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // POST api/clusters
        public void Post(string appName, [FromBody]string value)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // PUT api/clusters/appName
        public void Put(string appName, [FromBody]string value)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // DELETE api/clusters/appName/clusterName
        public void Delete(string appName, string clusterName)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
