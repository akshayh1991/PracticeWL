using static SecMan.Model.User;

namespace SecMan.Interfaces.DAL
{
    public interface IUserDAL
    {
        Task<ulong> AddUserAsync(AddUserDto model);

        Task<ulong> UpdateUserAsync(AddUserDto model, ulong userId);

        IQueryable<RoleDto> GetRoles();

        IQueryable<UserDto> GetUser();

        Task DeleteUserAsync(ulong userId);
    }
}
