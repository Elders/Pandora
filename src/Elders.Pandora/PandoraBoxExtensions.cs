namespace Elders.Pandora
{
    public static class PandoraBoxExtensions
    {
        public static Elders.Pandora.Box.Configuration Open(this Elders.Pandora.Box.Box box, PandoraOptions options)
        {
            var opener = new PandoraBoxOpener(box);
            return opener.Open(options);
        }
    }
}
