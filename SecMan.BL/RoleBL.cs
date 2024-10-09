using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.Data;

namespace SecMan.BL
{
    public class RoleBL : IRoleBL
    {

        private readonly IUnitOfWork _unitOfWork;
        public RoleBL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetRoleDto> AddRoleAsync(CreateRole addRoleDto)
        {
            var result = await _unitOfWork.IRoleRepository.AddRoleAsync(addRoleDto);
            var retGetRoleDto = new GetRoleDto
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                IsLoggedOutType = result.IsLoggedOutType,
                NoOfUsers = addRoleDto.LinkUsers.Count
            };

            return retGetRoleDto;
        }
        public async Task<IEnumerable<GetRoleDto>> GetAllRolesAsync()
        {
            try
            {
                var result = await _unitOfWork.IRoleRepository.GetAll(r => r.Users);

                return result.Select(r => new GetRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsLoggedOutType = r.IsLoggedOutType,
                    NoOfUsers = r.Users.Count
                }).ToList();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<GetRoleDto>();
            }            
         
        }

        public async Task<GetRoleDto?> GetRoleByIdAsync(ulong id)
        {
            Log.Information("Calling GetRoleByIdAsync: {@Id}",id);
            try
            {
                var result = await _unitOfWork.IRoleRepository.GetById(id, r=>r.Users);
                if (result == null)
                {
                    return null;
                }
                var returnRole = new GetRoleDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description,
                    IsLoggedOutType = result.IsLoggedOutType,
                    NoOfUsers = result.Users.Count
                };

                return returnRole;
            }
            catch (Exception ex)
            {
                Log.Information("Exception in GetRoleByIdAsync in RoleBL for {RoleId}", id);
                Log.Error(ex.InnerException, "Exception in GetRoleByIdAsync in RoleBL for {RoleId}",id);
                return null;
            }
           
        }


        public async Task<GetRoleDto?> UpdateRoleAsync(ulong id, CreateRole addRoleDto)
        {
            Task<GetRoleDto?> result = _unitOfWork.IRoleRepository.UpdateRoleAsync(id, addRoleDto);
            return await result;
        }


        public async Task<bool> DeleteRoleAsync(ulong id)
        {
            Log.Information("Calling DeleteRoleAsync: {@Id}", id);
            try
            {
                var result = await _unitOfWork.IRoleRepository.Delete(id);

                await _unitOfWork.SaveChangesAync();
                return result;
            }
            catch (Exception ex)
            {

                Log.Information("Exception in DeleteRoleAsync in RoleBL dor {RoleId}",id);
                Log.Error(ex.InnerException, "Exception in DeleteRoleAsync in RoleBL for {RoleId}", id);
                return false;
            }           

        }
    }
}
