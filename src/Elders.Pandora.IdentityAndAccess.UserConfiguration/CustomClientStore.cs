using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class CustomClientStore : IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var clientStorePath = ConfigurationManager.AppSettings["ClientStoreFolder"];

            var clientPath = Path.Combine(clientStorePath, clientId + ".json");

            if (File.Exists(clientPath))
            {
                return new Task<Client>(() => JsonConvert.DeserializeObject<Client>(File.ReadAllText(clientPath)));
            }

            return null;
        }
    }
}