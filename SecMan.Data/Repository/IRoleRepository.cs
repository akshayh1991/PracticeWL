using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.DAL
{
    public interface IRoleRepository : IGenericRepository<SQLCipher.Role>
    {
        Task<SQLCipher.Role> AddRoleAsync(CreateRole addRoleDto);

        Task<GetRoleDto?> UpdateRoleAsync(ulong id, CreateRole addRoleDto);
    }
}
