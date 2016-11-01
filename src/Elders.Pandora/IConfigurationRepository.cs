namespace Elders.Pandora
{
    public interface IConfigurationRepository
    {
        string Get(string key);
        void Set(string key, string value);
    }
}
