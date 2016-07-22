using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public static class PandoraIdentityServerServiceFactory
    {
        public static IdentityServerServiceFactory Create(IEnumerable<Client> clients, IEnumerable<Scope> scopes)
        {

            var factory = new IdentityServerServiceFactory
            {
                UserService = new Registration<IUserService>(resolver => PandoraUserServiceFactory.Factory()),
                ScopeStore = new Registration<IScopeStore>(resolver => new InMemoryScopeStore(scopes)),
                ClientStore = new Registration<IClientStore>(resolver => new InMemoryClientStore(clients)),
                //ClientStore = new Registration<IClientStore, CustomClientStore>()
            };

            return factory;
        }
    }
}