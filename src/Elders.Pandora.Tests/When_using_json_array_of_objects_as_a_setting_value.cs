using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Elders.Pandora.Tests
{
    public class When_using_json_array_of_objects_as_a_setting_value
    {
        Establish context = () =>
        {
            IConfigurationRepository cfgRepo = new CompositeTestConfigurationRepository();
            appContext = new ApplicationContext("app", "cluster", "m1");
            pandora = new Pandora(appContext, cfgRepo);
        };

        Because of = () => allKeys = pandora.GetAll(appContext).ToList();

        It should_have_all_keys = () => allKeys.Count.ShouldEqual(9);

        It should_have_propper_key1_0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:0").Single().Value.ShouldEqual("randomStringValue");
        It should_have_propper_key1_1_0_arr0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:1:0:arr0").Single().Value.ShouldEqual("0");
        It should_have_propper_key1_1_0_arr1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:1:0:arr1").Single().Value.ShouldEqual("1");

        It should_have_propper_key1_2_value0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:2:value0").Single().Value.ShouldEqual("1");
        It should_have_propper_key1_2_value1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:2:value1").Single().Value.ShouldEqual("value1");
        It should_have_propper_key1_3_value0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:3:value0").Single().Value.ShouldEqual("2");
        It should_have_propper_key1_3_value1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:3:value1").Single().Value.ShouldEqual("value2");
        It should_have_propper_key1_4_value0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:4:value0").Single().Value.ShouldEqual("3");
        It should_have_propper_key1_4_value1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:4:value1").Single().Value.ShouldEqual("value3");

        static Pandora pandora;
        static ApplicationContext appContext;
        static List<DeployedSetting> allKeys;

        class CompositeTestConfigurationRepository : IConfigurationRepository
        {
            List<DeployedSetting> keys = new List<DeployedSetting>();

            const string App = "app";
            const string Cluster = "cluster";
            const string Machine = "m1";

            public CompositeTestConfigurationRepository()
            {
                keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key1"), "[{\"value0\": 1,\"value1\": \"value1\"},{\"value0\": 2,\"value1\": \"value2\"}]"));
                keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key1"), "[\"randomStringValue\", [{\"arr0\": 0,\"arr1\": \"1\"}], {\"value0\": 1,\"value1\": \"value1\"},{\"value0\": 2,\"value1\": \"value2\"},{\"value0\": 3,\"value1\": \"value3\"}]"));
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
