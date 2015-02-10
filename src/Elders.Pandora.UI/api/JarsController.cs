using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Linq;

namespace Elders.Pandora.UI.api
{
    [Authorize]
    public class JarsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(JarsController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        public IEnumerable<Jar> Get()
        {
            var jars = Directory.GetFiles(storageFolder, "*.json", SearchOption.AllDirectories);

            foreach (var jar in jars)
            {
                Jar jarObject = null;

                try
                {
                    jarObject = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(jar));
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    continue;
                }

                if (jar != null)
                    yield return jarObject;
            }
        }

        public IEnumerable<Jar> Get(string projectName)
        {
            var jars = Directory.GetFiles(Path.Combine(storageFolder, projectName), "*.json", SearchOption.AllDirectories);

            foreach (var jar in jars)
            {
                Jar jarObject = null;

                try
                {
                    jarObject = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(jar));
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    continue;
                }

                if (jar != null)
                    yield return jarObject;
            }
        }

        public Jar Get(string projectName, string applicationName)
        {
            try
            {
                var workingDir = Path.Combine(storageFolder, projectName);

                var filePath = Path.Combine(workingDir, applicationName, applicationName + ".json");

                return JsonConvert.DeserializeObject<Jar>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Post(string projectName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(cfg.Name) || string.IsNullOrWhiteSpace(projectName))
                    return;

                var workingDir = Path.Combine(storageFolder, projectName);

                var filePath = Path.Combine(workingDir, cfg.Name, cfg.Name + ".json");

                if (File.Exists(filePath))
                    throw new ArgumentException("There is already a configuration with name " + cfg.Name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                Directory.CreateDirectory(Path.Combine(workingDir, cfg.Name));

                File.WriteAllText(filePath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Added new application configuration: " + cfg.Name + " in " + projectName;

                var git = new Git(workingDir);
                git.Stage(new List<string>() { filePath });
                git.Commit(message, username, email);
                git.Push();

                //MvcApplication.TcpServer.SendToAllClients(Encoding.UTF8.GetBytes(jar));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Put(string projectName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                var workingDir = Path.Combine(storageFolder, projectName);

                var filePath = Path.Combine(workingDir, cfg.Name, cfg.Name + ".json");

                if (!File.Exists(filePath))
                    throw new ArgumentException("There is no configuration with name " + cfg.Name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Updated configuration " + cfg.Name + " for " + projectName;

                var git = new Git(workingDir);
                git.Stage(new List<string>() { filePath });
                git.Commit(message, username, email);
                git.Push();

                MvcApplication.TcpServer.SendToAllClients(Encoding.UTF8.GetBytes(jar));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        public void Delete(string projectName, string applicationName)
        {
            try
            {
                var workingDir = Path.Combine(storageFolder, projectName);

                var filePath = Path.Combine(workingDir, applicationName, applicationName + ".json");

                if (File.Exists(filePath))
                {
                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Deleted configuration " + applicationName + " from " + projectName;

                    var git = new Git(workingDir);
                    git.Remove(new List<string>() { filePath });
                    git.Stage(new List<string>() { filePath });
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