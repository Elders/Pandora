using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    /// <summary>
    /// In Greek mythology, Pandora was the first human woman created by the gods, specifically by Hephaestus and Athena on the 
    /// instructions of Zeus. As Hesiod related it, each god helped create her by giving her unique gifts. Zeus ordered Hephaestus to 
    /// mold her out of earth as part of the punishment of humanity for Prometheus' theft of the secret of fire, and all the gods joined in 
    /// offering her "seductive gifts". Her other name—inscribed against her figure on a white-ground kylix in the British Museum—is Anesidora, 
    /// "she who sends up gifts" (up implying "from below" within the earth) 
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Pandora
    /// </remarks>
    public class Pandora
    {
        readonly IConfigurationRepository cfgRepo;
        readonly ApplicationContext context;

        public Pandora(ApplicationContext applicationContext, IConfigurationRepository configurationRepository)
        {
            this.context = applicationContext;
            this.cfgRepo = configurationRepository;
        }

        public ApplicationContext ApplicationContext { get { return context; } }

        public string Get(string key)
        {
            return Get(key, context);
        }

        public string Get(string key, ApplicationContext applicationContext)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ReferenceEquals(null, applicationContext)) throw new ArgumentNullException(nameof(applicationContext));

            var sanitizedKey = key.ToLower();
            string longKey = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, applicationContext.Machine, sanitizedKey);
            var setting = cfgRepo.Get(longKey);
            return setting;
        }

        public T Get<T>(string key)
        {
            return Get<T>(key, context);
        }

        public T Get<T>(string key, ApplicationContext context)
        {
            var json = Get(key, context);
            if (json == null)
                return default(T);
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public IEnumerable<DeployedSetting> GetAll()
        {
            return GetAll(context);
        }

        public IEnumerable<DeployedSetting> GetAll(ApplicationContext applicationContext)
        {
            return from setting in cfgRepo.GetAll()
                   where setting.Cluster == applicationContext.Cluster &&
                         setting.Machine == applicationContext.Machine &&
                         setting.ApplicationName == applicationContext.ApplicationName
                   select setting;
        }

        public void Set(string key, string value)
        {
            var settingName = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, key);
            cfgRepo.Set(settingName, value);
        }
    }
}