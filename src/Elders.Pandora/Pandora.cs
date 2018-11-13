using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        readonly IPandoraContext context;

        public Pandora(IPandoraContext context, IConfigurationRepository configurationRepository)
        {
            this.context = context;
            this.cfgRepo = configurationRepository;
        }

        public Pandora(IPandoraFactory factory) : this(factory.GetContext(), factory.GetConfiguration()) { }

        public IPandoraContext ApplicationContext { get { return context; } }

        public string Get(string settingKey)
        {
            return Get(settingKey, context);
        }

        public string Get(string settingKey, IPandoraContext applicationContext)
        {
            if (string.IsNullOrEmpty(settingKey)) throw new ArgumentNullException(nameof(settingKey));
            if (ReferenceEquals(null, applicationContext)) throw new ArgumentNullException(nameof(applicationContext));

            var sanitizedKey = settingKey.ToLower();
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

        public T Get<T>(string settingKey)
        {
            return Get<T>(settingKey, context);
        }

        public T Get<T>(string settingKey, IPandoraContext context)
        {
            var value = Get(settingKey, context);
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

        public bool TryGet(string settingKey, out string value)
        {
            return TryGet(settingKey, context, out value);
        }

        public bool TryGet(string settingKey, IPandoraContext applicationContext, out string value)
        {
            var sanitizedKey = settingKey.ToLower();
            var keyForMachine = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, applicationContext.Machine, sanitizedKey);

            if (cfgRepo.Exists(keyForMachine))
            {
                value = cfgRepo.Get(keyForMachine);
                return true;
            }

            string keyForCluster = NameBuilder.GetSettingName(applicationContext.ApplicationName, applicationContext.Cluster, Machine.NotSpecified, sanitizedKey);

            if (cfgRepo.Exists(keyForCluster))
            {
                value = cfgRepo.Get(keyForCluster);
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGet<T>(string settingKey, out T value)
        {
            return TryGet<T>(settingKey, context, out value);
        }

        public bool TryGet<T>(string settingKey, IPandoraContext applicationContext, out T value)
        {
            string rawValue;

            if (TryGet(settingKey, applicationContext, out rawValue) == false)
            {
                value = default(T);
                return false;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.IsValid(rawValue))
            {
                value = (T)converter.ConvertFrom(rawValue);
                return true;
            }
            else
            {
                value = JsonConvert.DeserializeObject<T>(rawValue);
                return true;
            }
        }

        public IEnumerable<DeployedSetting> GetAll()
        {
            return cfgRepo.GetAll();
        }

        public IEnumerable<DeployedSetting> GetAll(IPandoraContext context)
        {
            IEnumerable<DeployedSetting> allKeys = cfgRepo.GetAll();

            IEnumerable<DeployedSetting> clusterKeys = from setting in allKeys
                                                       where setting.Key.Cluster.Equals(context.Cluster, StringComparison.OrdinalIgnoreCase) &&
                                                             setting.Key.Machine.Equals(Box.Machine.NotSpecified, StringComparison.OrdinalIgnoreCase) &&
                                                             setting.Key.ApplicationName.Equals(context.ApplicationName, StringComparison.OrdinalIgnoreCase)
                                                       select setting;

            IEnumerable<DeployedSetting> machineKeys = from setting in allKeys
                                                       where setting.Key.Cluster.Equals(context.Cluster, StringComparison.OrdinalIgnoreCase) &&
                                                             setting.Key.Machine.Equals(context.Machine, StringComparison.OrdinalIgnoreCase) &&
                                                             setting.Key.ApplicationName.Equals(context.ApplicationName, StringComparison.OrdinalIgnoreCase)
                                                       select setting;

            return clusterKeys.Select(item => machineKeys.SingleOrDefault(x => x.Key.SettingKey.Equals(item.Key.SettingKey, StringComparison.OrdinalIgnoreCase)) ?? item);
        }

        public void Set(string settingKey, string value)
        {
            Set(settingKey, value, context);
        }

        public void Set(string settingKey, string value, IPandoraContext context)
        {
            var settingName = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, settingKey);
            cfgRepo.Set(settingName, value);
        }

        public void Delete(string settingKey)
        {
            Delete(settingKey, context);
        }

        public void Delete(string settingKey, IPandoraContext context)
        {
            var settingName = NameBuilder.GetSettingName(context.ApplicationName, context.Cluster, context.Machine, settingKey);
            cfgRepo.Delete(settingName);
        }
    }
}
