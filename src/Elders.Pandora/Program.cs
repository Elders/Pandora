using System;
using System.IO;
using Elders.Pandora.Box;
using Newtonsoft.Json;

namespace Elders.Pandora
{
    class Program
    {
        static void Main(string[] args)
        {
            string applicationName = args[0];
            string cluster = args[1];
            string machine = args[2];



            var jar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText(applicationName + ".json"));
            var box = Elders.Pandora.Box.Box.Mistranslate(jar);
            if (box.Name != applicationName)
                throw new InvalidProgramException("Invalid grant");
            var cfg = new Pandora(box).Open(cluster, machine);

            var computedCfg = JsonConvert.SerializeObject(cfg.AsDictionary());
            File.WriteAllText((NameBuilder.GetFileName(box.Name, cluster, machine) + ".json"), computedCfg);
        }
    }
}
