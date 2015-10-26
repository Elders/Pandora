using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Elders.Pandora.Box;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace Elders.Pandora.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ReferencesController : Controller
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(JarsController));

        [HttpGet("{projectName}/{configurationName}")]
        public IEnumerable<string> Get(string projectName, string configurationName)
        {
            try
            {
                var projectPath = Path.Combine(Folders.Projects, projectName);

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                var jar = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var references = new List<string>();

                foreach (var reference in jar.References)
                {
                    references.Add(reference.Values.First());
                }

                return references;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        [HttpPost("{projectName}/{configurationName}")]
        public void Post(string projectName, string configurationName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(configurationName))
                    throw new InvalidOperationException();

                var projectPath = Path.Combine(Folders.Projects, projectName);

                if (configurationName.EndsWith(".json", StringComparison.Ordinal) == false)
                    configurationName += ".json";

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                if (System.IO.File.Exists(configurationPath))
                    throw new InvalidOperationException("There is already a configuration file: " + configurationName);

                var box = Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                System.IO.File.WriteAllText(configurationPath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Added new configuration: " + cfg.Name + " in " + projectName;

                var git = new Git(projectPath);
                git.Stage(new List<string>() { configurationPath });
                git.Commit(message, username, email);
                git.Push();

                //MvcApplication.TcpServer.SendToAllClients(Encoding.UTF8.GetBytes(jar));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        [HttpPut("{projectName}/{configurationName}")]
        public void Put(string projectName, string configurationName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(configurationName))
                    throw new InvalidOperationException();

                var projectPath = Path.Combine(Folders.Projects, projectName);

                if (configurationName.EndsWith(".json", StringComparison.Ordinal) == false)
                    configurationName += ".json";

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                if (System.IO.File.Exists(configurationPath) == false)
                    throw new InvalidOperationException("There is no configuration file: " + configurationName);

                var box = Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                System.IO.File.WriteAllText(configurationPath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Added new application configuration: " + cfg.Name + " in " + projectName;

                var git = new Git(projectPath);
                git.Stage(new List<string>() { configurationPath });
                git.Commit(message, username, email);
                git.Push();

                //MvcApplication.TcpServer.SendToAllClients(Encoding.UTF8.GetBytes(jar));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        [HttpDelete("{projectName}/{configurationName}/{referenceName}")]
        public void Delete(string projectName, string configurationName, string referenceName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(configurationName))
                    throw new InvalidOperationException();

                var projectPath = Path.Combine(Folders.Projects, projectName);

                if (configurationName.EndsWith(".json", StringComparison.Ordinal) == false)
                    configurationName += ".json";

                var configurationPath = GetConfigurationFile(projectName, configurationName);

                if (System.IO.File.Exists(configurationPath) == false)
                    throw new InvalidOperationException("There is no configuration file: " + configurationName);

                var cfg = JsonConvert.DeserializeObject<Jar>(System.IO.File.ReadAllText(configurationPath));

                var box = Box.Box.Mistranslate(cfg);

                var referenceForDelete = box.References.FirstOrDefault(x => x.ContainsValue(referenceName));

                if (referenceForDelete != null)
                {
                    box.References.Remove(referenceForDelete);

                    var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                    System.IO.File.WriteAllText(configurationPath, jar);

                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Added new application configuration: " + cfg.Name + " in " + projectName;

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