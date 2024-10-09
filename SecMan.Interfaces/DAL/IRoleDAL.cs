using SecMan.Model;

namespace SecMan.Interfaces.DAL
{
    public interface IRoleDal
    {
       // Task<GetRoleDto> AddRoleAsync(CreateRole addRoleDto);
        Task<List<GetRoleDto>> GetAllRolesAsync();
        //Task<GetRoleDto?> GetRoleByIdAsync(ulong id);
        //Task<GetRoleDto?> UpdateRoleAsync(ulong id, CreateRole addRoleDto);
        //Task<bool> DeleteRoleAsync(ulong id);
    }
}
