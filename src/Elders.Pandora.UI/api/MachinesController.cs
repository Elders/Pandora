using Elders.Pandora.Box;
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

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        public Machine Get(string projectName, string applicationName, string clusterName, string machineName)
        {
            if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(clusterName) || string.IsNullOrWhiteSpace(machineName))
                return null;

            var workingDir = Path.Combine(storageFolder, projectName);

            var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

            var exists = File.Exists(cfgPath);

            if (!exists)
                throw new ArgumentException("There is no configuration for application " + applicationName);

            var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

            var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

            return box.Machines.SingleOrDefault(x => x.Name == machineName);
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

                var newMachine = JsonConvert.DeserializeObject<Machine>(value);

                var machines = box.Machines.ToList();

                if (!machines.Any(x => x.Name == newMachine.Name))
                {
                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added new machine " + newMachine.Name + " in " + applicationName + " in " + projectName;

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

                var newMachine = JsonConvert.DeserializeObject<Machine>(value);

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == newMachine.Name);

                if (existing != null)
                {
                    machines.Remove(existing);

                    machines.Add(newMachine);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Updated machine " + newMachine.Name + " in " + applicationName + " in " + projectName;

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

        public void Delete(string projectName, string applicationName, string machineName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationName) || string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(machineName))
                    return;

                var workingDir = Path.Combine(storageFolder, projectName);

                var cfgPath = Path.Combine(workingDir, applicationName, applicationName += ".json");

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + applicationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var machines = box.Machines.ToList();

                var existing = machines.FirstOrDefault(x => x.Name == machineName);

                if (existing != null)
                {
                    machines.Remove(existing);

                    box.Machines = machines;

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box));

                    File.WriteAllText(cfgPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Removed machine " + existing.Name + " from " + applicationName + " in " + projectName;

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
