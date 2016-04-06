using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Pandora.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dir = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(dir, "SampleConfiguration.json");
            var jar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(filePath));
            var box = Box.Mistranslate(jar);
            var pandora = new Elders.Pandora.Pandora(box);
            var defaults = pandora.Open(Elders.Pandora.PandoraOptions.Defaults);
            var testConfiguration = pandora.Open(new Elders.Pandora.PandoraOptions("test", string.Empty, false));

            var testCluster = box.Clusters.SingleOrDefault(x => x.Name == "test");
            var testClusterConfig = testCluster.AsDictionary();
            testClusterConfig["setting1"] = "override1";
            //testClusterConfig["refSetting2"] = "refOverride2";

            box.Override(new Cluster("test", testClusterConfig));

            testConfiguration = pandora.Open(new Elders.Pandora.PandoraOptions("test", string.Empty, false));

            Console.WriteLine(filePath);
            // Console.ReadKey();
        }
    }
}
