using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Http;

namespace Elders.Pandora.Api.Controllers
{
    public class JarsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(JarsController));

        private string storageFolder = ConfigurationManager.AppSettings["StorageFolder"];

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
                if (!name.EndsWith(".json"))
                    name += ".json";

                return JsonConvert.DeserializeObject<Jar>(File.ReadAllText(Path.Combine(storageFolder, name)));
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // POST api/jars
        public void Post([FromBody]string value)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                if (string.IsNullOrWhiteSpace(cfg.Name))
                    return;

                var filePath = Path.Combine(storageFolder, cfg.Name);

                if (File.Exists(filePath))
                    throw new ArgumentException("There is already a configuration with name " + cfg.Name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // PUT api/jars/name
        public void Put(string name, [FromBody]string value)
        {
            try
            {
                if (!name.EndsWith(".json"))
                    name += ".json";

                var cfg = JsonConvert.DeserializeObject<Jar>(value);

                var filePath = Path.Combine(storageFolder, name);

                var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

                var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                File.WriteAllText(filePath, jar);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // DELETE api/jars/name
        public void Delete(string name)
        {
            try
            {
                var filePath = Path.Combine(storageFolder, name);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }
    }
}
