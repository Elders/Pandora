using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Pandora.Box;
using Newtonsoft.Json;
using System.ComponentModel;

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
        readonly IPandoraContext context;

        public Pandora(IPandoraContext context, IConfigurationRepository configurationRepository)
        {
            this.context = context;
            this.cfgRepo = configurationRepository;
        }

        public IPandoraContext ApplicationContext { get { return context; } }

        public string Get(string key)
        {
            return Get(key, context);
        }

        public string Get(string key, IPandoraContext applicationContext)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (ReferenceEquals(null, applicationContext)) throw new ArgumentNullException(nameof(applicationContext));

            var sanitizedKey = key.ToLower();
            string keyForMachine = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, applicationContext.Machine, sanitizedKey);

            if (cfgRepo.Exists(keyForMachine))
            {
                return cfgRepo.Get(keyForMachine);
            }
            else
            {
                string keyForCluster = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, Machine.NotSpecified, sanitizedKey);
                return cfgRepo.Get(keyForCluster);
            }
        }

        public T Get<T>(string key)
        {
            return Get<T>(key, context);
        }

        public T Get<T>(string key, IPandoraContext context)
        {
            var value = Get(key, context);
            if (value == null)
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.IsValid(value))
            {
                T converted = (T)converter.ConvertFrom(value);
                return converted;
            }
            else
            {
                var result = JsonConvert.DeserializeObject<T>(value);
                return result;
            }
        }

        public IEnumerable<DeployedSetting> GetAll()
        {
            return cfgRepo.GetAll();
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
            Set(key, value, context);
        }

        public void Set(string key, string value, IPandoraContext applicationContex)
        {
            var settingName = NameBuilder.GetSettingName(applicationContex.ApplicationName, applicationContex.Cluster, applicationContex.Machine, key);
            cfgRepo.Set(settingName, value);
        }

        public void Delete(string key)
        {
            Delete(key, context);
        }

        public void Delete(string key, IPandoraContext applicationContex)
        {
            var settingName = NameBuilder.GetSettingName(applicationContex.ApplicationName, applicationContex.Cluster, applicationContex.Machine, key);
            cfgRepo.Delete(settingName);
        }
    }
}
