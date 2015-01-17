using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace Elders.Pandora.Api.Controllers
{
    public class JarsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(JarsController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        // GET api/jars
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

        // GET api/jars/name
        public Jar Get(string name)
        {
            try
            {
                var workingDir = Path.Combine(storageFolder, name);

                var filePath = Path.Combine(workingDir, name + ".json");

                return JsonConvert.DeserializeObject<Jar>(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // POST api/jars
        public void Post(string gitUrl, string email, string username, string password, string message, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(cfg.Name))
                    return;

                var workingDir = Path.Combine(storageFolder, cfg.Name);

                var filePath = Path.Combine(workingDir, cfg.Name + ".json");

                if (File.Exists(filePath))
                    throw new ArgumentException("There is already a configuration with name " + cfg.Name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);

                Git.Clone(gitUrl, workingDir);
                var git = new Git(workingDir, email, username, password);
                git.Commit(new List<string>() { filePath }, message);
                git.Push();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // PUT api/jars
        public void Put(string gitUrl, string email, string username, string password, string message, [FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                var workingDir = Path.Combine(storageFolder, cfg.Name);

                var filePath = Path.Combine(workingDir, cfg.Name + ".json");

                if (!File.Exists(filePath))
                    throw new ArgumentException("There is no configuration with name " + cfg.Name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);

                var git = new Git(workingDir, email, username, password);
                git.Commit(new List<string>() { filePath }, message);
                git.Push();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // DELETE api/jars/name
        public void Delete(string name, string gitUrl, string email, string username, string password, string message)
        {
            try
            {
                var workingDir = Path.Combine(storageFolder, name);

                var filePath = Path.Combine(workingDir, name + ".json");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    var git = new Git(workingDir, email, username, password);
                    git.Commit(new List<string>(), message);
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