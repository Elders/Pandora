namespace Elders.Pandora
{
    public interface IPandoraFactory
    {
        IPandoraContext GetContext();
        IConfigurationRepository GetConfiguration();
    }
}
