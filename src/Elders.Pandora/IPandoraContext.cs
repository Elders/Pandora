namespace Elders.Pandora
{
    public interface IPandoraContext
    {
        string ApplicationName { get; }

        string Cluster { get; }

        string Machine { get; }
    }
}
