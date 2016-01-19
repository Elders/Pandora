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
    public class MachinesController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MachinesController));

        public Machine Get(string projectName, string configurationName, string clusterName, string machineName)
        {
            if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName) || string.IsNullOrWhiteSpace(machineName))
                return null;

            var projectPath = Path.Combine(Folders.Projects, projectName);

            var configurationPath = GetConfigurationFile(projectName, configurationName);

            var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

            var box = Box.Box.Mistranslate(cfg);

            return box.Machines.SingleOrDefault(x => x.Name == machineName);
        }

        public void Post(string projectName, string configurationName, string machineName, [FromBody]string value)
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

                var newMachine = new Machine(machineName, newConfig);

                var machines = box.Machines.ToList();

                if (!machines.Any(x => x.Name == newMachine.Name))
                {
                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added new machine " + newMachine.Name + " in " + configurationName + " in " + projectName;

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

        public void Put(string projectName, string configurationName, string machineName, [FromBody]string value)
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

                var newMachine = new Machine(machineName, newConfig);

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == newMachine.Name);

                if (existing != null)
                {
                    machines.Remove(existing);

                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated machine " + newMachine.Name + " in " + configurationName + " in " + projectName;

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

        public void Delete(string projectName, string configurationName, string machineName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configurationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(machineName))
                    return;

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == machineName);

                if (existing != null)
                {
                    machines.Remove(existing);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box));

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed machine " + existing.Name + " from " + configurationName + " in " + projectName;

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
