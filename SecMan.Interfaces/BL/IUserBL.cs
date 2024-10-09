using SecMan.Model;
using static SecMan.Model.User;

namespace SecMan.Interfaces.BL
{
    public interface IUserBL
    {
        Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model);
        Task<ServiceResponse<User>> AddUserAsync(CreateUser model);
        Task<ServiceResponse<User>> GetUserByIdAsync(ulong userId);
        Task<ServiceResponse<User>> UpdateUserAsync(UpdateUser model, ulong userId);
        Task<ApiResponse> DeleteUserAsync(ulong userId);
    }

}
