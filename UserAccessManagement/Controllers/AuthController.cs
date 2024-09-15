using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net.Mail;

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
        #region PasswordResetLink

       
        //[HttpGet("reset-password1")]
        //public async Task<IActionResult> ResetPassword1([FromQuery] string token, [FromQuery] string email)
        //{
        //    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        //    {
        //        return BadRequest("Token and email are required.");
        //    }

        //    // Generate the password reset link.
        //    var resetLink = GeneratePasswordResetLink(token, email);

        //    // Send the password reset email.
        //    await SendPasswordResetEmailAsync(email, resetLink);

        //    return Ok("Password reset link sent.");
        //}

        //private async Task SendPasswordResetEmailAsync(string email, string resetLink)
        //{
        //    var message = new MailMessage
        //    {
        //        From = new MailAddress("akshayhuded777@gmail.com"), // Ensure this email is allowed to send emails through your SMTP server
        //        Subject = "Password Reset Request",
        //        Body = $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>.",
        //        IsBodyHtml = true
        //    };
        //    message.To.Add(email);

        //    try
        //    {
        //        using (var client = new SmtpClient("smtp.gmail.com", 587)) // Gmail SMTP server address and port
        //        {
        //            client.Credentials = new System.Net.NetworkCredential("watlow751@gmail.com", "Watlow@123"); // Your email and password
        //            client.EnableSsl = true; // Use SSL/TLS
        //            await client.SendMailAsync(message);
        //        }
        //    }
        //    catch (SmtpException smtpEx)
        //    {
        //        // Handle SMTP exceptions
        //        Console.WriteLine($"SMTP error: {smtpEx.Message}");
        //        // You may log the error or rethrow as needed
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle general exceptions
        //        Console.WriteLine($"General error: {ex.Message}");
        //        // You may log the error or rethrow as needed
        //    }
        //}


        //private string GeneratePasswordResetLink(string token, string email)
        //{
        //    // Adjust the URL action name and controller name as needed
        //    var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = email }, Request.Scheme);
        //    return resetLink;
        //}
        #endregion

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
