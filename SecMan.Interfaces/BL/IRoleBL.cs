using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IRoleBL
    {
        Task<GetRoleDto> AddRoleAsync(AddRoleDto addRoleDto);
        Task<List<GetRoleDto>> GetAllRolesAsync();
        Task<GetRoleDto?> GetRoleByIdAsync(ulong id);
        Task<GetRoleDto?> UpdateRoleAsync(ulong id, AddRoleDto addRoleDto);
        Task<bool> DeleteRoleAsync(ulong id);
    }
}
