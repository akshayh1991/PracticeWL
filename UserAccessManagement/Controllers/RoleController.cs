using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Data.Exceptions;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using UserAccessManagement.Filters;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// Controller for managing roles within the application.
    /// </summary>
    [Route("roles")]
    [ApiController]
    //[Authorize]
    //[TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBL _roleBAL; // Business logic layer for roles
        private readonly ILogger<RoleController> _logger; // Logger for capturing logs

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class.
        /// </summary>
        /// <param name="roleBAL">Business logic layer for roles.</param>
        /// <param name="logger">Logger for capturing logs.</param>
        public RoleController(IRoleBL roleBAL, ILogger<RoleController> logger)
        {
            _roleBAL = roleBAL;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="dto">Data transfer object containing role details.</param>
        /// <returns>Action result indicating the outcome of the creation.</returns>
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] CreateRole dto)
        {
            _logger.LogInformation("AddRole method called with request: {@Request}", dto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var role = await _roleBAL.AddRoleAsync(dto);
                return CreatedAtAction(nameof(AddRole), role);
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Conflict occurred while adding role.");
                throw;
            }
            catch (BadRequestForLinkUsersNotExits ex)
            {
                _logger.LogError(ex, "Provided LinkUser/Users doesn't exist. Please provide a valid LinkUser/Users list.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding role.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <returns>Action result containing the list of roles.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.LogInformation("GetAllRoles method called");

            try
            {
                var roles = await _roleBAL.GetAllRolesAsync();


                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving roles.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to retrieve.</param>
        /// <returns>Action result containing the role details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(ulong id)
        {
            _logger.LogInformation("GetRoleById method called with ID: {Id}", id);

            try
            {
                var role = await _roleBAL.GetRoleByIdAsync(id);
                if (role == null)
                {
                    throw new CommonBadRequestForRole("The role with the specified ID was not found.");
                }
                return Ok(role);
            }
            catch (CommonBadRequestForRole ex)
            {
                _logger.LogWarning(ex, "Role not found with ID: {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving role with ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to update.</param>
        /// <param name="addRoleDto">Data transfer object containing updated role details.</param>
        /// <returns>Action result indicating the outcome of the update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(ulong id, [FromBody] CreateRole addRoleDto)
        {
            _logger.LogInformation("UpdateRole method called with ID: {Id} and request: {@Request}", id, addRoleDto);

            try
            {
                var role = await _roleBAL.UpdateRoleAsync(id, addRoleDto);
                if (role == null)
                {
                    throw new CommonBadRequestForRole("The role with the specified ID was not found.");
                }
                return Ok(role);
            }
            catch (UpdatingExistingNameException ex)
            {
                _logger.LogWarning(ex, "Role name already present with same name: {Id}", id);
                throw;
            }
            catch (CommonBadRequestForRole ex)
            {
                _logger.LogWarning(ex, "Role not found with ID: {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating role with ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a role by its ID.
        /// </summary>
        /// <param name="id">The ID of the role to delete.</param>
        /// <returns>Action result indicating whether the deletion was successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(ulong id)
        {
            _logger.LogInformation("DeleteRole method called with ID: {Id}", id);

            try
            {
                var isDeleted = await _roleBAL.DeleteRoleAsync(id);

                if (isDeleted)
                {
                    return NoContent();
                }
                else
                {
                    throw new CommonBadRequestForRole("The role with the specified ID was not found.");
                }
            }
            catch (CommonBadRequestForRole ex)
            {
                _logger.LogWarning(ex, "Role not found with ID: {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting role with ID: {Id}", id);
                throw;
            }
        }
    }
}
