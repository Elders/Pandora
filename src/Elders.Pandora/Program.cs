using System;
using System.IO;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    class Program
    {
        static int Main(string[] args)
        {

            string applicationName = args[0];
            string cluster = args[1];
            string machine = args[2];
            string file = args.Length == 4 ? args[3] : applicationName;

            var jar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(file + ".json"));
            var box = Elders.Pandora.Box.Box.Mistranslate(jar);
            if (box.Name != applicationName)
                throw new InvalidProgramException("Invalid grant");
            var cfg = new Pandora(box).Open(cluster, machine);

            var computedCfg = JsonConvert.SerializeObject(cfg.AsDictionary());

            foreach (var setting in cfg.AsDictionary())
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value, EnvironmentVariableTarget.Machine);
            }

            File.WriteAllText((NameBuilder.GetFileName(box.Name, cluster, machine) + ".json"), computedCfg);

            return 0;
        }
    }
}
