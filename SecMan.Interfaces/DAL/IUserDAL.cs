using SecMan.Model;
using static SecMan.Model.User;

namespace SecMan.Interfaces.DAL
{
    public interface IUserDal
    {
        Task<Model.User> AddUserAsync(CreateUser model);

        Task<Model.User> UpdateUserAsync(UpdateUser model, ulong userId);

        Task<List<RoleModel>> GetRolesByRoleId(List<ulong> roleIds);

        IQueryable<RoleModel> GetRoles();

        Task<User?> GetUserByUsername(string? username);

        Task LogLoginAttempts(ulong userId, bool isLoginSuccess);

        Task<string?> GetUserPasswordByUserId(ulong userId);

        Task<Tuple<UserDetails?, List<AppPermissions>?>> GetUserDetails(ulong userId);

        Task<ulong?> GetUserBySessionId(string sessionId);

        Task UpdateUserSessionDetails(ulong userId, string sessionId, double sessionExpiryTime, bool isLogin);

        IQueryable<User> GetUser();

        Task<User?> GetUserById(ulong id);

        Task DeleteUserAsync(ulong userId);
    }
}
