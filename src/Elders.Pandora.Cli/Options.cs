using CommandLine;
using CommandLine.Text;

namespace Elders.Pandora
{
    class OpenOptions
    {
        public const string EnvVarOutput = "envvar";

        [Option('j', "jar", HelpText = "Input jar file")]
        public string Jar { get; set; }

        [Option('c', "cluster", HelpText = "Cluster name")]
        public string Cluster { get; set; }

        [Option('m', "machine", HelpText = "Machine name")]
        public string Machine { get; set; }

        [Option('a', "application", HelpText = "Application name")]
        public string Application { get; set; }

        [Option('o', "output", DefaultValue = EnvVarOutput, HelpText = "Output")]
        public string Output { get; set; }
    }

    class Options
    {
        public Options()
        {
            OpenVerb = new OpenOptions();
        }

        [VerbOption("open", HelpText = "Opens the pandora box.")]
        public OpenOptions OpenVerb { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage(string verb = null)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
