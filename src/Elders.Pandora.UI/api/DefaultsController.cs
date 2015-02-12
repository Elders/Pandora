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
    public class DefaultsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultsController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        public Configuration Get(string projectName, string applicationName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return null;

                var cfgPath = Path.Combine(storageFolder, projectName, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                return box.Defaults;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        public string Get(string projectName, string applicationName, string defaultName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(defaultName))
                    return null;

                var cfgPath = Path.Combine(storageFolder, projectName, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var setting = box.Defaults.AsDictionary().FirstOrDefault(x => x.Key == defaultName);

                if (string.IsNullOrWhiteSpace(setting.Value))
                    throw new ArgumentNullException("There is no default setting with name " + defaultName);
                else
                    return setting.Value;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        public void Post(string projectName, string applicationName, [FromBody]KeyValuePair<string, string> setting)
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

                var defaults = box.Defaults.AsDictionary();

                if (!box.Defaults.ContainsKey(setting.Key))
                {
                    defaults.Add(setting.Key, setting.Value);

                    box.Defaults = new Configuration(box.Defaults.Name, defaults);

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added setting " + setting.Key + " in " + applicationName + " in " + projectName;

                    var git = new Git(workingDir);
                    git.Stage(new List<string>() { cfgPath });
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

        public void Put(string projectName, string applicationName, [FromBody]Dictionary<string, string> defaults)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || defaults == null || defaults.Count == 0)
                    return;

                var workingDir = Path.Combine(storageFolder, projectName);

                var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);


                box.Defaults = new Configuration(box.Defaults.Name, defaults);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(cfgPath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Updated default settings for " + applicationName + " in " + projectName;

                var git = new Git(workingDir);
                git.Stage(new List<string>() { cfgPath });
                git.Commit(message, username, email);
                git.Push();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        public void Delete(string projectName, string applicationName, string key)
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

                var defaults = box.Defaults.AsDictionary();

                if (defaults.ContainsKey(key))
                {
                    defaults.Remove(key);

                    box.Defaults = new Configuration(box.Defaults.Name, defaults);

                    foreach (var cluster in box.Clusters)
                    {
                        cluster.DeleteKey(key);
                    }

                    foreach (var machine in box.Machines)
                    {
                        machine.DeleteKey(key);
                    }

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box));

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed setting " + key + " from " + applicationName + " in " + projectName;

                    var git = new Git(workingDir);
                    git.Stage(new List<string>() { cfgPath });
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
    }
}
