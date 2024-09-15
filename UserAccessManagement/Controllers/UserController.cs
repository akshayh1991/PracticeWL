using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using static SecMan.Model.User;

namespace UserAccessManagement.Controllers
{
    [ApiController]
    [Route("users")]
    [ProducesResponseType(typeof(BadRequestResponse), 400)]
    [ProducesResponseType(typeof(CommonResponse), 500)]
    [ProducesResponseType(typeof(CommonResponse), 401)]
    [ProducesResponseType(typeof(CommonResponse), 403)]
    public class UsersController : ControllerBase
    {
        private readonly IUserBL _userBAL;

        public UsersController(IUserBL userBAL)
        {
            _userBAL = userBAL;
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(typeof(CommonResponse), 409)]
        [SwaggerOperation(Summary = "Create User")]
        public async Task<ActionResult> AddUserAsync(AddUserDto model)
        {
            Log.Information("Started API {APIName}", nameof(AddUserAsync));
            var res = await _userBAL.AddUserAsync(model);

            if (res.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new CommonResponse(ResponseConstants.Conflict, HttpStatusCode.Conflict, res.Message));

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequestResponse(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(AddUserAsync));

            return Created(nameof(AddUserAsync), res.Data);
        }


        [HttpGet("GetUser")]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [ProducesResponseType(typeof(CommonResponse), 204)]
        [SwaggerOperation(Summary = "List Users")]
        public async Task<ActionResult> GetUsersAsync([FromQuery] UsersFilterDto model)
        {
            Log.Information("Started API {APIName}", nameof(GetUsersAsync));
            var res = await _userBAL.GetUsersAsync(model);

            Log.Information("Adding total user count to response headers,User Count {usercount}", res.Data.UserCount);
            Response.Headers.Append(ResponseHeaders.TotalCount, res.Data.UserCount.ToString());

            if (res.Data.Users?.Count <= 0)
                return NoContent();

            Log.Information("Finishing API {APIName}", nameof(GetUsersAsync));

            return Ok(res.Data.Users);
        }


        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [SwaggerOperation(Summary = "Enquire user by id")]
        public async Task<ActionResult> GetUserByIdAsync(ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(GetUserByIdAsync));

            var res = await _userBAL.GetUserByIdAsync(userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequestResponse(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(GetUserByIdAsync));

            return Ok(res.Data);
        }


        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [SwaggerOperation(Summary = "Update user by id")]
        public async Task<ActionResult> UpdateUserAsync(AddUserDto model, ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(UpdateUserAsync));

            var res = await _userBAL.UpdateUserAsync(model, userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequestResponse(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Finishing API {APIName}", nameof(UpdateUserAsync));

            return Ok(res.Data);
        }


        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [SwaggerOperation(Summary = "Delete user by id")]
        public async Task<ActionResult> DeleteUserAsync(ulong userId)
        {
            Log.Information("Started API {APIName}", nameof(DeleteUserAsync));

            var res = await _userBAL.DeleteUserAsync(userId);

            if (res.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new BadRequestResponse(ResponseConstants.InvalidRequest, res.Message));

            Log.Information("Started API {APIName}", nameof(DeleteUserAsync));

            return NoContent();
        }
    }
}
