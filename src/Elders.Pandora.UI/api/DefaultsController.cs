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
    public class DefaultsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultsController));

        public Configuration Get(string projectName, string applicationName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName))
                    return null;

                var applicationPath = Path.Combine(Folders.Projects, projectName, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

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

                var applicationPath = Path.Combine(Folders.Projects, projectName, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var setting = box.Defaults.AsDictionary().FirstOrDefault(x => x.Key == defaultName);

                if (string.IsNullOrWhiteSpace(setting.Value))
                    throw new InvalidOperationException("There is no default setting with name " + defaultName);
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

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var defaults = box.Defaults.AsDictionary();

                if (!defaults.ContainsKey(setting.Key))
                {
                    defaults.Add(setting.Key, setting.Value);

                    box.Defaults = new Configuration(box.Defaults.Name, defaults);

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added setting " + setting.Key + " in " + applicationName + " in " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configPath });
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

        public void Put(string projectName, string applicationName, [FromBody]Dictionary<string, string> settings)
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

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                box.Defaults = new Configuration(box.Defaults.Name, settings);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(configPath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Updated default settings in " + applicationName + " in " + projectName;

                var git = new Git(projectPath);
                git.Stage(new List<string>() { configPath });
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

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationName);

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationName);

                var configPath = files.First();

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));

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

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed setting " + key + " from " + applicationName + " in " + projectName;

                    var git = new Git(projectPath);
                    git.Stage(new List<string>() { configPath });
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
