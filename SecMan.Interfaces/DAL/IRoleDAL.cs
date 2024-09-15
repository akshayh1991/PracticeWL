using SecMan.Model;

namespace SecMan.Interfaces.DAL
{
    public interface IRoleDAL
    {
        Task<GetRoleDto> AddRoleAsync(AddRoleDto addRoleDto);
        Task<List<GetRoleDto>> GetAllRolesAsync();
        Task<GetRoleDto?> GetRoleByIdAsync(ulong id);
        Task<GetRoleDto?> UpdateRoleAsync(ulong id, AddRoleDto addRoleDto);
        Task<bool> DeleteRoleAsync(ulong id);
    }
}
