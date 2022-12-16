using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Elders.Pandora.Tests
{
    public class When_using_json_objects_as_a_setting_value
    {
        Establish context = () =>
        {
            IConfigurationRepository cfgRepo = new CompositeTestConfigurationRepository();
            appContext = new ApplicationContext("app", "cluster", "m1");
            pandora = new Pandora(appContext, cfgRepo);
        };

        Because of = () => allKeys = pandora.GetAll(appContext).ToList();

        It should_have_all_keys = () => allKeys.Count.ShouldEqual(10);

        It should_have_propper_key1_stringValue = () => allKeys.Where(x => x.Key.SettingKey == "key1:stringValue").Single().Value.ShouldEqual("value");
        It should_have_propper_key1_objectValue_stringValue = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:stringValue").Single().Value.ShouldEqual("nested-value");
        It should_have_propper_key1_objectValue_intValue = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:intValue").Single().Value.ShouldEqual("1");

        It should_have_propper_key1_objectValue_array_0_prop1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:0:prop1").Single().Value.ShouldEqual("prop1");
        It should_have_propper_key1_objectValue_array_0_prop2 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:0:prop2").Single().Value.ShouldEqual("prop2");
        It should_have_propper_key1_objectValue_array_1 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:1").Single().Value.ShouldEqual("randomString");
        It should_have_propper_key1_objectValue_array_2_prop3 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:2:prop3").Single().Value.ShouldEqual("prop3");
        It should_have_propper_key1_objectValue_array_2_prop4 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:2:prop4").Single().Value.ShouldEqual("prop4");
        It should_have_propper_key1_objectValue_array_3_0 = () => allKeys.Where(x => x.Key.SettingKey == "key1:objectValue:array:3:0").Single().Value.ShouldEqual("nested-array");
        It should_have_propper_key1_boolValue = () => allKeys.Where(x => x.Key.SettingKey == "key1:boolValue").Single().Value.ShouldEqual("True");

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
                keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key1"), "{}"));
                keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key1"), "{\"stringValue\": \"value\",\"objectValue\": {\"stringValue\": \"nested-value\",\"intValue\": 1,\"array\": [{\"prop1\": \"prop1\",\"prop2\": \"prop2\"},\"randomString\",{\"prop3\": \"prop3\",\"prop4\": \"prop4\"},[\"nested-array\"]]},\"boolValue\": true}"));
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
