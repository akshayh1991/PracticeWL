using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecMan.Data.Exceptions;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace SecMan.Data.DAL
{

    public class RoleRepository : GenericRepository<SQLCipher.Role>, IRoleRepository
    {
        private readonly Serilog.ILogger _logger = Log.ForContext<RoleRepository>();

        private Db _context { get; }


        public RoleRepository(Db context) : base(context)
        {
            _context = context;
        }

        public override async Task<bool> Delete(object id)
        {
            _logger.Information("Attempting to delete role with ID: {RoleId}", id);

            try
            {
                ulong newId = (ulong)id;

                // Retrieve the role to be deleted
                var role = await _context.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == newId);

                if (role == null)
                {
                    _logger.Information("No role found with ID: {RoleId}. Deletion aborted.", id);
                    return false;
                }


                // Remove associated role-user links if necessary
                if (role.Users.Any())
                {
                    _logger.Information("Removing associated role-user links for Role ID: {RoleId}. Number of links to remove: {LinkCount}", id, role.Users.Count);
                }

                // Remove the role
                _context.Roles.Remove(role);

                _logger.Information("Role with ID: {RoleId} deleted successfully.", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Information("Exception in Delete Role for {RoleId}", id);
                _logger.Error(ex.InnerException, "Exception in Delete Role for {RoleId}", id);
                return false;
            }
        }

        public async Task<SQLCipher.Role> AddRoleAsync(CreateRole addRoleDto)
        {
            _logger.Information("Starting to add role. Role Name: {RoleName}", addRoleDto.Name);

            var existingRole = await _context.Roles.AnyAsync(x => x.Name == addRoleDto.Name);

            if (existingRole)
            {
                _logger.Warning("Conflict detected. Role with name {RoleName} already exists.", addRoleDto.Name);
                throw new ConflictException("A role with the same name already exists.");
            }
            var allUsers = await _context.Users.CountAsync(x => addRoleDto.LinkUsers.Contains(x.Id));

            if (allUsers != addRoleDto.LinkUsers.Count)
            {
                _logger.Warning("Invalid user IDs provided for role addition");
                throw new BadRequestForLinkUsersNotExits("Some provided user IDs do not exist. Please provide valid user IDs.");
            }
            try
            {
                var validUsers = await _context.Users.Where(x => addRoleDto.LinkUsers.Contains(x.Id)).ToListAsync();
                // Create new role
                var role = new SecMan.Data.SQLCipher.Role
                {
                    Name = addRoleDto.Name,
                    Description = addRoleDto.Description,
                    IsLoggedOutType = addRoleDto.IsLoggedOutType,
                    Users = validUsers
                };

                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                ulong roleId = role.Id;

                _logger.Information("Role creation completed. Role ID: {RoleId}", roleId);

                return role;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException, "Database update exception occurred while adding role. Role Name: {RoleName}", addRoleDto.Name);

                return new SecMan.Data.SQLCipher.Role();
            }
        }

        public async Task<GetRoleDto> UpdateRoleAsync(ulong id, CreateRole addRoleDto)
        {
            _logger.Information("Attempting to update role with ID: {RoleId}", id);

            // Retrieve the role to be updated
            var role = await _context.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

            if (role == null)
            {
                _logger.Information("No role found with ID: {RoleId}. Update aborted.", id);
                return null;
            }
            var existingName = await _context.Roles.AnyAsync(x => x.Name == addRoleDto.Name && x.Id != id);

            if (existingName)
            {
                _logger.Warning("Conflict detected. Role with name {RoleName} already exists.", addRoleDto.Name);
                throw new UpdatingExistingNameException("A role with the same name already exists.");
            }

            var allUsers = await _context.Users.Where(x => addRoleDto.LinkUsers.Contains(x.Id))
                                          .ToListAsync();
            if (allUsers.Count != addRoleDto.LinkUsers.Count)
            {
                _logger.Warning("Invalid user IDs provided for role addition");
                throw new BadRequestForLinkUsersNotExits("Some provided user IDs do not exist. Please provide valid user IDs.");
            }

            try
            {
                _logger.Information("Role found with ID: {RoleId}. Updating role details.", id);

                // Update role properties
                role.Name = addRoleDto.Name;
                role.Description = addRoleDto.Description;
                role.IsLoggedOutType = addRoleDto.IsLoggedOutType;

                // Update role-user links
                var newUsers = allUsers.Where(x => addRoleDto.LinkUsers.Contains(x.Id)).ToList();
                role.Users = newUsers;

                _logger.Information("Removing existing role-user links and adding new ones for Role ID: {RoleId}.", id);

                // Save changes to the database
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();

                _logger.Information("Role updated successfully with ID: {RoleId}.", id);

                // Create DTO for updated role
                var updatedRoleDto = new GetRoleDto
                {
                    Id = id,
                    Name = role.Name,
                    Description = role.Description,
                    IsLoggedOutType = role.IsLoggedOutType,
                    NoOfUsers = role.Users.Count
                };

                _logger.Information("Role update completed with ID: {RoleId}. New user count: {UserCount}", id, updatedRoleDto.NoOfUsers);

                return updatedRoleDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException, "Database update exception occurred while updating role with ID: {RoleId}", id);
                return null;
            }
        }


    }
}

