using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IRoleBL
    {
        Task<GetRoleDto> AddRoleAsync(CreateRole addRoleDto);
        Task<IEnumerable<GetRoleDto>> GetAllRolesAsync();
        Task<GetRoleDto?> GetRoleByIdAsync(ulong id);
        Task<GetRoleDto?> UpdateRoleAsync(ulong id, CreateRole addRoleDto);
        Task<bool> DeleteRoleAsync(ulong id);
    }
}
