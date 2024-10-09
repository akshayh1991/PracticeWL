using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.BL;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// Auth controller
    /// </summary>
    [Route("auth")]
    [ApiController]
    
    public class AuthController : ControllerBase
    {
        private readonly IPasswordBl _passwordBL;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthBL _authBL;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="passwordBL"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="httpContext"></param>
        /// <param name="authBL"></param>
        public AuthController(IPasswordBl passwordBL, 
                              IConfiguration configuration,
                              ILogger<AuthController> logger,
                              IHttpContextAccessor httpContext, IAuthBL authBL)
        {
            _passwordBL = passwordBL;
            _configuration = configuration;
            _logger = logger;
            _authBL = authBL;
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            _logger.LogInformation("Change Password method called with request: {@Request}", changePassword);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user: {Username}. Errors: {@Errors}", changePassword.userName, ModelState);
                return BadRequest(ModelState);
            }

            if (changePassword == null)
            {
                _logger.LogWarning("ChangePassword called with null request.");
                return BadRequest("Invalid request.");
            }

            var result = await _passwordBL.UpdatePasswordAsync(changePassword.userName, changePassword.oldPassword, changePassword.newPassword);

            if (result != null)
            {
                _logger.LogInformation("Password changed successfully for user: {Username}", changePassword.userName);
                return NoContent();
            }

            _logger.LogWarning("Password change failed for user: {Username}", changePassword.userName);
            return BadRequest("Password change failed.");
        }


        /// <summary>
        /// Forget Password
        /// </summary>
        /// <param name="forgetPasswordDto"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            try
            {
                var response = await _passwordBL.ForgetPasswordAsync(forgetPasswordDto.userName);

                if (response != null && response.link != null)
                {
                    return NoContent(); 
                }

                return BadRequest("Failed to generate password reset link."); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the forget password request for user: {Username}", forgetPasswordDto.userName);
                return StatusCode(500, "Internal server error. Please try again later."); 
            }
        }


        /// <summary>
        /// Reset Password Link Validation
        /// </summary>
        /// <param name="token" example="abc123def456">Hashed Token with username and password for the given email</param>
        /// <param name="email" example="john.doe@acme.com">Email ID of the user</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery][Required] string token,[FromQuery][Required] string email)
        {
            _logger.LogInformation("ResetPassword method called for email: {Email}", email);

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token is null or empty.");
                return BadRequest("Token is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email is null or empty.");
                return BadRequest("Email is required.");
            }

            string? jwtToken = null;
            if (Request.Headers.TryGetValue("Authorization", out var authHeader) &&
                AuthenticationHeaderValue.TryParse(authHeader, out var parsedHeader) &&
                parsedHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                jwtToken = parsedHeader.Parameter; 
            }

            if (jwtToken != null)
            {
                _logger.LogInformation("JWT token extracted: {JwtToken}", jwtToken);
            }
            else
            {
                _logger.LogWarning("JWT token not found in the request.");
            }

            bool response = await _passwordBL.GetUserNamePasswordAsync(email, token);

            if (response)
            {
                string? baseURL = _configuration["ResetPassword:RedirectURL"];
                var redirectURL = $"{baseURL}?token={jwtToken}";
                _logger.LogInformation("Reset password link generated successfully for email: {Email}", email);
                return Redirect(redirectURL);
            }

            _logger.LogWarning("Reset password validation failed for email: {Email}", email);
            return BadRequest("Invalid token or email.");
        }



        /// <summary>
        /// Reset Password
        /// </summary>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, [FromHeader] string authorization)
        {
            _logger.LogInformation("ResetPassword method called.");

            if (resetPasswordDto == null)
            {
                _logger.LogWarning("ResetPasswordDto is null.");
                return BadRequest("Invalid request.");
            }

            if (string.IsNullOrWhiteSpace(resetPasswordDto.newPassword))
            {
                _logger.LogWarning("New password is null or empty.");
                return BadRequest("New password is required.");
            }


            bool isSuccess = await _passwordBL.CheckForHashedToken(authorization, resetPasswordDto.newPassword);

            if (isSuccess)
            {
                _logger.LogInformation("Password reset successfully.");
                return Ok("Password reset successfully.");
            }

            _logger.LogWarning("Password reset failed due to invalid token.");
            return BadRequest("Invalid token or password reset failed.");
        }


        #region Pavan's Code
        /// <summary>
        /// User Authentication and Token Generation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<IActionResult> LoginAsync(LoginRequest model)
        {
            Log.Information("Starting Login API's controller Method with input {@LoginRequest}", model);

            ServiceResponse<LoginServiceResponse> res = await _authBL.LoginAsync(model);
            Log.Information("Completed BAL's LoginAsync Method Execution, the response received is : {@Response}", res);

            if (res.StatusCode == HttpStatusCode.OK && res.Data != null)
            {
                Log.Information("validating if session id null");
                if (res.Data.SSOSessionId != null)
                {
                    CookieOptions cookieOptions = new CookieOptions
                    {

                        Domain = "localhost",
                        Path = "/",
                        HttpOnly = false,
                        Secure = false,
                        SameSite = SameSiteMode.None,
                        Expires = DateTimeOffset.Now.AddMinutes(res.Data.ExpiresIn)
                    };
                    Log.Information("Session coockie is created with coockie options : {@CoockieOptions}", cookieOptions);
                    Response.Cookies.Append(ResponseHeaders.SSOSessionId, res.Data.SSOSessionId, cookieOptions);
                }

                Log.Information("Completed Login API's controller Method");
                return Ok(new LoginResponse
                {
                    Token = res.Data.Token,
                    ExpiresIn = DateTimeOffset.Now.AddMinutes(res.Data.ExpiresIn)
                });
            }
            Log.Information("Varified response received for BAL Method is Unauthorized, Returning Unauthorized Response");

            Log.Information("Completed Login API's controller Method");
            return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, res.Message));
        }


        /// <summary>
        /// Validate Session
        /// </summary>
        /// <param name="ssoSessionId"></param>
        /// <returns></returns>
        [HttpPost("validate-session")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<IActionResult> ValidateSessionAsync([FromHeader] string ssoSessionId)
        {
            Log.Information("Starting ValidateSession API's controller Method with input {@SSOSessionId}", ssoSessionId);

            Log.Information("Check if the input session is null or white space");
            if (string.IsNullOrWhiteSpace(ssoSessionId))
            {
                Log.Information("Completed ValidateSession API's controller Method");
                return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.InvalidSessionId));
            }
            ServiceResponse<LoginServiceResponse> res = await _authBL.ValidateSessionAsync(ssoSessionId);
            Log.Information("Completed BAL's ValidateSessionAsync Method Execution, the response received is : {@Response}", res);

            if (res.StatusCode == HttpStatusCode.OK && res.Data != null)
            {
                Log.Information("validating if session id null");
                if (res.Data.SSOSessionId != null)
                {
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Domain = "localhost",
                        Path = "/",
                        HttpOnly = false,
                        Secure = false,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.Now.AddMinutes(res.Data.ExpiresIn)
                    };
                    Log.Information("Session coockie is created with coockie options : {@CoockieOptions}", cookieOptions);
                    Response.Cookies.Append("SSO_SESSION_ID", res.Data.SSOSessionId, cookieOptions);
                }
                Log.Information("Completed Login API's controller Method");
                return Ok(new LoginResponse
                {
                    Token = res.Data.Token,
                    ExpiresIn = DateTimeOffset.Now.AddMinutes(res.Data.ExpiresIn)
                });
            }
            Log.Information("Varified response received for BAL Method is Unauthorized, Returning Unauthorized Response");

            Log.Information("Completed Login API's controller Method");
            return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, res.Message));
        }
        #endregion

    }
}
