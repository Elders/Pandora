using Elders.Pandora.Box;
using Elders.Pandora.UI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public Machine Get(string projectName, string applicationName, string clusterName, string machineName)
        {
            if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName) || string.IsNullOrWhiteSpace(machineName))
                return null;

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

            return box.Machines.SingleOrDefault(x => x.Name == machineName);
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

                var newMachine = JsonConvert.DeserializeObject<Machine>(value);

                var machines = box.Machines.ToList();

                if (!machines.Any(x => x.Name == newMachine.Name))
                {
                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added new machine " + newMachine.Name + " in " + applicationName + " in " + projectName;

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

                var newMachine = JsonConvert.DeserializeObject<Machine>(value);

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == newMachine.Name);

                if (existing != null)
                {
                    machines.Remove(existing);

                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated machine " + newMachine.Name + " in " + applicationName + " in " + projectName;

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

        public void Delete(string projectName, string applicationName, string machineName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(machineName))
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

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == machineName);

                if (existing != null)
                {
                    machines.Remove(existing);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box));

                    File.WriteAllText(configPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed machine " + existing.Name + " from " + applicationName + " in " + projectName;

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
