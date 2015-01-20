using Elders.Pandora.Box;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace Elders.Pandora.UI.api
{
    public class DefaultsController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultsController));

        private string storageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Elders", "Pandora");

        // GET api/defaults/appName
        public IEnumerable<KeyValuePair<string, string>> Get(string appName)
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

                return box.Defaults.AsDictionary();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // GET api/defaults/appName/defaultName
        public string Get(string appName, string defaultName)
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

                var setting = box.Defaults.AsDictionary().FirstOrDefault(x => x.Key == defaultName);

                if (string.IsNullOrWhiteSpace(setting.Value))
                    throw new ArgumentNullException("There is no default setting with name " + defaultName);
                else
                    return setting.Value;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // POST api/defaults/appName
        public void Post(string appName, [FromBody]KeyValuePair<string, string> setting)
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

                var defaults = box.Defaults.AsDictionary();

                if (!box.Defaults.ContainsKey(setting.Key))
                {
                    defaults.Add(setting.Key, setting.Value);

                    box.Defaults = new Elders.Pandora.Box.Configuration(box.Defaults.Name, defaults);

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // PUT api/defaults/appName
        public void Put(string appName, [FromBody]string key, [FromBody]string value)
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

                var defaults = box.Defaults.AsDictionary();

                if (defaults.ContainsKey(key))
                {
                    defaults[key] = value;

                    box.Defaults = new Elders.Pandora.Box.Configuration(box.Defaults.Name, defaults);

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box), Formatting.Indented);

                    File.WriteAllText(cfgPath, jar);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw ex;
            }
        }

        // DELETE api/defaults/appName/key
        public void Delete(string appName, string key)
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

                var defaults = box.Defaults.AsDictionary();

                if (defaults.ContainsKey(key))
                {
                    defaults.Remove(key);

                    box.Defaults = new Elders.Pandora.Box.Configuration(box.Defaults.Name, defaults);

                    var jar = JsonConvert.SerializeObject(Elders.Pandora.Box.Box.Mistranslate(box));

                    File.WriteAllText(cfgPath, jar);
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
