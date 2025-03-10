using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;

namespace Elders.Pandora.Tests
{
    public class When_combining_cluster_with_machine_configurations
    {
        Establish context = () =>
        {
            IConfigurationRepository cfgRepo = new TestConfigurationRepository();
            appContext = new ApplicationContext("app", "cluster", "m1");
            pandora = new Pandora(appContext, cfgRepo);
        };

        Because of = () => allKeys = pandora.GetAll(appContext).ToList();

        It should_have_all_keys = () => allKeys.Count.ShouldEqual(3);

        It should_have_propper_key1 = () => allKeys.Where(x => x.Key.SettingKey == "key1").Single().Value.ShouldEqual("machine_value_1");
        It should_have_propper_key2 = () => allKeys.Where(x => x.Key.SettingKey == "key2").Single().Value.ShouldEqual("cluster_value_2");
        It should_have_propper_key3 = () => allKeys.Where(x => x.Key.SettingKey == "key3").Single().Value.ShouldEqual("machine_value_3");

        static Pandora pandora;
        static ApplicationContext appContext;
        static List<DeployedSetting> allKeys;
    }

    public class TestConfigurationRepository : IConfigurationRepository
    {
        List<DeployedSetting> keys = new List<DeployedSetting>();

        const string App = "app";
        const string Cluster = "cluster";
        const string Machine = "m1";

        public TestConfigurationRepository()
        {
            keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key1"), "cluster_value_1"));
            keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key2"), "cluster_value_2"));
            keys.Add(new DeployedSetting(new Key(App, Cluster, Box.Machine.NotSpecified, "key3"), "cluster_value_3"));

            keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key1"), "machine_value_1"));
            keys.Add(new DeployedSetting(new Key(App, Cluster, Machine, "key3"), "machine_value_3"));
        }

        public Task DeleteAsync(string key) { throw new NotImplementedException(); }

        public Task<bool> ExistsAsync(string key) { throw new NotImplementedException(); }

        public Task<string> GetAsync(string key) { throw new NotImplementedException(); }

        public IEnumerable<DeployedSetting> GetAll(IPandoraContext context)
        {
            return keys;
        }

        public Task SetAsync(string key, string value) { throw new NotImplementedException(); }
    }
}
