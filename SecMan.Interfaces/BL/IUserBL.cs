using SecMan.Model;
using static SecMan.Model.User;

namespace SecMan.Interfaces.BL
{
    public interface IUserBL
    {
        Task<ServiceResponse<UserDto>> AddUserAsync(AddUserDto model);

        Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model);

        Task<ServiceResponse<UserDto>> GetUserByIdAsync(ulong userId);

        Task<ServiceResponse<UserDto>> UpdateUserAsync(AddUserDto model, ulong userId);

        Task<ApiResponse> DeleteUserAsync(ulong userId);
    }

}
