using System;

namespace Elders.Pandora
{
    public class DeployedSetting
    {
        public DeployedSetting(string raw, string applicationName, string cluster, string machine, string settingKey, string value)
            : this(new Key(applicationName, cluster, machine, settingKey), value)
        {

        }

        public DeployedSetting(Key key, string value)
        {
            if (ReferenceEquals(null, key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

            Key = key;
            Value = value;
        }

        public Key Key { get; private set; }

        public string Value { get; private set; }
    }
}
