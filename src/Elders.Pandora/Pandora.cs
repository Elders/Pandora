using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using Elders.Pandora.Box;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<Pandora> logger;

        public Pandora(IPandoraContext context, IConfigurationRepository configurationRepository, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.cfgRepo = configurationRepository;
            this.logger = loggerFactory?.CreateLogger<Pandora>();
        }

        public Pandora(IPandoraContext context, IConfigurationRepository configurationRepository) : this(context, configurationRepository, null) { }

        public Pandora(IPandoraFactory factory) : this(factory.GetContext(), factory.GetConfiguration(), null) { }

        public Pandora(IPandoraFactory factory, ILoggerFactory loggerFactory) : this(factory.GetContext(), factory.GetConfiguration(), loggerFactory) { }

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
                var result = JsonSerializer.Deserialize<T>(value);
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
                value = JsonSerializer.Deserialize<T>(rawValue);
                return true;
            }
        }

        public IEnumerable<DeployedSetting> GetAll(IPandoraContext context)
        {
            try
            {
                IEnumerable<DeployedSetting> allKeys = cfgRepo.GetAll(context);

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

                var merged = clusterKeys.Select(item => machineKeys.SingleOrDefault(x => x.Key.SettingKey.Equals(item.Key.SettingKey, StringComparison.OrdinalIgnoreCase)) ?? item);
                var result = new List<DeployedSetting>();
                foreach (var item in merged)
                {
                    if (item.Value is null)
                    {
                        logger?.LogWarning("Skipping unassigned key {application}.{cluster}.{machine}.{settingKey}. Value is null.",
                            item.Key.ApplicationName,
                            item.Key.Cluster,
                            item.Key.Machine,
                            item.Key.SettingKey);

                        result.Add(item);
                        continue;
                    }

                    var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(item.Value)));
                    var parsed = false;
                    JsonDocument document = default;

                    try
                    {
                        parsed = JsonDocument.TryParseValue(ref reader, out document);
                    }
                    catch (Exception) { }

                    if (parsed)
                    {
                        var parsedSettings = ParseNestedSetting(document.RootElement, item.Key);
                        result.AddRange(parsedSettings);
                        document?.Dispose();
                    }
                    else
                        result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unable to parse configurations.");
                return Enumerable.Empty<DeployedSetting>();
            }

            static List<DeployedSetting> ParseNestedSetting(JsonElement element, Key key, int? index = null)
            {
                var result = new List<DeployedSetting>();

                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        var settingKey = $"{key.SettingKey}:{prop.Name}";
                        if (index.HasValue)
                            settingKey = $"{key.SettingKey}:{index.Value}:{prop.Name}";

                        var newKey = new Key(key.ApplicationName, key.Cluster, key.Machine, settingKey);
                        var nested = ParseNestedSetting(prop.Value, newKey);
                        result.AddRange(nested);
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    var i = 0;
                    foreach (var arrayItem in element.EnumerateArray())
                    {
                        var newKey = key;
                        if (index.HasValue)
                            newKey = key.WithSettingKey($"{key.SettingKey}:{index.Value}");

                        var nested = ParseNestedSetting(arrayItem, newKey, i++);
                        result.AddRange(nested);
                    }
                }
                else if (element.ValueKind == JsonValueKind.Null)
                {
                    var newKey = key;
                    if (index.HasValue)
                        newKey = key.WithSettingKey($"{key.SettingKey}:{index.Value}");

                    result.Add(new DeployedSetting(newKey, null));
                }
                else
                {
                    var newKey = key;
                    if (index.HasValue)
                        newKey = key.WithSettingKey($"{key.SettingKey}:{index.Value}");

                    var value = element.ToString();
                    result.Add(new DeployedSetting(newKey, value));
                }

                return result;
            }
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
