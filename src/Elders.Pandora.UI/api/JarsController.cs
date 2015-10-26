using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Linq;
using Elders.Pandora.UI.Common;

namespace Elders.Pandora.UI.api
{
    //[Authorize]
    public class JarsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(JarsController));

        public IEnumerable<Jar> Get()
        {
            var projects = Directory.GetDirectories(Folders.Projects);

            foreach (var project in projects)
            {
                var applications = Directory.GetDirectories(project);

                foreach (var application in applications)
                {
                    if (application.Contains(".git"))
                        continue;

                    var files = Directory.GetFiles(application, "*.json");

                    if (files.Count() > 1)
                        throw new InvalidOperationException("There are multiple configuration files for application: " + application.Replace(project + "\\", ""));

                    var configPath = files.First();

                    Jar jarObject = null;

                    try
                    {
                        jarObject = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);

                        continue;
                    }

                    if (configPath != null)
                        yield return jarObject;
                }
            }
        }

        public IEnumerable<Jar> Get(string projectName)
        {
            string projectPath = Path.Combine(Folders.Projects, projectName);

            var applications = Directory.GetDirectories(projectPath);

            foreach (var application in applications)
            {
                if (application.Contains(".git"))
                    continue;

                var files = Directory.GetFiles(application, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + application.Replace(projectPath + "\\", ""));

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + application.Replace(projectPath + "\\", ""));

                var configPath = files.First();

                Jar jarObject = null;

                try
                {
                    jarObject = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    continue;
                }

                if (configPath != null)
                    yield return jarObject;
            }
        }

        public Jar Get(string projectName, string applicationName)
        {
            try
            {
                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationPath.Replace(projectPath + "\\", ""));

                if (files.Count() > 1)
                    throw new InvalidOperationException("There are multiple configuration files for application: " + applicationPath.Replace(projectPath + "\\", ""));

                var configPath = files.First();

                return JsonConvert.DeserializeObject<Jar>(File.ReadAllText(configPath));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw;
            }
        }

        public void Post(string projectName, string applicationName, string fileName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(applicationName))
                    throw new InvalidOperationException();

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                if (Directory.Exists(applicationPath))
                {
                    var files = Directory.GetFiles(applicationPath, "*.json");

                    if (files.Count() > 0)
                        throw new InvalidOperationException("There is already a configuration file for application: " + applicationPath.Replace(projectPath + "\\", ""));
                }
                else
                {
                    Directory.CreateDirectory(Path.Combine(applicationPath));
                }

                var filePath = Path.Combine(applicationPath, fileName + ".json");

                var box = Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Added new application configuration: " + cfg.Name + " in " + projectName;

                var git = new Git(projectPath);
                git.Stage(new List<string>() { filePath });
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

        public void Put(string projectName, string applicationName, string fileName, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(applicationName))
                    throw new InvalidOperationException();

                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                var files = Directory.GetFiles(applicationPath, "*.json");

                if (files.Count() == 0)
                    throw new InvalidOperationException("There is no configuration file for application: " + applicationPath.Replace(projectPath + "\\", ""));

                var filePath = Path.Combine(applicationPath, fileName + ".json");

                var box = Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);

                var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                var username = nameClaim != null ? nameClaim.Value : "no name claim";
                var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                var email = emailClaim != null ? emailClaim.Value : "no email claim";
                var message = "Added new application configuration: " + cfg.Name + " in " + projectName;

                var git = new Git(projectPath);
                git.Stage(new List<string>() { filePath });
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

        public void Delete(string projectName, string applicationName)
        {
            try
            {
                var projectPath = Path.Combine(Folders.Projects, projectName);

                var applicationPath = Path.Combine(projectPath, applicationName);

                if (Directory.Exists(applicationPath))
                {
                    var nameClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "name");
                    var username = nameClaim != null ? nameClaim.Value : "no name claim";
                    var emailClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "email");
                    var email = emailClaim != null ? emailClaim.Value : "no email claim";
                    var message = "Deleted configuration " + applicationName + " from " + projectName;

                    var git = new Git(projectPath);
                    git.Remove(new List<string>() { applicationPath });
                    git.Stage(new List<string>() { applicationPath });
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