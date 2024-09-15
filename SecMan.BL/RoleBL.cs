using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.BL
{
    public class RoleBL : IRoleBL
    {
        private readonly IRoleDAL _roleDAL;
        public RoleBL(IRoleDAL roleDAL)
        {
            _roleDAL = roleDAL;
        }
        public async Task<GetRoleDto> AddRoleAsync(AddRoleDto addRoleDto)
        {
            Task<GetRoleDto> result = _roleDAL.AddRoleAsync(addRoleDto);
            return await result;
        }
        public async Task<List<GetRoleDto>> GetAllRolesAsync()
        {
            Task<List<GetRoleDto>> result = _roleDAL.GetAllRolesAsync();
            return await result;
        }
        public async Task<GetRoleDto?> GetRoleByIdAsync(ulong id)
        {
            Task<GetRoleDto?> result = _roleDAL.GetRoleByIdAsync(id);
            return await result;
        }
        public async Task<GetRoleDto?> UpdateRoleAsync(ulong id, AddRoleDto addRoleDto)
        {
            Task<GetRoleDto?> result = _roleDAL.UpdateRoleAsync(id, addRoleDto);
            return await result;
        }
        public async Task<bool> DeleteRoleAsync(ulong id)
        {
            Task<bool> result = _roleDAL.DeleteRoleAsync(id);
            return await result;
        }
    }
}
