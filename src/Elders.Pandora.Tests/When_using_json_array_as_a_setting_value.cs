using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Elders.Pandora.Tests
{
    public class When_using_json_array_as_a_setting_value
    {
        Establish context = () =>
        {
            IConfigurationRepository cfgRepo = new ArrayTestConfigurationRepository();
            appContext = new ApplicationContext("app", "cluster", "m1");
            pandora = new Pandora(appContext, cfgRepo);
        };

        Because of = () => allKeys = pandora.GetAll(appContext).ToList();

        It should_have_all_keys = () => allKeys.Count.ShouldEqual(2);

        It should_have_propper_key1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:0").Single().Value.ShouldEqual("value0");
        It should_have_propper_key2 = () => allKeys.Where(x => x.Key.SettingKey == "key1:1").Single().Value.ShouldEqual("value1");

        static Pandora pandora;
        static ApplicationContext appContext;
        static List<DeployedSetting> allKeys;

        class ArrayTestConfigurationRepository : IConfigurationRepository
        {
            List<DeployedSetting> keys = new List<DeployedSetting>();

            const string App = "app";
            const string Cluster = "cluster";
            const string Machine = "m1";

            public ArrayTestConfigurationRepository()
            {
                keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key1"), "[\"value0\", \"value1\", \"value2\"]"));
                keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key1"), "[\"value0\", \"value1\"]"));
            }

            public void Delete(string key) { throw new NotImplementedException(); }

            public bool Exists(string key) { throw new NotImplementedException(); }

            public string Get(string key) { throw new NotImplementedException(); }

            public IEnumerable<DeployedSetting> GetAll(IPandoraContext context)
            {
                return keys;
            }

            public void Set(string key, string value) { throw new NotImplementedException(); }
        }
    }
}
