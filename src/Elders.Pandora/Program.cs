using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elders.Padnora.Box;
using Newtonsoft.Json;

namespace Elders.Configuration.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var jar = JsonConvert.DeserializeObject<Jar>(File.ReadAllText("conf.json"));
            var box = Box.Mistranslate(jar);
            var cfg = new Pandora(box).Open("test", "localhost");

            System.Console.ReadLine();
        }
    }
}
