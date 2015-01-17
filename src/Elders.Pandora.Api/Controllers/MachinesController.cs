using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace Elders.Pandora.Api.Controllers
{
    public class MachinesController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MachinesController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        // GET api/machines
        public IEnumerable<KeyValuePair<string, string>> Get(string appName, string clusterName, string machineName)
        {
            if (!appName.EndsWith(".json"))
                appName += ".json";

            var cfgPath = Path.Combine(storageFolder, appName);

            var exists = File.Exists(cfgPath);

            if (!exists)
                throw new ArgumentException("There is no configuration for application " + appName);

            var cfg = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(cfgPath));

            var box = Elders.Pandora.Box.Box.Mistranslate(cfg);

            var pandora = new Pandora(box).Open(clusterName, machineName);

            return pandora.AsDictionary();
        }

        // POST api/machines/appName
        public void Post(string appName, [FromBody]string value)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }

        // PUT api/machines/appName
        public void Put(string appName, [FromBody]string value)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }

        // DELETE api/machines/appName/machineName
        public void Delete(string appName, string machineName)
        {
            try
            {
                if (!appName.EndsWith(".json"))
                    appName += ".json";

                var cfgPath = Path.Combine(storageFolder, appName);

                var exists = File.Exists(cfgPath);

                if (!exists)
                    throw new ArgumentException("There is no configuration for application " + appName);

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
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }
    }
}
