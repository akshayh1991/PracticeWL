using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.Net;
using UserAccessManagement.Filters;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// This Controller contains all the user CRUD API's
    /// </summary>
    [ApiController]
    [Route("users")]
    [ProducesResponseType(typeof(BadRequest), 400)]
    [ProducesResponseType(typeof(ServerError), 500)]
    [ProducesResponseType(typeof(Unauthorized), 401)]
    [ProducesResponseType(typeof(Forbidden), 403)]
    //[Authorize]
    //[TypeFilter(typeof(SecurityActionFilter), Arguments = ["CAN_EDIT_SECURITY"])]
    public class UsersController : ControllerBase
    {
        private readonly IUserBL _userBAL;
        /// <summary>
        /// User Controller Constructor for DI
        /// </summary>
        /// <param name="userBAL"></param>
        public UsersController(IUserBL userBAL)
        {
            _userBAL = userBAL;
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(typeof(Conflict), 409)]
        public async Task<ActionResult> AddUserAsync(CreateUser model)
        {

            Log.Information("Started API {APIName}", nameof(AddUserAsync));
            ServiceResponse<User> res = await _userBAL.AddUserAsync(model);

            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(AddUserAsync));

            return Created(nameof(AddUserAsync), res.Data);
        }

        /// <summary>
        /// List Users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(204)]
        public async Task<ActionResult> GetUsersAsync([FromQuery] UsersFilterDto model)
        {
            Log.Information("Started API {APIName}", nameof(GetUsersAsync));
            ServiceResponse<UsersWithCountDto> res = await _userBAL.GetUsersAsync(model);

            if (res?.Data?.Users?.Count <= 0)
                return NoContent();

            Log.Information("Adding total user count to response headers,User Count {@Usercount}", res?.Data?.UserCount);
            Response.Headers.Append(ResponseHeaders.TotalCount, res?.Data?.UserCount.ToString());
            Response.Headers.Append("Access-Control-Allow-Origin", "*");

            Log.Information("Finishing API {APIName}", nameof(GetUsersAsync));

            return Ok(res?.Data?.Users);
        }

        /// <summary>
        /// Enquire user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<ActionResult> GetUserByIdAsync(ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(GetUserByIdAsync));

            ServiceResponse<User> res = await _userBAL.GetUserByIdAsync(userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(GetUserByIdAsync));

            return Ok(res.Data);
        }

        /// <summary>
        /// Update user by id
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<ActionResult> UpdateUserAsync(UpdateUser model, ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(UpdateUserAsync));

            ServiceResponse<User> res = await _userBAL.UpdateUserAsync(model, userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(UpdateUserAsync));

            return Ok(res.Data);
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteUserAsync(ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(DeleteUserAsync));

            ApiResponse res = await _userBAL.DeleteUserAsync(userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(DeleteUserAsync));

            return NoContent();
        }
    }
}
