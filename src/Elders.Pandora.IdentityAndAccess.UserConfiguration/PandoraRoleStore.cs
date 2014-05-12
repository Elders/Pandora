using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraRoleStore : IRoleStore<PandoraRole, string>, IQueryableRoleStore<PandoraRole>
    {
        public IQueryable<PandoraRole> Roles { get { return new List<PandoraRole>().AsQueryable(); } }
        public Task CreateAsync(PandoraRole role) { throw new NotImplementedException(); }
        public Task DeleteAsync(PandoraRole role) { throw new NotImplementedException(); }
        public void Dispose() { throw new NotImplementedException(); }
        public Task<PandoraRole> FindByIdAsync(string roleId) { throw new NotImplementedException(); }
        public Task<PandoraRole> FindByNameAsync(string roleName) { throw new NotImplementedException(); }
        public Task UpdateAsync(PandoraRole role) { throw new NotImplementedException(); }
    }
}