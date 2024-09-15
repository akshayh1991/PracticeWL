using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;

namespace UserAccessManagement.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IPasswordBL _passwordBL;

        public AuthController(IPasswordBL passwordBL)
        {
            _passwordBL = passwordBL;
        }
        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            var response= _passwordBL.UpdatePasswordAsync(changePassword.oldPassword, changePassword.newPassword);
            if (response.ToString()!=null)
            {
                return NoContent();
            }
            return BadRequest(response);
        }

        /// <summary>
        /// Reset Password Link Validation
        /// </summary>
        /// <param name="token" example="abc123def456">Hashed Token with username and password for the given email"</param>
        /// <param name="email" example="john.doe@acme.com">Email ID of the user</param>
        /// <returns></returns>
        //[HttpGet("reset-password")]
        //public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromRoute] string email)
        //{
        //    return Ok();
        //}

        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        //{
        //    return Ok();
        //}

        //[HttpPost]
        //[Route("GenerateJwtToken")]
        //public async Task<IActionResult> GenerateToken([FromBody] JwtTokenRequestDto jwtTokenRequestDto)
        //{
        //    var token = _tokenRepositoryBL.CreateJwtToken(jwtTokenRequestDto.Username);
        //    var response = new JwtTokenResponseDto
        //    {
        //        Token = token,
        //        UserName = jwtTokenRequestDto.Username,
        //    };
        //    return Ok(response);
        //}
    }
}
