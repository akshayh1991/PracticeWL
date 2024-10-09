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
    public interface IUserRepository : IGenericRepository<SQLCipher.User>
    {
        Task<Model.User> UpdateUserAsync(UpdateUser model, ulong userId);

        Task<Model.User> AddUserAsync(CreateUser model);

        Task<SQLCipher.User?> GetUserByUsername(string username);

        Task<List<RoleModel>> GetRolesByRoleId(List<ulong> roleIds);

        Task<ulong?> GetUserBySessionId(string sessionId);

        Task<Tuple<UserDetails?, List<AppPermissions>?>> GetUserDetails(ulong userId);

        Task UpdateUserSessionDetails(ulong userId, string sessionId, double sessionExpiryTime, bool isLogin);

        Task LogLoginAttempts(ulong userId, bool isLoginSuccess);
    }
}
